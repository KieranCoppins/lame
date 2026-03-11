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
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public EditableText()
    {
        InitializeComponent();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private void StartEditing()
    {
        EditButton.Visibility = Visibility.Collapsed;
        DisplayText.Visibility = Visibility.Collapsed;

        EditBox.Visibility = Visibility.Visible;
        Underline.Visibility = Visibility.Visible;
        ConfirmationButtons.Visibility = Visibility.Visible;

        EditBox.Focus();
        EditBox.SelectAll();
    }

    private void StopEditing()
    {
        EditButton.Visibility = Visibility.Visible;
        DisplayText.Visibility = Visibility.Visible;

        EditBox.Visibility = Visibility.Collapsed;
        Underline.Visibility = Visibility.Collapsed;
        ConfirmationButtons.Visibility = Visibility.Collapsed;
    }

    private void EditBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) StopEditing();

        if (e.Key == Key.Escape) StopEditing();
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        StartEditing();
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        StopEditing();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO restore original text
        StopEditing();
    }
}