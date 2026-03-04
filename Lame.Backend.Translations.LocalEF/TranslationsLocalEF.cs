using Lame.Backend.EntityFramework;
using Lame.Backend.EntityFramework.Models;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;

namespace Lame.Backend.Translations.LocalEF;

public class TranslationsLocalEF : ITranslations
{
    private readonly AppDbContext _context;
    
    public TranslationsLocalEF(AppDbContext context)
    {
        _context = context;
    }
    
    public Task<List<Translation>> Get()
    {
        return _context.Translations
            .Select(entity => (Translation)entity)
            .ToListAsync();
    }

    public Task<Translation?> Get(Guid id)
    {
        return _context.Translations
            .Where(entity => entity.Id == id)
            .Select(entity => (Translation)entity)
            .FirstOrDefaultAsync();
    }

    public Task<List<Translation>> GetForAsset(Guid assetId)
    {
        return _context.Translations
            .Where(entity => entity.AssetId == assetId)
            .Select(entity => (Translation)entity)
            .ToListAsync();
    }

    public Task Create(Translation translation)
    {
        translation.CreatedAt = DateTime.UtcNow;
        _context.Translations.Add(MapToEntity(translation));
        return _context.SaveChangesAsync();
    }

    public async Task Update(Translation translation)
    {
        _context.Translations.Update(MapToEntity(translation));
        await _context.SaveChangesAsync();
    }

    private static TranslationEntity MapToEntity(Translation translation)
    {
        return new TranslationEntity()
        {
            Id =  translation.Id,
            AssetId = translation.AssetId,
            CreatedAt =   translation.CreatedAt,
            Language =   translation.Language,
            Content =   translation.Content,
            MajorVersion =  translation.MajorVersion,
            MinorVersion =  translation.MinorVersion,
        };
    }
}