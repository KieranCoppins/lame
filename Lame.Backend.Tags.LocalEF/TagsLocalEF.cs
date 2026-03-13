using Lame.Backend.EntityFramework;
using Lame.Backend.EntityFramework.Models;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Backend.Tags.LocalEF;

public class TagsLocalEF : ITags
{
    private readonly IServiceProvider _serviceProvider;

    public TagsLocalEF(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<List<Tag>> Get()
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            return await context.Tags
                .OrderBy(t => t.Name)
                .Select(t => (Tag)t)
                .ToListAsync();
        });
    }

    public async Task<List<Tag>> Get(string searchTerm, int limit)
    {
        return await Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            searchTerm = searchTerm.ToLower();

            return await context.Tags
                .Where(t => t.Name.ToLower().Contains(searchTerm))
                .OrderBy(t => t.Name.ToLower() == searchTerm ? 0 :
                    t.Name.ToLower().StartsWith(searchTerm) ? 1 : 2)
                .ThenBy(t => t.Name)
                .Take(limit)
                .Select(t => (Tag)t)
                .ToListAsync();
        });
    }

    public async Task<List<Tag>> GetTagsForResource(Guid resourceId)
    {
        return await Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var assetTags = TagsFor<AssetEntity>(context, resourceId);
            var translationTags = TagsFor<TranslationEntity>(context, resourceId);

            return await assetTags
                .Union(translationTags)
                .Distinct()
                .Select(t => (Tag)t)
                .ToListAsync();
        });
    }

    public async Task<List<Guid>> GetResourcesWithTag(Guid tagId, ResourceType resourceType)
    {
        return await Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            return resourceType switch
            {
                ResourceType.Asset => await GetResourcesWithTag<AssetEntity>(context, tagId),
                ResourceType.Translation => await GetResourcesWithTag<AssetEntity>(context, tagId),
                _ => throw new ArgumentException("Invalid resource type", nameof(resourceType))
            };
        });
    }

    public Task AddTagToResource(Tag tag, Guid resourceId, ResourceType resourceType)
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Check tag exists, if not create it
            var existingTag = await context.Tags.FirstOrDefaultAsync(t => t.Id == tag.Id);

            if (existingTag == null) await Create(tag);

            return resourceType switch
            {
                ResourceType.Asset => AddTagToResource<AssetEntity>(context, tag.Id, resourceId),
                ResourceType.Translation => AddTagToResource<TranslationEntity>(context, tag.Id, resourceId),
                _ => throw new ArgumentException("Invalid resource type", nameof(resourceType))
            };
        });
    }

    public Task RemoveTagFromResource(Guid tagId, Guid resourceId)
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Try to remove tag from both asset and translation, since we don't know the resource type
            await RemoveTagFromResource<AssetEntity>(context, tagId, resourceId);
            await RemoveTagFromResource<TranslationEntity>(context, tagId, resourceId);
        });
    }

    public Task Create(Tag tag)
    {
        return Task.Run(() =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            context.Tags.Add(MapToEntity(tag));
            return context.SaveChangesAsync();
        });
    }

    public Task Update(Tag tag)
    {
        return Task.Run(() =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            context.Tags.Update(MapToEntity(tag));
            return context.SaveChangesAsync();
        });
    }

    private IQueryable<TagEntity> TagsFor<TEntity>(AppDbContext context, Guid resourceId)
        where TEntity : class, ITaggableEntity
    {
        return context.Set<TEntity>()
            .Where(e => e.Id == resourceId)
            .SelectMany(e => e.Tags);
    }

    private async Task<List<Guid>> GetResourcesWithTag<TEntity>(AppDbContext context, Guid tagId)
        where TEntity : class, ITaggableEntity
    {
        return await context.Set<TEntity>()
            .Where(entity => entity.Tags.Any(tag => tag.Id == tagId))
            .Select(entity => entity.Id)
            .ToListAsync();
    }

    private async Task AddTagToResource<TEntity>(
        AppDbContext context,
        Guid tagId,
        Guid resourceId)
        where TEntity : class, ITaggableEntity, new()
    {
        var tag = await context.Tags
            .FirstOrDefaultAsync(t => t.Id == tagId);

        if (tag == null) throw new InvalidOperationException($"Tag with Id {tagId} not found.");

        var entity = await context.Set<TEntity>()
            .Include(e => e.Tags)
            .FirstOrDefaultAsync(e => e.Id == resourceId);

        if (entity == null) throw new InvalidOperationException($"Resource with Id {resourceId} not found.");

        if (entity.Tags.All(t => t.Id != tagId))
        {
            entity.Tags.Add(tag);
            await context.SaveChangesAsync();
        }
    }

    private async Task RemoveTagFromResource<TEntity>(
        AppDbContext context,
        Guid tagId,
        Guid resourceId)
        where TEntity : class, ITaggableEntity
    {
        var entity = await context.Set<TEntity>()
            .Include(e => e.Tags)
            .FirstOrDefaultAsync(e => e.Id == resourceId);

        if (entity == null) return;

        var tag = entity.Tags.FirstOrDefault(t => t.Id == tagId);

        if (tag == null) return;

        entity.Tags.Remove(tag);
        await context.SaveChangesAsync();
    }

    private static TagEntity MapToEntity(Tag tag)
    {
        return new TagEntity
        {
            Id = tag.Id,
            Name = tag.Name
        };
    }
}