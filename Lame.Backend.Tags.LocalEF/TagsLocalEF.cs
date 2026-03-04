using Lame.Backend.EntityFramework;
using Lame.Backend.EntityFramework.Models;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;

namespace Lame.Backend.Tags.LocalEF;

public class TagsLocalEF : ITags
{
    private readonly AppDbContext _context;
    
    public TagsLocalEF(AppDbContext context)
    {
        _context = context;
    }
    
    public Task<List<Tag>> Get()
    {
        throw new NotImplementedException();
    }

    public Task<List<Tag>> Get(string searchTerm, int limit)
    {
        searchTerm = searchTerm.ToLower();

        return _context.Tags
            .Where(t => t.Name.ToLower().Contains(searchTerm))
            .OrderBy(t => t.Name.ToLower() == searchTerm ? 0 :
                t.Name.ToLower().StartsWith(searchTerm) ? 1 : 2)
            .ThenBy(t => t.Name)
            .Take(limit)
            .Select(t => (Tag)t)
            .ToListAsync();
    }

    public Task<List<Tag>> GetTagsForResource(Guid resourceId)
    {
        var assetTags = TagsFor<AssetEntity>(resourceId);
        var translationTags = TagsFor<TranslationEntity>(resourceId);

        return assetTags
            .Union(translationTags)
            .Distinct()
            .Select(t => (Tag)t)
            .ToListAsync();
    }

    public Task<List<Guid>> GetResourcesWithTag(Guid tagId, ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.Asset => GetResourcesWithTag<AssetEntity>(tagId),
            ResourceType.Translation => GetResourcesWithTag<AssetEntity>(tagId),
            _ => throw new ArgumentException("Invalid resource type", nameof(resourceType))
        };
    }

    public async Task AddTagToResource(Tag tag, Guid resourceId, ResourceType resourceType)
    {
        // Check tag exists, if not create it
        var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == tag.Id);

        if (existingTag == null)
        {
            await Create(tag);
        }

        await (resourceType switch
        {
            ResourceType.Asset => AddTagToResource<AssetEntity>(tag.Id, resourceId),
            ResourceType.Translation => AddTagToResource<TranslationEntity>(tag.Id, resourceId),
            _ => throw new ArgumentException("Invalid resource type", nameof(resourceType))
        });
    }

    public Task RemoveTagFromResource(Guid tagId, Guid resourceId)
    {
        throw new NotImplementedException();
    }

    public Task Create(Tag tag)
    {
        _context.Tags.Add(MapToEntity(tag));
        return _context.SaveChangesAsync();
    }

    public Task Update(Tag tag)
    {
        _context.Tags.Update(MapToEntity(tag));
        return _context.SaveChangesAsync();
    }
    
    private IQueryable<TagEntity> TagsFor<TEntity>(Guid resourceId)
        where TEntity : class, ITaggableEntity
    {
        return _context.Set<TEntity>()
            .Where(e => e.Id == resourceId)
            .SelectMany(e => e.Tags);
    }
    
    private async Task<List<Guid>> GetResourcesWithTag<TEntity>(
        Guid tagId)
        where TEntity : class, ITaggableEntity
    {
        return await _context.Set<TEntity>()
            .Where(entity => entity.Tags.Any(tag => tag.Id == tagId))
            .Select(entity => entity.Id)
            .ToListAsync();
    }
    
    private async Task AddTagToResource<TEntity>(
        Guid tagId,
        Guid resourceId)
        where TEntity : class, ITaggableEntity, new()
    {
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.Id == tagId);

        if (tag == null)
        {
            throw new InvalidOperationException($"Tag with Id {tagId} not found.");
        }

        var entity = await _context.Set<TEntity>()
            .Include(e => e.Tags)
            .FirstOrDefaultAsync(e => e.Id == resourceId);
        
        if (entity == null)
        {
            throw new InvalidOperationException($"Resource with Id {resourceId} not found.");
        }
        
        if (entity.Tags.All(t => t.Id != tagId))
        {
            entity.Tags.Add(tag);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task RemoveTagFromResource<TEntity>(
        Guid tagId,
        Guid resourceId)
        where TEntity : class, ITaggableEntity
    {
        var entity = await _context.Set<TEntity>()
            .Include(e => e.Tags)
            .FirstOrDefaultAsync(e => e.Id == resourceId);

        if (entity == null)
        {
            throw new InvalidOperationException($"Resource with Id {resourceId} not found.");
        }

        var tag = entity.Tags.FirstOrDefault(t => t.Id == tagId);

        if (tag != null)
        {
            entity.Tags.Remove(tag);
            await _context.SaveChangesAsync();
        }
    }
    
    private static TagEntity MapToEntity(Tag tag)
    {
        return new TagEntity
        {
            Id = tag.Id,
            Name = tag.Name,
        };
    }
    
}