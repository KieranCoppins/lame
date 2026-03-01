using Lame.DomainModel;

namespace Lame.Backend.EntityFramework.Models;

public class AssetEntity : Asset
{
    public ICollection<TranslationEntity> Translations { get; set; }
    public ICollection<AssetEntity> LinkedContent { get; set; }
}