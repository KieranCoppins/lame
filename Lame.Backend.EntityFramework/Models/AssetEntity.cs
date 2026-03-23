using Lame.DomainModel;

namespace Lame.Backend.EntityFramework.Models;

public class AssetEntity : Asset, ITaggableEntity
{
    public ICollection<TranslationEntity> Translations { get; set; }
    public ICollection<AssetLinkEntity> LinkedTo { get; set; }
    public ICollection<AssetLinkEntity> LinkedFrom { get; set; }
    public ICollection<TargetAssetTranslationEntity> TargetedTranslations { get; set; }
    public ICollection<TagEntity> Tags { get; set; }
}