using Lame.DomainModel;

namespace Lame.Backend.EntityFramework.Models;

public class AssetEntity : Asset, ITaggableEntity
{
    public ICollection<TranslationEntity> Translations { get; set; }
    public ICollection<AssetEntity> LinkedContent { get; set; }
    public ICollection<TargetAssetTranslationEntity> TargetedTranslations { get; set; }
    public ICollection<TagEntity> Tags { get; set; }
}