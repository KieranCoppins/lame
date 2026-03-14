namespace Lame.Backend.EntityFramework.Models;

public class TargetAssetTranslationEntity
{
    public Guid AssetId { get; set; }
    public string Language { get; set; }
    public Guid TranslationId { get; set; }

    // Navigation Properties
    public AssetEntity Asset { get; set; }
    public TranslationEntity Translation { get; set; }
}