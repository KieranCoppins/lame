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

    public async Task<List<TranslationDto>> GetTargetedForAsset(Guid assetId)
    {
        return await Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var results = await context.Assets
                .Where(asset => asset.Id == assetId)
                .TargetTranslationPerLanguage()
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

    public Task<List<TranslationDto>> GetAllForLanguageForAsset(Guid assetId, string language)
    {
        return Task.Run(() =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            return context.Translations
                .Where(t => t.AssetId == assetId && t.Language == language)
                .AsDto()
                .OrderByDescending(t => t.MajorVersion)
                .ThenByDescending(t => t.MinorVersion)
                .ToListAsync();
        });
    }

    public Task<List<TranslationDto>> GetAllForAsset(Guid assetId)
    {
        return Task.Run(() =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            return context.Translations
                .Where(t => t.AssetId == assetId)
                .AsDto()
                .OrderByDescending(t => t.MajorVersion)
                .ThenByDescending(t => t.MinorVersion)
                .ToListAsync();
        });
    }

    public Task SetTargetTranslation(Guid translationId)
    {
        return Task.Run(() =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var translation = context.Translations.FirstOrDefault(t => t.Id == translationId);
            if (translation == null)
                throw new InvalidOperationException($"Could not find translation with id: {translationId}");

            var target = context.TargetAssetTranslations.FirstOrDefault(t =>
                t.AssetId == translation.AssetId && t.Language == translation.Language);

            if (target != null)
            {
                // Update existing target translation to point to the new translation
                target.TranslationId = translation.Id;
                context.TargetAssetTranslations.Update(target);
            }
            else
            {
                // Create a new target
                context.TargetAssetTranslations.Add(new TargetAssetTranslationEntity
                {
                    AssetId = translation.AssetId,
                    Language = translation.Language,
                    TranslationId = translation.Id
                });
            }

            return context.SaveChangesAsync();
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

            // Always target a new translation
            var target = context.TargetAssetTranslations.FirstOrDefault(t =>
                t.AssetId == translation.AssetId && t.Language == translation.Language);

            if (target != null)
            {
                // Update existing target translation to point to the new translation
                target.TranslationId = translation.Id;
                context.TargetAssetTranslations.Update(target);
            }
            else
            {
                // Create a new target
                context.TargetAssetTranslations.Add(new TargetAssetTranslationEntity
                {
                    AssetId = translation.AssetId,
                    Language = translation.Language,
                    TranslationId = translation.Id
                });
            }


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