using System.Collections.ObjectModel;
using System.Windows.Input;
using Lame.Backend.Assets;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Frontend.ViewModels;

public class AssetLibraryViewModel : PageViewModel
{
    private readonly IAssets _assets;
    private readonly INavigationService _navigationService;
    private readonly IServiceProvider _serviceProvider;

    public AssetLibraryViewModel(IAssets assets, INavigationService navigationService, IServiceProvider serviceProvider)
    {
        _assets = assets;
        _navigationService = navigationService;
        _serviceProvider = serviceProvider;

        Assets = [];

        ViewAssetDetailsCommand = new RelayCommand<AssetViewModel>(OnViewAssetDetails);
        Page = AppPage.Library;
    }

    public bool IsLoading
    {
        get;
        private set => SetField(ref field, value);
    }

    public ObservableCollection<AssetViewModel> Assets { get; }

    public ICommand ViewAssetDetailsCommand { get; }

    public override void OnNavigatedTo()
    {
        base.OnNavigatedTo();
        _ = LoadAssets();
    }

    private async Task LoadAssets()
    {
        IsLoading = true;
        try
        {
            var assets = await _assets.Get();
            Assets.Clear();
            foreach (var asset in assets) Assets.Add(new AssetViewModel(asset));
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnViewAssetDetails(AssetViewModel asset)
    {
        _navigationService.NavigateTo(() =>
            ActivatorUtilities.CreateInstance<AssetDetailsViewModel>(_serviceProvider, asset.Asset));
    }
}