using Lame.DomainModel;
using Lame.Frontend.Enums;

namespace Lame.Frontend.ViewModels;

public class AssetViewModel : PageViewModel
{
    public AssetViewModel(AssetDto asset)
    {
        Asset = asset;
        Page = AppPage.Library;
    }

    public AssetDto Asset { get; }

    public string InternalName => Asset.InternalName;

    public string AssetType => Asset.AssetType.ToString();

    // TODO set the total number of supported translations somewhere globally through some config
    public string Progress => $"{Asset.NumTranslations} / 15 ";

    public string LastModified => Asset.LastUpdatedAt.ToString("yyyy-MM-dd");

    public string CreatedAt => Asset.CreatedAt.ToString("yyyy-MM-dd");

    public string ContentContext =>
        string.IsNullOrEmpty(Asset.ContextNotes) ? "No context provided" : Asset.ContextNotes;

    public override string ToString()
    {
        return InternalName;
    }
}