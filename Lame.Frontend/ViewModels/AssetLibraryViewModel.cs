using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using Lame.Backend.Assets;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Frontend.ViewModels;

public class AssetLibraryViewModel : BaseViewModel
{
    public ObservableCollection<AssetViewModel> Assets { get; }
    
    public ICommand ViewAssetDetailsCommand { get; }
    
    private readonly IAssets _assets;
    private readonly INavigationService _navigationService;
    private readonly IServiceProvider _serviceProvider;

    public AssetLibraryViewModel(IAssets assets, INavigationService navigationService, IServiceProvider serviceProvider)
    {
        _assets = assets;
        _navigationService = navigationService;
        _serviceProvider = serviceProvider;

        _navigationService.CurrentViewModelChanged += async () =>
        {
            if (_navigationService.CurrentViewModel == this)
            {
                await LoadAssets();
            }
        };

        ViewAssetDetailsCommand = new RelayCommand<AssetViewModel>(OnViewAssetDetails);
        
        Assets = [];
    }
    
    public async Task LoadAssets()
    {
        var assets = await _assets.Get();
        Assets.Clear();
        foreach (var asset in assets)
        {
            Assets.Add(new AssetViewModel(asset));
        }
    }

    private void OnViewAssetDetails(AssetViewModel asset)
    {
        _navigationService.NavigateTo(() => ActivatorUtilities.CreateInstance<AssetDetailsViewModel>(_serviceProvider, asset.Asset));
    }

    private void CreateDummyData()
    {
        Task.WaitAll([
        _assets.Create(new Asset { Id = Guid.NewGuid(), InternalName = "ui_main_menu_title", AssetType = AssetType.Text }),
        _assets.Create(new Asset { Id = Guid.NewGuid(), InternalName = "dialog_quest_01_intro", AssetType = AssetType.Text }),
        _assets.Create(new Asset { Id = Guid.NewGuid(), InternalName = "voice_quest_01_intro", AssetType = AssetType.Audio }),
        _assets.Create(new Asset { Id = Guid.NewGuid(), InternalName = "ui_settings_graphics", AssetType = AssetType.Text }),
        _assets.Create(new Asset { Id = Guid.NewGuid(), InternalName = "ui_inventory_empty", AssetType = AssetType.Text }),
        ]);
    }
}