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

    public Task<List<AssetDto>> Get()
    {
        // TODO consider pagination if the dataset grows large
        return _context.Assets
            .Include(entity => entity.Translations)
            .Select(entity => MapToDto(entity))
            .ToListAsync();
    }

    public Task<AssetDto?> Get(Guid id)
    {
        return _context.Assets
            .Include(entity => entity.Translations)
            .Where(entity => entity.Id == id)
            .Select(entity => MapToDto(entity))
            .FirstOrDefaultAsync();
    }

    public Task Create(Asset asset)
    {
        _context.Assets.Add(MapToEntity(asset));
        return _context.SaveChangesAsync();
    }

    public async Task Update(Asset asset)
    {
        _context.Assets.Update(MapToEntity(asset));
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
    
    private static AssetDto MapToDto(AssetEntity entity)
    {
        return new AssetDto()
        {
            Id = entity.Id,
            AssetType = entity.AssetType,
            ContextNotes = entity.ContextNotes,
            InternalName = entity.InternalName,
            LastUpdatedAt = entity.LastUpdatedAt,
            CreatedAt = entity.CreatedAt,
            Status = entity.Status,
            NumTranslations = entity.Translations.Count,
        };
    }
}