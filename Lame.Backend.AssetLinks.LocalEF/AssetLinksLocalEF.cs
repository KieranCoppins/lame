using Lame.Backend.EntityFramework;
using Lame.Backend.EntityFramework.Models;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Backend.AssetLinks.LocalEF;

public class AssetLinksLocalEF : IAssetLinks
{
    private readonly IServiceProvider _serviceProvider;

    public AssetLinksLocalEF(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<List<AssetLink>> GetAssetLinks()
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var results = await context.AssetLinks
                .GroupBy(link => new
                {
                    AssetId1 = link.AssetEntityId < link.LinkedContentId
                        ? link.AssetEntityId
                        : link.LinkedContentId,
                    AssetId2 = link.AssetEntityId < link.LinkedContentId
                        ? link.LinkedContentId
                        : link.AssetEntityId
                })
                .Select(g => g.First())
                .ToListAsync();

            return results
                .Select(entity => new AssetLink
                {
                    AssetEntityId = entity.AssetEntityId,
                    LinkedContentId = entity.LinkedContentId,
                    Synced = entity.Synced
                }).ToList();
        });
    }

    public Task DesyncAssetLinks(Guid assetId)
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var links = await context.AssetLinks
                .Where(link => link.AssetEntityId == assetId || link.LinkedContentId == assetId)
                .ToListAsync();

            context.AssetLinks.UpdateRange(links.Select(link =>
            {
                link.Synced = false;
                return link;
            }));

            await context.SaveChangesAsync();
        });
    }

    public Task SyncAssetLink(Guid assetId, Guid linkedAssetId, bool synced = true)
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var linkA = await context.AssetLinks
                .Where(link => link.AssetEntityId == assetId && link.LinkedContentId == linkedAssetId)
                .FirstOrDefaultAsync();

            var linkB = await context.AssetLinks
                .Where(link => link.AssetEntityId == linkedAssetId && link.LinkedContentId == assetId)
                .FirstOrDefaultAsync();

            if (linkA == null || linkB == null)
                throw new InvalidOperationException("Link not found");

            linkA.Synced = synced;
            linkB.Synced = synced;

            context.AssetLinks.UpdateRange(linkA, linkB);
            await context.SaveChangesAsync();
        });
    }

    public async Task<AssetLink> Create(Guid assetId, Guid linkedAssetId)
    {
        return await Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var assetAEntity = await context.Assets
                .Where(entity => entity.Id == assetId)
                .FirstOrDefaultAsync();

            var assetBEntity = await context.Assets
                .Where(entity => entity.Id == linkedAssetId)
                .FirstOrDefaultAsync();

            if (assetAEntity == null || assetBEntity == null)
                throw new InvalidOperationException("One or both assets not found");

            // Create asset links
            var linkA = new AssetLinkEntity
            {
                AssetEntityId = assetAEntity.Id,
                AssetEntity = assetAEntity,
                LinkedContentId = assetBEntity.Id,
                LinkedAssetEntity = assetBEntity,
                Synced = false
            };

            var linkB = new AssetLinkEntity
            {
                AssetEntityId = assetBEntity.Id,
                AssetEntity = assetBEntity,
                LinkedContentId = assetAEntity.Id,
                LinkedAssetEntity = assetAEntity,
                Synced = false
            };

            // Compound Key will ensure that we don't create duplicate links
            context.AssetLinks.AddRange(linkA, linkB);
            await context.SaveChangesAsync();
            return linkA;
        });
    }

    public Task Delete(Guid assetId, Guid linkedAssetId)
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var linkA = await context.AssetLinks
                .Where(link => link.AssetEntityId == assetId && link.LinkedContentId == linkedAssetId)
                .FirstOrDefaultAsync();

            var linkB = await context.AssetLinks
                .Where(link => link.AssetEntityId == linkedAssetId && link.LinkedContentId == assetId)
                .FirstOrDefaultAsync();

            // If the link doesn't exist we can just fail silently, since the end result is the same
            if (linkA == null || linkB == null) return Task.CompletedTask;

            context.AssetLinks.RemoveRange(linkA, linkB);
            return context.SaveChangesAsync();
        });
    }
}