using System.ComponentModel;
using System.Windows.Input;
using Lame.DomainModel;

namespace Lame.Frontend.ViewModels;

public class AssetViewModel : BaseViewModel
{
    public AssetDto Asset { get; }
    
    public string InternalName => Asset.InternalName;
    
    public string AssetType => Asset.AssetType.ToString();
    
    // TODO set the total number of supported translations somewhere globally through some config
    public string Progress => $"{Asset.NumTranslations} / 15 ";

    public string LastModified => Asset.LastUpdatedAt.ToString("yyyy-mm-dd");
    
    public string CreatedAt => Asset.CreatedAt.ToString("yyyy-mm-dd");
    
    public string ContentContext => Asset.ContextNotes ?? "No context provided";
    
    public List<string> Tags => ["main-quest", "chapter-1", "intro", "quest"];

    public AssetViewModel(AssetDto asset)
    {
        Asset = asset;
    }
}