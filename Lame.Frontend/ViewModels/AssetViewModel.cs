using Lame.DomainModel;
using Lame.Frontend.Enums;

namespace Lame.Frontend.ViewModels;

public class AssetViewModel : PageViewModel
{
    public readonly int SupportedLanguagesCount;

    public AssetViewModel(AssetDto asset, int SupportedLanguagesCount)
    {
        this.SupportedLanguagesCount = SupportedLanguagesCount;
        Asset = asset;
        Page = AppPage.Library;
    }

    public AssetDto Asset { get; }

    public string InternalName => Asset.InternalName;

    public string AssetType => Asset.AssetType.ToString();

    // TODO set the total number of supported translations somewhere globally through some config
    public string Progress => $"{Asset.NumTranslations} / {SupportedLanguagesCount} ";

    public float ProgressPercentage =>
        SupportedLanguagesCount == 0 ? 0 : (float)Asset.NumTranslations / SupportedLanguagesCount;

    public string LastModified => Asset.LastUpdatedAt.ToString("yyyy-MM-dd");

    public string CreatedAt => Asset.CreatedAt.ToString("yyyy-MM-dd");

    public string? ContentContext => Asset.ContextNotes;

    public override string ToString()
    {
        return InternalName;
    }
}