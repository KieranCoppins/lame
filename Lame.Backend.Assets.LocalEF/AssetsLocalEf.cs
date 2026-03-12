using Lame.Backend.EntityFramework;
using Lame.Backend.EntityFramework.Models;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Backend.Assets.LocalEF;

public class AssetsLocalEf : IAssets
{
    private readonly IServiceProvider _serviceProvider;

    public AssetsLocalEf(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<List<AssetDto>> Get()
    {
        return await Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // TODO consider pagination if the dataset grows large
            var assets = await context.Assets
                .Include(e => e.Translations)
                .ToListAsync();

            return assets.Select(MapToDto).ToList();
        });
    }

    public async Task<List<AssetDto>> Get(string searchTerm, int limit = 10)
    {
        return await Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            searchTerm = searchTerm.ToLower();

            var assets = await context.Assets
                .Include(a => a.Translations)
                .Include(a => a.Tags)
                .Where(a =>
                    a.InternalName.ToLower().Contains(searchTerm) ||
                    a.Tags.Any(t => t.Name.ToLower().Contains(searchTerm)))
                .OrderBy(a =>
                    a.InternalName.ToLower() == searchTerm ? 0 :
                    a.InternalName.ToLower().StartsWith(searchTerm) ? 1 :
                    a.InternalName.ToLower().Contains(searchTerm) ? 2 :
                    3)
                .ThenBy(a => a.InternalName)
                .Take(limit)
                .ToListAsync();

            return assets.Select(MapToDto).ToList();
        });
    }

    public async Task<AssetDto?> Get(Guid id)
    {
        return await Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var asset = await context.Assets
                .Include(entity => entity.Translations)
                .Where(entity => entity.Id == id)
                .FirstOrDefaultAsync();

            return asset == null ? null : MapToDto(asset);
        });
    }

    public async Task<List<AssetDto>> GetLinkedAssets(Guid assetId)
    {
        return await Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var assetWithLinkedContent = await context.Assets
                .Include(entity => entity.LinkedContent)
                .ThenInclude(entity => entity.Translations)
                .Where(entity => entity.Id == assetId)
                .FirstOrDefaultAsync();

            return assetWithLinkedContent?.LinkedContent.Select(MapToDto).ToList() ?? [];
        });
    }

    public Task LinkAssets(Guid assetA, Guid assetB)
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var assetAEntity = await context.Assets
                .Include(entity => entity.LinkedContent)
                .Where(entity => entity.Id == assetA)
                .FirstOrDefaultAsync();

            var assetBEntity = await context.Assets
                .Include(entity => entity.LinkedContent)
                .Where(entity => entity.Id == assetB)
                .FirstOrDefaultAsync();

            if (assetAEntity == null || assetBEntity == null) return Task.CompletedTask;

            // TODO validate that the assets are not already linked?

            assetAEntity.LinkedContent.Add(assetBEntity);
            assetBEntity.LinkedContent.Add(assetAEntity);
            context.Assets.UpdateRange(assetAEntity, assetBEntity);
            return context.SaveChangesAsync();
        });
    }

    public Task UnLinkAssets(Guid assetA, Guid assetB)
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var assetAEntity = await context.Assets
                .Include(entity => entity.LinkedContent)
                .Where(entity => entity.Id == assetA)
                .FirstOrDefaultAsync();

            var assetBEntity = await context.Assets
                .Include(entity => entity.LinkedContent)
                .Where(entity => entity.Id == assetB)
                .FirstOrDefaultAsync();

            if (assetAEntity == null || assetBEntity == null) return Task.CompletedTask;

            // TODO validate that the assets are actually linked?

            assetAEntity.LinkedContent.Remove(assetBEntity);
            assetBEntity.LinkedContent.Remove(assetAEntity);
            context.Assets.UpdateRange(assetAEntity, assetBEntity);
            return context.SaveChangesAsync();
        });
    }

    public Task Create(Asset asset)
    {
        return Task.Run(() =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            asset.CreatedAt = DateTime.UtcNow;
            asset.LastUpdatedAt = DateTime.UtcNow;
            asset.Status = AssetStatus.Active;
            context.Assets.Add(MapToEntity(asset));
            return context.SaveChangesAsync();
        });
    }

    public Task Update(Asset asset)
    {
        return Task.Run(() =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var entity = MapToEntity(asset);
            entity.LastUpdatedAt = DateTime.UtcNow;

            context.Assets.Update(entity);
            return context.SaveChangesAsync();
        });
    }

    public Task Delete(Guid id)
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var sourceContent = await context.Assets.FindAsync(id);

            if (sourceContent != null)
            {
                sourceContent.Status = AssetStatus.Deleted;
                context.Assets.Update(sourceContent);
                await context.SaveChangesAsync();
            }
        });
    }

    private static AssetEntity MapToEntity(Asset asset)
    {
        return new AssetEntity
        {
            Id = asset.Id,
            AssetType = asset.AssetType,
            ContextNotes = asset.ContextNotes,
            InternalName = asset.InternalName,
            LastUpdatedAt = asset.LastUpdatedAt,
            CreatedAt = asset.CreatedAt,
            Status = asset.Status
        };
    }

    private static AssetDto MapToDto(AssetEntity entity)
    {
        return new AssetDto
        {
            Id = entity.Id,
            AssetType = entity.AssetType,
            ContextNotes = entity.ContextNotes,
            InternalName = entity.InternalName,
            LastUpdatedAt = entity.LastUpdatedAt,
            CreatedAt = entity.CreatedAt,
            Status = entity.Status,
            NumTranslations = entity.Translations.Count
        };
    }
}