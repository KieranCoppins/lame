using System.Windows;
using System.Windows.Controls;

namespace Lame.Frontend.Views;

public partial class AssetDetailsView : UserControl
{
    public AssetDetailsView()
    {
        InitializeComponent();
    }

    private void EditTitleButton_Click(object sender, RoutedEventArgs e)
    {
        InternalNameText.StartEditing();
    }

    private void EditContextButton_Click(object sender, RoutedEventArgs e)
    {
        ContextNotesText.StartEditing();
    }
}