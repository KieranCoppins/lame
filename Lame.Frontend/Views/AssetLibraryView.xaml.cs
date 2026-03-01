using System.Windows.Controls;
using Lame.Backend.Assets;
using Lame.Frontend.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Frontend.Views;

public partial class AssetLibraryView : UserControl
{
    public AssetLibraryView()
    {
        InitializeComponent();
        var viewModel = new AssetLibraryViewModel(App.ServiceProvider.GetService<IAssets>());

        DataContext = viewModel;
        viewModel.LoadAssets();
    }
}