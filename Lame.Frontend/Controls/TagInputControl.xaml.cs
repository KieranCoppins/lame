using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Lame.DomainModel;
using Lame.Frontend.ViewModels;

namespace Lame.Frontend.Controls;

public partial class TagInputControl : UserControl
{
    public static readonly DependencyProperty SelectedTagsProperty =
        DependencyProperty.Register(
            nameof(SelectedTags),
            typeof(ObservableCollection<TagViewModel>),
            typeof(TagInputControl),
            new PropertyMetadata(null, OnSelectedTagsChanged));

    public static readonly DependencyProperty TagSearchProperty =
        DependencyProperty.Register(
            nameof(TagSearch),
            typeof(Func<string, Task<List<Tag>>>),
            typeof(TagInputControl),
            new PropertyMetadata(null, OnTagSearchChanged));

    public TagInputControl()
    {
        InitializeComponent();
        TagSearchWrapper = async searchText =>
            (await TagSearch(searchText))
            .Select(tag => new TagViewModel(tag));
    }

    public Func<string, Task<IEnumerable>> TagSearchWrapper { get; set; }

    public TagViewModel? SelectedTag { get; set; }

    public ObservableCollection<TagViewModel> SelectedTags
    {
        get => (ObservableCollection<TagViewModel>)GetValue(SelectedTagsProperty);
        set => SetValue(SelectedTagsProperty, value);
    }

    public Func<string, Task<List<Tag>>> TagSearch
    {
        get => (Func<string, Task<List<Tag>>>)GetValue(TagSearchProperty);
        set => SetValue(TagSearchProperty, value);
    }

    private static void OnSelectedTagsChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        var control = (TagInputControl)d;
        control.SelectedTags = (ObservableCollection<TagViewModel>)e.NewValue;
    }

    private static void OnTagSearchChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        var control = (TagInputControl)d;
        control.TagSearch = (Func<string, Task<List<Tag>>>)e.NewValue;
    }

    private async Task AddTag(string name)
    {
        var result = await TagSearch(name);
        if (result.Count > 0 && result[0].Name == name)
        {
            AddTag(new TagViewModel(result[0]));
            return;
        }

        var existingTag = SelectedTags.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        var tag = existingTag ?? new TagViewModel(new Tag { Name = name, Id = Guid.NewGuid() });
        AddTag(tag);
    }

    private void AddTag(TagViewModel tag)
    {
        // If we have already added this tag, skip it
        if (SelectedTags.Any(t => tag.Tag.Id == t.Tag.Id))
            return;

        SelectedTags.Add(tag);
    }

    private void RemoveTag(TagViewModel tag)
    {
        if (SelectedTags?.Contains(tag) != true)
            return;

        SelectedTags.Remove(tag);
    }

    private void RemoveTag_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is TagViewModel tag) RemoveTag(tag);
    }

    private void TagComboBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CommitTagFromText();
            e.Handled = true;
        }
        else if (e.Key == Key.OemComma)
        {
            CommitTagFromText();
            e.Handled = true;
        }
    }

    private void CommitTagFromText()
    {
        if (string.IsNullOrWhiteSpace(TagComboBox.SearchText)) return;

        var parts = TagComboBox.SearchText
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim());

        foreach (var part in parts)
            _ = AddTag(part);

        TagComboBox.SearchText = "";

        if (TagComboBox.SelectedItem != null)
            TagComboBox.SelectedItem = null;
    }
}