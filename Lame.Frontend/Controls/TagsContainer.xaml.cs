using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Lame.DomainModel;
using Lame.Frontend.Helpers;

namespace Lame.Frontend.Controls;

public partial class TagsContainer : UserControl, INotifyPropertyChanged
{
    public static readonly DependencyProperty TagsProperty =
        DependencyProperty.Register(
            nameof(Tags),
            typeof(ObservableCollection<Tag>),
            typeof(TagsContainer),
            new FrameworkPropertyMetadata(new ObservableCollection<Tag>(),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTagsChanged));

    public static readonly DependencyProperty GetTagsProperty =
        DependencyProperty.Register(
            nameof(GetTags),
            typeof(Func<Task<List<Tag>>>),
            typeof(TagsContainer),
            new PropertyMetadata(OnGetTagsChanged));

    private readonly Brush _buttonBrush;

    private readonly Brush _buttonHoverBrush;


    private readonly DispatcherTimer _stoppedEditingDebounceTimer;

    public TagsContainer()
    {
        InitializeComponent();

        AvailableTags = [];

        _buttonBrush = RootButton.Background;
        _buttonHoverBrush = ButtonHelpers.GetHoverBackground(RootButton);

        _stoppedEditingDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _stoppedEditingDebounceTimer.Tick += StoppedEditingDebounceTimer_Tick;
    }

    public bool IsEditing
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableCollection<Tag> Tags
    {
        get => (ObservableCollection<Tag>)GetValue(TagsProperty);
        set => SetValue(TagsProperty, value);
    }

    public Func<Task<List<Tag>>> GetTags
    {
        get => (Func<Task<List<Tag>>>)GetValue(GetTagsProperty);
        set => SetValue(GetTagsProperty, value);
    }

    public ObservableCollection<Tag> AvailableTags { get; set; }

    public IEnumerable<Tag> AvailableFilteredTags => string.IsNullOrWhiteSpace(TagText)
        ? AvailableTags
        : AvailableTags
            .Where(t => t.Name.Contains(TagText, StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(t => t.Name.Equals(TagText, StringComparison.CurrentCultureIgnoreCase) ? 0 :
                t.Name.StartsWith(TagText, StringComparison.CurrentCultureIgnoreCase) ? 1 : 2)
            .ThenBy(t => t.Name);


    public Tag? SelectedAvailableTag { get; set; }

    public string TagText
    {
        get;
        set
        {
            if (!SetField(ref field, value)) return;
            OnPropertyChanged(nameof(AvailableFilteredTags));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void StoppedEditingDebounceTimer_Tick(object? sender, EventArgs e)
    {
        IsEditing = false;
    }

    private static void OnGetTagsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (TagsContainer)d;
        control.GetTags = (Func<Task<List<Tag>>>)e.NewValue;

        _ = control.LoadTags();
    }

    private void Tags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
            // Items added to Tags => remove them from AvailableTags
        {
            foreach (Tag addedTag in e.NewItems)
            {
                var toRemove = AvailableTags.FirstOrDefault(t => t.Id == addedTag.Id);
                if (toRemove != null)
                    AvailableTags.Remove(toRemove);
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            // Items removed from Tags => add them back to AvailableTags
            foreach (Tag removedTag in e.OldItems)
                if (AvailableTags.All(t => t.Id != removedTag.Id))
                    AvailableTags.Add(removedTag);
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            // Reload the tags
            _ = LoadTags();
        }

        // Flash the popup so that it moves in case we have overflowed to a new line
        AvailableTagsPopup.IsOpen = false;
    }

    private static void OnTagsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (TagsContainer)d;

        // Unsubscribe from the old collection
        if (e.OldValue is ObservableCollection<Tag> oldCollection)
            oldCollection.CollectionChanged -= control.Tags_CollectionChanged;

        // Subscribe to the new collection
        if (e.NewValue is ObservableCollection<Tag> newCollection)
            newCollection.CollectionChanged += control.Tags_CollectionChanged;
    }

    private async Task LoadTags()
    {
        var tags = await GetTags();
        AvailableTags.Clear();
        foreach (var tag in tags)
            if (Tags.All(t => t.Id != tag.Id))
                AvailableTags.Add(tag);
    }

    private void EditTagsButton_Clicked(object sender, RoutedEventArgs e)
    {
        BeginEditing();
    }

    private void BeginEditing()
    {
        _stoppedEditingDebounceTimer.Stop();
        RootButton.Background = _buttonHoverBrush;
        TagTextBox.Visibility = Visibility.Visible;
        AvailableTagsPopup.IsOpen = AvailableTags.Count > 0;
        TagTextBox.Focus();
        IsEditing = true;
    }

    private void EndEditing()
    {
        RootButton.Background = _buttonBrush;
        TagTextBox.Visibility = Visibility.Collapsed;
        AvailableTagsPopup.IsOpen = false;

        // Clicking off the text box to either select an available tag or to remove a tag will cause
        // the lost focus event to fire and stop editing. It will then immediately be followed by the selection changed
        // event which will begin editing again. Or if the user is going to click a button, it will cause us to stop
        // editing and then the button will collapse before the click event can fire. To prevent this,
        // we will debounce stopping editing so that if we immediately begin editing again,
        // we won't have stopped editing in the first place
        _stoppedEditingDebounceTimer.Start();
    }

    private void CreateTag(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName) || Tags.FirstOrDefault(t => t.Name == tagName) != null) return;

        var existingTag = AvailableTags.FirstOrDefault(t => t.Name == tagName);
        var tag = existingTag ?? new Tag { Name = tagName, Id = Guid.NewGuid() };

        if (Tags.All(t => t.Id != tag.Id))
            Tags.Add(tag);
    }

    private void TagTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            // Create tag and stop editing
            CreateTag(TagTextBox.Text);
            TagTextBox.Text = string.Empty;
            EndEditing();
            e.Handled = true;
        }

        else if (e.Key == Key.Escape)
        {
            // Stop editing without creating tag
            TagTextBox.Text = string.Empty;
            EndEditing();
            e.Handled = true;
        }

        else if (e.Key == Key.OemComma)
        {
            // Create tag and allow for more tags to be added
            CreateTag(TagTextBox.Text);
            TagTextBox.Text = string.Empty;
            AvailableTagsPopup.IsOpen = AvailableTags.Count > 0;
            e.Handled = true;
        }

        else if (e.Key == Key.Back && string.IsNullOrEmpty(TagTextBox.Text) && Tags.Count > 0)
        {
            // Delete the last tag if we are backspacing on an empty input
            Tags.RemoveAt(Tags.Count - 1);
            AvailableTagsPopup.IsOpen = AvailableTags.Count > 0;
        }
    }

    private void TagTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        TagTextBox.Text = string.Empty;
        EndEditing();
    }

    private void AvailableTagsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectedAvailableTag != null)
            // Add tag to Tags collection
            if (Tags.All(t => t.Id != SelectedAvailableTag.Id))
            {
                Tags.Add(SelectedAvailableTag);
                TagTextBox.Text = string.Empty;
                BeginEditing();
            }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void RemoveTagButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Tag tag)
            Tags.Remove(tag);
    }
}