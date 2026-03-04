using Lame.DomainModel;

namespace Lame.Backend.Tags;

public interface ITags
{
    Task<List<Tag>> Get();
    Task<List<Tag>> Get(string searchTerm, int limit);
    Task<List<Tag>> GetTagsForResource(Guid resourceId);
    Task<List<Guid>> GetResourcesWithTag(Guid tagId, ResourceType resourceType);
    
    Task AddTagToResource(Tag tag, Guid resourceId, ResourceType resourceType);
    Task RemoveTagFromResource(Guid tagId, Guid resourceId);
    
    Task Create(Tag tag);
    Task Update(Tag tag);
}