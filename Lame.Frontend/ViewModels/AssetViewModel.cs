using System.ComponentModel;
using System.Windows.Input;
using Lame.DomainModel;
using Lame.Frontend.Enums;

namespace Lame.Frontend.ViewModels;

public class AssetViewModel : PageViewModel
{
    public AssetDto Asset { get; }
    
    public string InternalName => Asset.InternalName;
    
    public string AssetType => Asset.AssetType.ToString();
    
    // TODO set the total number of supported translations somewhere globally through some config
    public string Progress => $"{Asset.NumTranslations} / 15 ";

    public string LastModified => Asset.LastUpdatedAt.ToString("yyyy-MM-dd");
    
    public string CreatedAt => Asset.CreatedAt.ToString("yyyy-MM-dd");
    
    public string ContentContext => Asset.ContextNotes ?? "No context provided";

    public AssetViewModel(AssetDto asset)
    {
        Asset = asset;
        Page = AppPage.Library;
    }
}