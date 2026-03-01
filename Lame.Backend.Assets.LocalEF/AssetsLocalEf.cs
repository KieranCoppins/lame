using Lame.Backend.EntityFramework;
using Lame.Backend.EntityFramework.Models;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;

namespace Lame.Backend.Assets.LocalEF;

public class AssetsLocalEf : IAssets
{
    private readonly AppDbContext _context;

    public AssetsLocalEf(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Asset>> Get()
    {
        // TODO consider pagination if the dataset grows large
        return _context.Assets
            .Select(entity => (Asset)entity)
            .ToListAsync();
    }

    public Task<Asset?> Get(Guid id)
    {
        return _context.Assets
            .Where(entity => entity.Id == id)
            .Select(entity => (Asset)entity)
            .FirstOrDefaultAsync();
    }

    public Task Create(Asset asset)
    {
        _context.Assets.Add(MapToEntity(asset));
        return _context.SaveChangesAsync();
    }

    public async Task Update(Asset asset)
    {
        var existingEntity = await _context.Assets.FindAsync(asset.Id);
        if (existingEntity == null)
        {
            throw new InvalidOperationException($"Asset with ID {asset.Id} not found.");
        }

        existingEntity.ContextNotes = asset.ContextNotes;
        existingEntity.AssetType = asset.AssetType;
        existingEntity.InternalName = asset.InternalName;
        existingEntity.LastUpdatedAt = asset.LastUpdatedAt;
        existingEntity.CreatedAt = asset.CreatedAt;
        existingEntity.Status = asset.Status;

        _context.Assets.Update(existingEntity);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(Guid id)
    {
        var sourceContent = await _context.Assets.FindAsync(id);
        if (sourceContent != null)
        {
            sourceContent.Status = AssetStatus.Deleted;
            _context.Assets.Update(sourceContent);
            await _context.SaveChangesAsync();
        }
    }

    private static AssetEntity MapToEntity(Asset asset)
    {
        return new AssetEntity()
        {
            Id = asset.Id,
            AssetType = asset.AssetType,
            ContextNotes = asset.ContextNotes,
            InternalName = asset.InternalName,
            LastUpdatedAt = asset.LastUpdatedAt,
            CreatedAt = asset.CreatedAt,
            Status = asset.Status,
        };
    }
}