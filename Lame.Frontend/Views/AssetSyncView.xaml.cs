using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lame.Frontend.Views;

public partial class AssetSyncView : UserControl
{
    public AssetSyncView()
    {
        InitializeComponent();
    }

    private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Handled) return;

        e.Handled = true;

        var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = MouseWheelEvent,
            Source = sender
        };

        var parent = ((Control)sender).Parent as UIElement;
        parent?.RaiseEvent(eventArg);
    }
}