using System.Windows;
using System.Windows.Media;

namespace Lame.Frontend.Helpers;

public static class ButtonHelpers
{
    public static readonly DependencyProperty HoverBackgroundProperty =
        DependencyProperty.RegisterAttached(
            "HoverBackground",
            typeof(Brush),
            typeof(ButtonHelpers),
            new PropertyMetadata(default(Brush))
        );

    public static void SetHoverBackground(UIElement element, Brush value)
    {
        element.SetValue(HoverBackgroundProperty, value);
    }

    public static Brush GetHoverBackground(UIElement element)
    {
        return (Brush)element.GetValue(HoverBackgroundProperty);
    }
}