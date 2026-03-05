using System.Windows;

namespace Lame.Frontend.Helpers;

public static class CommonHelpers
{
    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.RegisterAttached(
            "IsLoading",
            typeof(bool),
            typeof(CommonHelpers),
            new PropertyMetadata(false)
        );

    public static void SetIsLoading(UIElement element, bool value)
    {
        element.SetValue(IsLoadingProperty, value);
    }

    public static bool GetIsLoading(UIElement element)
    {
        return (bool)element.GetValue(IsLoadingProperty);
    }
}