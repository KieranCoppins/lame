using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Lame.Frontend.Controls;

// TODO: This control fights against what combo box should be quite a lot. It has a few bugs. We should consider
// creating our own control for more control on what we want it to do. However, this was easy to keyboard navigation
// ultimately we would probably use some third party library but that's out of scope of this project
public partial class DynamicLoadingComboBox : ComboBox
{
    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(DynamicLoadingComboBox),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ContentSearchProperty =
        DependencyProperty.Register(
            nameof(ContentSearch),
            typeof(Func<string, Task<IEnumerable>>),
            typeof(DynamicLoadingComboBox),
            new PropertyMetadata(null, OnContentSearchChanged));

    public DynamicLoadingComboBox()
    {
        InitializeComponent();
        IsTextSearchEnabled = false;
        StaysOpenOnEdit = true;
        IsEditable = true;
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string SearchText
    {
        get;
        set
        {
            field = value;

            if (ItemsSource != null && ItemsSource.Cast<object>().Any(i => i.ToString() == SearchText)) return;

            _ = SearchContent();
        }
    }

    public Func<string, Task<IEnumerable>> ContentSearch
    {
        get => (Func<string, Task<IEnumerable>>)GetValue(ContentSearchProperty);
        set => SetValue(ContentSearchProperty, value);
    }

    private static void OnContentSearchChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        var control = (DynamicLoadingComboBox)d;
        control.ContentSearch = (Func<string, Task<IEnumerable>>)e.NewValue;
    }

    private async Task SearchContent()
    {
        if (string.IsNullOrEmpty(SearchText))
            return;

        SelectedItem = null;
        ItemsSource = await ContentSearch(SearchText);
        IsDropDownOpen = true;
    }
}