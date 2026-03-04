namespace Lame.Backend.EntityFramework.Models;

public interface ITaggableEntity
{
    public Guid Id { get; set; }
    public ICollection<TagEntity> Tags { get; set; }
}