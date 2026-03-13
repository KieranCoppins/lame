using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Lame.Frontend.ViewModels;

namespace Lame.Frontend.Views;

public partial class AssetDetailsView : UserControl
{
    public AssetDetailsView()
    {
        InitializeComponent();

        AssetTagsContainer.PropertyChanged += TagsContainer_PropertyChanged;
    }

    private void TagsContainer_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AssetTagsContainer.IsEditing) &&
            !AssetTagsContainer.IsEditing &&
            DataContext is AssetDetailsViewModel vm)
            _ = vm.UpdateAssetTags();
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