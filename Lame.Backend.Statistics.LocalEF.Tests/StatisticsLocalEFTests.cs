using Lame.Backend.EntityFramework;
using Lame.Backend.EntityFramework.Models;
using Lame.Backend.EntityFramework.Tests;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Backend.Statistics.LocalEF.Tests;

public class StatisticsLocalEFTests
{
    [Fact]
    public async Task GetProjectStatistics_WithNoData_ReturnsZerosAndEmptyDictionaries()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var statisticsLocalEf = new StatisticsLocalEF(serviceProvider);

        // Act
        var result = await statisticsLocalEf.GetProjectStatistics();

        // Assert
        Assert.Equal(0, result.TotalAssets);
        Assert.Equal(0, result.TotalLanguages);
        Assert.Equal(0, result.MissingTranslations);
        Assert.Empty(result.TranslationsByLanguage);
        Assert.Empty(result.AssetsByType);
        Assert.Empty(result.AssetsByTag);
    }

    [Fact]
    public async Task GetProjectStatistics_WithAssetsLanguagesAndTags_ReturnsCorrectCounts()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tag = new TagEntity { Id = Guid.NewGuid(), Name = "tag1" };
        var language = new LanguageEntity { LanguageCode = "en" };
        var asset = new AssetEntity
        {
            Id = Guid.NewGuid(), InternalName = "asset1", Status = AssetStatus.Active, AssetType = AssetType.Audio,
            Tags = new List<TagEntity> { tag }
        };

        var translation = new TranslationEntity { Id = Guid.NewGuid(), Language = "en" };

        var translationTarget = new TargetAssetTranslationEntity
        {
            AssetId = asset.Id,
            TranslationId = translation.Id,
            Asset = asset,
            Translation = translation,
            Language = "en"
        };

        context.Tags.Add(tag);
        context.Languages.Add(language);
        context.Assets.Add(asset);
        context.Translations.Add(translation);
        context.TargetAssetTranslations.Add(translationTarget);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var statisticsLocalEf = new StatisticsLocalEF(serviceProvider);

        // Act
        var result = await statisticsLocalEf.GetProjectStatistics();

        // Assert
        Assert.Equal(1, result.TotalAssets);
        Assert.Equal(1, result.TotalLanguages);
        Assert.Equal(0, result.MissingTranslations);
        Assert.Single(result.TranslationsByLanguage);
        Assert.Single(result.AssetsByType);
        Assert.Single(result.AssetsByTag);
        Assert.Equal(1, result.AssetsByTag.Values.First());
    }

    [Fact]
    public async Task GetProjectStatistics_WithDeletedAssets_ExcludesDeletedFromCounts()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using (var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName))
        {
            var tag = new TagEntity { Id = Guid.NewGuid(), Name = "tag1" };

            var assetActive = new AssetEntity
            {
                Id = Guid.NewGuid(), InternalName = "asset1", Status = AssetStatus.Active, AssetType = AssetType.Text,
                Tags = new List<TagEntity> { tag }
            };

            var assetDeleted = new AssetEntity
            {
                Id = Guid.NewGuid(), InternalName = "asset2", Status = AssetStatus.Deleted, AssetType = AssetType.Text,
                Tags = new List<TagEntity> { tag }
            };

            var assetActiveTranslation = new TranslationEntity { Id = Guid.NewGuid(), Language = "en" };
            var assetDeletedTranslation = new TranslationEntity { Id = Guid.NewGuid(), Language = "en" };

            var language = new LanguageEntity { LanguageCode = "en" };
            var activeTranslationTarget = new TargetAssetTranslationEntity
            {
                AssetId = assetActive.Id,
                Asset = assetActive,
                TranslationId = assetActiveTranslation.Id,
                Translation = assetActiveTranslation,
                Language = "en"
            };

            var deletedTranslationTarget = new TargetAssetTranslationEntity
            {
                AssetId = assetDeleted.Id,
                Asset = assetDeleted,
                TranslationId = assetDeletedTranslation.Id,
                Translation = assetDeletedTranslation,
                Language = "en"
            };

            context.Tags.Add(tag);
            context.Languages.Add(language);
            context.Assets.AddRange(assetActive, assetDeleted);
            context.TargetAssetTranslations.AddRange(activeTranslationTarget, deletedTranslationTarget);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var statisticsLocalEf = new StatisticsLocalEF(serviceProvider);

        // Act
        var result = await statisticsLocalEf.GetProjectStatistics();

        // Assert
        Assert.Equal(1, result.TotalAssets);
        Assert.Single(result.AssetsByType);
        Assert.Single(result.AssetsByTag);
        Assert.Equal(1, result.AssetsByTag.Values.First());
    }

    [Fact]
    public async Task GetProjectStatistics_MissingTranslations_ComputesCorrectly()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);
        var languageEn = new LanguageEntity { LanguageCode = "en" };
        var languageFr = new LanguageEntity { LanguageCode = "fr" };
        var asset = new AssetEntity
        {
            Id = Guid.NewGuid(), InternalName = "asset1", Status = AssetStatus.Active, AssetType = AssetType.Audio
        };

        var translation = new TranslationEntity { Id = Guid.NewGuid(), Language = "en" };

        var translationTarget = new TargetAssetTranslationEntity
        {
            AssetId = asset.Id,
            TranslationId = translation.Id,
            Asset = asset,
            Translation = translation,
            Language = "en"
        };

        context.Languages.AddRange(languageEn, languageFr);
        context.Assets.Add(asset);
        context.Translations.Add(translation);
        context.TargetAssetTranslations.Add(translationTarget);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var statisticsLocalEf = new StatisticsLocalEF(serviceProvider);

        // Act
        var result = await statisticsLocalEf.GetProjectStatistics();

        // Assert
        Assert.Equal(1, result.TotalAssets);
        Assert.Equal(2, result.TotalLanguages);
        Assert.Equal(1, result.MissingTranslations);
        Assert.Equal(2, result.TranslationsByLanguage.Count);
        Assert.Equal(1, result.TranslationsByLanguage["en"]);
        Assert.Equal(0, result.TranslationsByLanguage["fr"]);
    }
}