using System.Windows;

namespace Lame.Frontend.Helpers;

public static class TextBoxHelpers
{
    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.RegisterAttached(
            "Placeholder",
            typeof(string),
            typeof(TextBoxHelpers),
            new PropertyMetadata(string.Empty));

    public static void SetPlaceholder(DependencyObject element, string value)
        => element.SetValue(PlaceholderProperty, value);

    public static string GetPlaceholder(DependencyObject element)
        => (string)element.GetValue(PlaceholderProperty);
}