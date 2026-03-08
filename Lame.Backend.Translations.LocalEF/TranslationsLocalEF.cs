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

            var results = await context.Translations.Where(entity => entity.AssetId == assetId).ToListAsync();

            // English will always be there by default on asset creation
            var englishTranslation = results.First(t => t.Language == "en");

            var englishDto = MapToDto(englishTranslation);
            englishDto.Status = TranslationStatus.UpToDate;

            // Other translations are out of date if their major version is less than the english translation
            // The english translation is out of date if any other translation has a major version greater than it
            var dtos = results
                .Where(entity => entity.Language != "en")
                .Select(entity =>
                {
                    var dto = MapToDto(entity);

                    if (entity.MajorVersion < englishTranslation.MajorVersion)
                        dto.Status = TranslationStatus.Outdated;
                    else
                        dto.Status = TranslationStatus.UpToDate;

                    if (entity.MajorVersion > englishTranslation.MajorVersion)
                        englishDto.Status = TranslationStatus.Outdated;
                    return dto;
                })
                .ToList();

            dtos.Add(englishDto);

            // Add missing translations with empty content based on supported languages
            var languages = await context.Languages.ToListAsync();
            var languageCodes = languages.Select(l => l.LanguageCode).ToList();

            foreach (var languageCode in languageCodes)
                if (dtos.All(t => t.Language != languageCode))
                    dtos.Add(new TranslationDto { Status = TranslationStatus.Missing, Language = languageCode });

            // Sort translations to match the ordering of languages
            var sortedDtos = dtos.OrderBy(t => languageCodes.IndexOf(t.Language)).ToList();

            return sortedDtos;
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

    private static TranslationDto MapToDto(TranslationEntity translation)
    {
        return new TranslationDto
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