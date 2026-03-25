using Lame.Backend.ChangeLog;
using Lame.Backend.EntityFramework;
using Lame.Backend.EntityFramework.Models;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Backend.Imports.LocalEF;

public class ImportsLocalEF : IImports
{
    private readonly IChangeLog _changeLog;
    private readonly IServiceProvider _serviceProvider;

    public ImportsLocalEF(IServiceProvider serviceProvider, IChangeLog changeLog)
    {
        _serviceProvider = serviceProvider;
        _changeLog = changeLog;
    }

    public Task<int> Import(ImportOptions importOptions)
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (importOptions.ImportData.Count == 0)
                return 0;

            var assetIds = importOptions.ImportData
                .Select(x => x.Asset.Id)
                .ToHashSet();

            var existingAssets = await context.Assets
                .Where(a => assetIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id);

            var targetLanguage = importOptions.ImportData.First().TargetTranslation.Language;

            var existingTranslations = (await context.Translations
                    .Where(t => assetIds.Contains(t.AssetId) &&
                                (t.Language == "en" || t.Language == targetLanguage))
                    .OrderByDescending(t => t.MajorVersion)
                    .ThenByDescending(t => t.MinorVersion)
                    .ToListAsync())
                .GroupBy(t => t.AssetId)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList());

            var assetTargetedSourceTranslations = await context.Assets
                .Include(a => a.TargetedTranslations)
                .ThenInclude(t => t.Translation)
                .Where(a => assetIds.Contains(a.Id))
                .Select(a => a.TargetedTranslations
                    // Source translation is always english
                    .First(t => t.Language == "en")
                    .Translation)
                .ToDictionaryAsync(a => a.AssetId);

            var updatedAssetsCount = 0;
            var createdAssetsCount = 0;
            var createdTranslationsCount = 0;

            var newAssets = new List<AssetEntity>();
            var newTranslations = new List<TranslationEntity>();

            // Find assets to update or create
            foreach (var importData in importOptions.ImportData)
            {
                // Update existing asset properties if an asset exists and the option is selected
                if (existingAssets.TryGetValue(importData.Asset.Id, out var existingAsset) &&
                    importOptions.OverwriteExistingAssetProperties)
                {
                    if (existingAsset.InternalName != importData.Asset.InternalName ||
                        existingAsset.ContextNotes != importData.Asset.ContextNotes)
                    {
                        existingAsset.InternalName = importData.Asset.InternalName;
                        existingAsset.ContextNotes = importData.Asset.ContextNotes;
                        existingAsset.LastUpdatedAt = DateTime.UtcNow;
                        updatedAssetsCount++;
                    }
                }
                // Create new assets if the option is selected and the asset doesn't exist
                else if (existingAsset == null && importOptions.CreateMissingAssets)
                {
                    newAssets.Add(new AssetEntity
                    {
                        Id = importData.Asset.Id,
                        AssetType = importData.Asset.AssetType,
                        ContextNotes = importData.Asset.ContextNotes,
                        InternalName = importData.Asset.InternalName,
                        CreatedAt = DateTime.UtcNow,
                        LastUpdatedAt = DateTime.UtcNow,
                        Status = AssetStatus.Active
                    });
                    createdAssetsCount++;
                }
                // Otherwise if the asset doesn't exist, and we're not to make them, skip to the next import data
                else if (existingAsset == null)
                {
                    continue;
                }

                // Create default translation entities with a version of 1.0
                var sourceTranslationEntity = new TranslationEntity
                {
                    Id = importData.SourceTranslation.Id,
                    AssetId = importData.Asset.Id,
                    Language = "en",
                    Content = importData.SourceTranslation.Content,
                    CreatedAt = DateTime.UtcNow,
                    MajorVersion = 1,
                    MinorVersion = 0
                };

                var targetTranslationEntity = new TranslationEntity
                {
                    Id = importData.TargetTranslation.Id,
                    AssetId = importData.Asset.Id,
                    Language = importData.TargetTranslation.Language,
                    Content = importData.TargetTranslation.Content,
                    CreatedAt = DateTime.UtcNow,
                    MajorVersion = 1,
                    MinorVersion = 0
                };

                // If we didn't have an existing entity, we created a new one above so lets keep the version as 1.0
                if (existingAsset == null)
                {
                    newTranslations.AddRange([sourceTranslationEntity, targetTranslationEntity]);
                    continue;
                }

                // If we are an existing asset we need to look at our existing translations
                // to determine what version we should make these new assets

                var existingSourceTranslation = assetTargetedSourceTranslations[importData.Asset.Id];

                // Gets the latest version across both source and target languages for this asset
                var latestExistingMajorVersion =
                    existingTranslations[importData.Asset.Id].First().MajorVersion;

                // If we have major changes and our content is different then create the new latest version
                // If our content is the same we ignore the import option
                if (existingSourceTranslation.Content != importData.SourceTranslation.Content &&
                    importOptions.ContainsMajorChanges)
                {
                    sourceTranslationEntity.MajorVersion = latestExistingMajorVersion + 1;
                    sourceTranslationEntity.MinorVersion = 0;
                    targetTranslationEntity.MajorVersion = latestExistingMajorVersion + 1;
                    targetTranslationEntity.MinorVersion = 0;

                    newTranslations.AddRange([sourceTranslationEntity, targetTranslationEntity]);
                    continue;
                }

                // Otherwise let's keep the same major version as the source translation we are currently targeting
                // this does not always mean it is the latest translation in the event the user rolled back
                sourceTranslationEntity.MajorVersion = existingSourceTranslation.MajorVersion;
                targetTranslationEntity.MajorVersion = existingSourceTranslation.MajorVersion;

                // Get the latest minor version for this major version with the source language
                sourceTranslationEntity.MinorVersion = existingTranslations[importData.Asset.Id]
                    .FirstOrDefault(x =>
                        x.Language == "en" &&
                        x.MajorVersion == targetTranslationEntity.MajorVersion
                    )?.MinorVersion + 1 ?? 0;

                // Get the latest minor version for this major version with the target language
                targetTranslationEntity.MinorVersion = existingTranslations[importData.Asset.Id]
                    .FirstOrDefault(x =>
                        x.Language == targetLanguage &&
                        x.MajorVersion == targetTranslationEntity.MajorVersion
                    )?.MinorVersion + 1 ?? 0;

                // TODO perhaps we dont always create new target translations if the targeted source and target translations are the same as the import source and target translation
                // Always create a new target translation
                newTranslations.Add(targetTranslationEntity);

                // If our source content is different then create a new source translation also
                if (existingSourceTranslation.Content != importData.SourceTranslation.Content)
                    newTranslations.Add(sourceTranslationEntity);
            }

            // Create Assets
            context.Assets.UpdateRange(existingAssets.Values);
            context.Assets.AddRange(newAssets);

            // Create Translations
            context.Translations.AddRange(newTranslations);

            // Target new translations
            foreach (var translation in newTranslations)
            {
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
            }

            await context.SaveChangesAsync();

            await _changeLog.Create(new ChangeLogEntry
            {
                ResourceAction = ResourceAction.Imported,
                ResourceType = ResourceType.Translation,
                Message =
                    $"Imported {createdAssetsCount} assets, updated {updatedAssetsCount} assets with {newTranslations.Count} new translations"
            });

            return newTranslations.Count;
        });
    }
}