using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Lame.DomainModel;

namespace Lame.Frontend.Controls;

public partial class TagInputControl : UserControl
{
    public static readonly DependencyProperty SelectedTagsProperty =
        DependencyProperty.Register(
            nameof(SelectedTags),
            typeof(ObservableCollection<Tag>),
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
        TagSearchWrapper = async searchText => await TagSearch(searchText);
    }

    public Func<string, Task<IEnumerable>> TagSearchWrapper { get; set; }

    public Tag? SelectedTag { get; set; }

    public ObservableCollection<Tag> SelectedTags
    {
        get => (ObservableCollection<Tag>)GetValue(SelectedTagsProperty);
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
        control.SelectedTags = (ObservableCollection<Tag>)e.NewValue;
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
            AddTag(result[0]);
            return;
        }

        var existingTag = SelectedTags.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        var tag = existingTag ?? new Tag { Name = name, Id = Guid.NewGuid() };
        AddTag(tag);
    }

    private void AddTag(Tag tag)
    {
        // If we have already added this tag, skip it
        if (SelectedTags.Any(t => tag.Id == t.Id))
            return;

        SelectedTags.Add(tag);
    }

    private void RemoveTag(Tag tag)
    {
        if (SelectedTags?.Contains(tag) != true)
            return;

        SelectedTags.Remove(tag);
    }

    private void RemoveTag_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Tag tag) RemoveTag(tag);
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