using System.Collections.ObjectModel;
using Lame.Backend.Assets;
using Lame.DomainModel;

namespace Lame.Frontend.ViewModels;

public class AssetLibraryViewModel
{
    public ObservableCollection<AssetViewModel> Assets { get; set; }
    
    private IAssets _assets;

    public AssetLibraryViewModel(IAssets assets)
    {
        _assets = assets;

        Assets = [];
    }
    
    public async void LoadAssets()
    {
        CreateDummyData();
        
        var assets = await _assets.Get();
        Assets.Clear();
        foreach (var asset in assets)
        {
            Assets.Add(new AssetViewModel(asset));
        }
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