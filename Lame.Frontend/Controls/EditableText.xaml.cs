using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lame.Frontend.Controls;

public partial class EditableText : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(EditableText),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTextChanged
            ));

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(EditableText),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
            ));

    public EditableText()
    {
        InitializeComponent();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (EditableText)d;
        control.Text = (string)e.NewValue;

        if (string.IsNullOrWhiteSpace(control.Text))
            control.PlaceholderTextBlock.Visibility = Visibility.Visible;
        else
            control.PlaceholderTextBlock.Visibility = Visibility.Collapsed;
    }

    public void StartEditing()
    {
        PlaceholderTextBlock.Visibility = Visibility.Collapsed;
        DisplayText.Visibility = Visibility.Collapsed;

        EditBox.Visibility = Visibility.Visible;
        Underline.Visibility = Visibility.Visible;

        EditBox.Focus();
        EditBox.SelectAll();
    }

    public void StopEditing()
    {
        DisplayText.Visibility = Visibility.Visible;
        EditBox.Visibility = Visibility.Collapsed;
        Underline.Visibility = Visibility.Collapsed;
    }

    private void EditBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) StopEditing();

        if (e.Key == Key.Escape) StopEditing();
    }

    private void EditBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        StopEditing();
    }
}