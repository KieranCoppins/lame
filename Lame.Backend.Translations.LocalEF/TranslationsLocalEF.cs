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

    public Task Create(Translation translation)
    {
        _context.Translations.Add(MapToEntity(translation));
        return _context.SaveChangesAsync();
    }

    public async Task Update(Translation translation)
    {
        var existingEntity = await _context.Translations.FindAsync(translation.Id);
        if (existingEntity == null)
        {
            throw new InvalidOperationException($"Translation with ID {translation.Id} not found.");
        }
        
        existingEntity.Content = translation.Content;
        existingEntity.MajorVersion = translation.MajorVersion;
        existingEntity.MinorVersion = translation.MinorVersion;
        existingEntity.CreatedAt = translation.CreatedAt;
        existingEntity.Language = translation.Language;
        
        _context.Translations.Update(existingEntity);
        await _context.SaveChangesAsync();
    }

    private static TranslationEntity MapToEntity(Translation translation)
    {
        return new TranslationEntity()
        {
            Id =  translation.Id,
            CreatedAt =   translation.CreatedAt,
            Language =   translation.Language,
            Content =   translation.Content,
            MajorVersion =  translation.MajorVersion,
            MinorVersion =  translation.MinorVersion,
        };
    }
}