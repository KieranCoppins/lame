using Lame.DomainModel;

namespace Lame.Backend.EntityFramework.Models;

public class TranslationEntity : Translation, ITaggableEntity
{
    public AssetEntity Asset { get; set; }
    
    public ICollection<TagEntity> Tags { get; set; }
}