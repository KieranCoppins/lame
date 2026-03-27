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

    public async Task<PaginatedResponse<AssetDto>> Get(int page, int pageSize)
    {
        return await Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (pageSize <= 0)
                return new PaginatedResponse<AssetDto>
                {
                    Page = page,
                    TotalPages = 0,
                    Items = []
                };

            var query = context.Assets
                .Where(a => a.Status != AssetStatus.Deleted);

            var totalAssets = await query.CountAsync();

            var pagedAssets = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .AsDto()
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalAssets / pageSize);

            return new PaginatedResponse<AssetDto>
            {
                Page = page,
                TotalPages = totalPages,
                Items = pagedAssets
            };
        });
    }

    public async Task<List<AssetDto>> Get(string searchTerm, int limit = 10)
    {
        return await Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            searchTerm = searchTerm.ToLower();

            return await context.Assets
                .Include(a => a.Tags)
                .Where(a => a.Status != AssetStatus.Deleted)
                .SearchBy(searchTerm)
                .Take(limit)
                .AsDto()
                .ToListAsync();
        });
    }

    public async Task<AssetDto?> Get(Guid id)
    {
        return await Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            return await context.Assets
                .Where(entity => entity.Id == id)
                .AsDto()
                .FirstOrDefaultAsync();
        });
    }

    public Task<List<AssetDto>> Get(List<Guid> ids)
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            return await context.Assets
                .Where(entity => ids.Contains(entity.Id))
                .AsDto()
                .ToListAsync();
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

    public async Task<List<AssetDto>> GetLinkedAssets(Guid assetId)
    {
        return await Task.Run(() =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            return context.Assets
                .Where(entity => entity.Id == assetId)
                .SelectMany(a => a.LinkedTo.Select(x => x.LinkedAssetEntity)
                    .Where(linkedAsset => linkedAsset.Status != AssetStatus.Deleted))
                .AsDto()
                .ToListAsync();
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
}