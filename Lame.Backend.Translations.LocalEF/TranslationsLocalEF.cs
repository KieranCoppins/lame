using Lame.Backend.EntityFramework;
using Lame.Backend.EntityFramework.Models;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Backend.Translations.LocalEF;

public class TranslationsLocalEF : ITranslations
{
    private readonly IServiceProvider _serviceProvider;

    public TranslationsLocalEF(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<List<TranslationDto>> GetForAsset(Guid assetId)
    {
        return await Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var results = await context.Translations
                .Where(entity => entity.AssetId == assetId)
                .LatestVersionPerLanguage()
                .AsDto()
                .ToListAsync();

            // Add missing translations with empty content based on supported languages
            var languages = await context.Languages.ToListAsync();
            var languageCodes = languages.Select(l => l.LanguageCode).ToList();

            foreach (var languageCode in languageCodes)
                if (results.All(t => t.Language != languageCode))
                    results.Add(new TranslationDto
                    {
                        Status = TranslationStatus.Missing,
                        Language = languageCode,
                        AssetId = assetId,
                        Id = Guid.NewGuid()
                    });

            // Sort translations to match the ordering of languages
            return results.OrderBy(t => languageCodes.IndexOf(t.Language)).ToList();
        });
    }

    public Task Create(Translation translation)
    {
        return Task.Run(() =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            translation.CreatedAt = DateTime.UtcNow;
            context.Translations.Add(MapToEntity(translation));
            return context.SaveChangesAsync();
        });
    }

    public Task Update(Translation translation)
    {
        return Task.Run(() =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Translations.Update(MapToEntity(translation));
            return context.SaveChangesAsync();
        });
    }

    private static TranslationEntity MapToEntity(Translation translation)
    {
        return new TranslationEntity
        {
            Id = translation.Id,
            AssetId = translation.AssetId,
            CreatedAt = translation.CreatedAt,
            Language = translation.Language,
            Content = translation.Content,
            MajorVersion = translation.MajorVersion,
            MinorVersion = translation.MinorVersion
        };
    }
}