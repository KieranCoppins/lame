using Lame.DomainModel;

namespace Lame.Backend.EntityFramework.Models;

public class TagEntity : Tag
{
    public ICollection<AssetEntity> Assets { get; set; }
    
    public ICollection<TranslationEntity> Translations { get; set; }
}