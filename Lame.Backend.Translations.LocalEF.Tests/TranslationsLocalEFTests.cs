using Lame.Backend.EntityFramework;
using Lame.Backend.EntityFramework.Models;
using Lame.Backend.EntityFramework.Tests;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Backend.Translations.LocalEF.Tests;

public class TranslationsLocalEFTests
{
    [Fact]
    public async Task GetTargetedForAsset_AssetWithAllLanguages_ReturnsAllTargetedTranslations()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assetId = Guid.NewGuid();
        var languages = new List<LanguageEntity>
        {
            new() { LanguageCode = "en" },
            new() { LanguageCode = "fr" }
        };
        var translations = new List<TranslationEntity>
        {
            new() { Id = Guid.NewGuid(), AssetId = assetId, Language = "en" },
            new() { Id = Guid.NewGuid(), AssetId = assetId, Language = "fr" }
        };
        var asset = new AssetEntity { Id = assetId, InternalName = "asset_a", Status = AssetStatus.Active };

        context.Languages.AddRange(languages);
        context.Assets.Add(asset);
        context.Translations.AddRange(translations);
        context.TargetAssetTranslations.AddRange(
            new TargetAssetTranslationEntity { AssetId = assetId, Language = "en", TranslationId = translations[0].Id },
            new TargetAssetTranslationEntity { AssetId = assetId, Language = "fr", TranslationId = translations[1].Id }
        );
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var translationsLocalEf = new TranslationsLocalEF(serviceProvider);

        // Act
        var result = await translationsLocalEf.GetTargetedForAsset(assetId);

        // Asset
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Id == translations[0].Id);
        Assert.Contains(result, t => t.Id == translations[1].Id);
    }

    [Fact]
    public async Task GetTargetedForAsset_AssetWithMissingLanguage_AddsMissingTranslationDto()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assetId = Guid.NewGuid();
        var languages = new List<LanguageEntity>
        {
            new() { LanguageCode = "en" },
            new() { LanguageCode = "fr" }
        };

        var translations = new List<TranslationEntity>
        {
            new() { Id = Guid.NewGuid(), AssetId = assetId, Language = "en" }
        };

        var asset = new AssetEntity { Id = assetId, InternalName = "asset_a", Status = AssetStatus.Active };

        context.Languages.AddRange(languages);
        context.Assets.Add(asset);
        context.Translations.AddRange(translations);
        context.TargetAssetTranslations.Add(
            new TargetAssetTranslationEntity { AssetId = assetId, Language = "en", TranslationId = translations[0].Id }
        );

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var translationsLocalEf = new TranslationsLocalEF(serviceProvider);

        // Act
        var result = await translationsLocalEf.GetTargetedForAsset(assetId);


        // Asset
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Id == translations[0].Id);
        var missing = result.FirstOrDefault(t => t.Language == "fr");
        Assert.NotNull(missing);
        Assert.Equal(TranslationStatus.Missing, missing.Status);
        Assert.Equal(assetId, missing.AssetId);
    }

    [Fact]
    public async Task GetTargetedForAsset_AssetDoesNotExist_ReturnsMissingForAllLanguages()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assetId = Guid.NewGuid();
        var languages = new List<LanguageEntity>
        {
            new() { LanguageCode = "en" },
            new() { LanguageCode = "fr" }
        };

        context.Languages.AddRange(languages);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var translationsLocalEf = new TranslationsLocalEF(serviceProvider);

        // Act
        var result = await translationsLocalEf.GetTargetedForAsset(assetId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal(TranslationStatus.Missing, t.Status));
        Assert.All(result, t => Assert.Equal(assetId, t.AssetId));
        Assert.Contains(result, t => t.Language == "en");
        Assert.Contains(result, t => t.Language == "fr");
    }

    [Fact]
    public async Task GetTargetedForAsset_NoLanguagesConfigured_ReturnsEmptyList()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assetId = Guid.NewGuid();
        var asset = new AssetEntity { Id = assetId, InternalName = "asset_a", Status = AssetStatus.Active };
        context.Assets.Add(asset);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var translationsLocalEf = new TranslationsLocalEF(serviceProvider);

        // Act
        var result = await translationsLocalEf.GetTargetedForAsset(assetId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllForLanguageForAsset_AssetWithMultipleVersions_ReturnsOrderedByVersion()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assetId = Guid.NewGuid();
        var translations = new List<TranslationEntity>
        {
            new() { Id = Guid.NewGuid(), AssetId = assetId, Language = "en" },
            new() { Id = Guid.NewGuid(), AssetId = assetId, Language = "en" },
            new() { Id = Guid.NewGuid(), AssetId = assetId, Language = "en" }
        };
        context.Translations.AddRange(translations);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var translationsLocalEf = new TranslationsLocalEF(serviceProvider);

        // Act
        var result = await translationsLocalEf.GetAllForLanguageForAsset(assetId, "en");

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, t => t.Id == translations[0].Id);
        Assert.Contains(result, t => t.Id == translations[1].Id);
        Assert.Contains(result, t => t.Id == translations[2].Id);
    }

    [Fact]
    public async Task GetAllForLanguageForAsset_AssetWithNoTranslations_ReturnsEmptyList()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assetId = Guid.NewGuid();
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var translationsLocalEf = new TranslationsLocalEF(serviceProvider);

        // Act
        var result = await translationsLocalEf.GetAllForLanguageForAsset(assetId, "en");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllForLanguageForAsset_TranslationsForOtherLanguages_IgnoresOtherLanguages()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assetId = Guid.NewGuid();
        var translations = new List<TranslationEntity>
        {
            new() { Id = Guid.NewGuid(), AssetId = assetId, Language = "en" },
            new() { Id = Guid.NewGuid(), AssetId = assetId, Language = "fr" }
        };

        context.Translations.AddRange(translations);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var translationsLocalEf = new TranslationsLocalEF(serviceProvider);

        // Act
        var result = await translationsLocalEf.GetAllForLanguageForAsset(assetId, "en");


        // Assert
        Assert.Single(result);
        Assert.Equal(translations[0].Id, result[0].Id);
    }

    [Fact]
    public async Task GetAllForLanguageForAsset_TranslationsForOtherAssets_IgnoresOtherAssets()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assetId = Guid.NewGuid();
        var otherAssetId = Guid.NewGuid();
        var translations = new List<TranslationEntity>
        {
            new() { Id = Guid.NewGuid(), AssetId = assetId, Language = "en" },
            new() { Id = Guid.NewGuid(), AssetId = otherAssetId, Language = "en" }
        };
        context.Translations.AddRange(translations);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var translationsLocalEf = new TranslationsLocalEF(serviceProvider);

        // Act
        var result = await translationsLocalEf.GetAllForLanguageForAsset(assetId, "en");

        // Assert
        Assert.Single(result);
        Assert.Equal(translations[0].Id, result[0].Id);
    }

    [Fact]
    public async Task SetTargetTranslation_TranslationExists_TargetIsCreatedIfNotExists()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assetId = Guid.NewGuid();
        var translation = new TranslationEntity { Id = Guid.NewGuid(), AssetId = assetId, Language = "en" };
        context.Translations.Add(translation);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var translationsLocalEf = new TranslationsLocalEF(serviceProvider);

        // Act
        await translationsLocalEf.SetTargetTranslation(translation.Id);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);
        var target = assertContext.TargetAssetTranslations.FirstOrDefault(t =>
            t.AssetId == assetId);
        Assert.NotNull(target);
        Assert.Equal(translation.Id, target.TranslationId);
    }

    [Fact]
    public async Task SetTargetTranslation_TranslationExists_TargetIsUpdatedIfExists()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assetId = Guid.NewGuid();
        var translations = new List<TranslationEntity>
        {
            new() { Id = Guid.NewGuid(), AssetId = assetId, Language = "en" },
            new() { Id = Guid.NewGuid(), AssetId = assetId, Language = "en" }
        };
        var target = new TargetAssetTranslationEntity
            { AssetId = assetId, Language = "en", TranslationId = translations[0].Id };
        context.Translations.AddRange(translations);
        context.TargetAssetTranslations.Add(target);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var translationsLocalEf = new TranslationsLocalEF(serviceProvider);

        // Act
        await translationsLocalEf.SetTargetTranslation(translations[1].Id);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);
        var updatedTarget = assertContext.TargetAssetTranslations.FirstOrDefault(t =>
            t.AssetId == assetId);
        Assert.NotNull(updatedTarget);
        Assert.Equal(translations[1].Id, updatedTarget.TranslationId);
    }

    [Fact]
    public async Task SetTargetTranslation_TranslationDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var translationsLocalEf = new TranslationsLocalEF(serviceProvider);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            translationsLocalEf.SetTargetTranslation(Guid.NewGuid()));
    }

    [Fact]
    public async Task Create_NewTranslation_AddsTranslationAndSetsTarget()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var assetId = Guid.NewGuid();
        var translation = new Translation { Id = Guid.NewGuid(), AssetId = assetId, Language = "en" };

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var translationsLocalEf = new TranslationsLocalEF(serviceProvider);

        // Act
        await translationsLocalEf.Create(translation);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);
        var created = assertContext.Translations.FirstOrDefault(t => t.Id == translation.Id);
        var target =
            assertContext.TargetAssetTranslations.FirstOrDefault(t => t.AssetId == assetId);

        Assert.NotNull(created);
        Assert.NotNull(target);
        Assert.Equal(translation.Id, target.TranslationId);
    }

    [Fact]
    public async Task Create_TranslationForExistingTarget_UpdatesTargetToNewTranslation()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assetId = Guid.NewGuid();
        var oldTranslation = new TranslationEntity { Id = Guid.NewGuid(), AssetId = assetId, Language = "en" };
        var target = new TargetAssetTranslationEntity
            { AssetId = assetId, Language = "en", TranslationId = oldTranslation.Id };

        context.Translations.Add(oldTranslation);
        context.TargetAssetTranslations.Add(target);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var newTranslation = new Translation { Id = Guid.NewGuid(), AssetId = assetId, Language = "en" };

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var translationsLocalEf = new TranslationsLocalEF(serviceProvider);

        // Act
        await translationsLocalEf.Create(newTranslation);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);
        var updatedTarget =
            assertContext.TargetAssetTranslations.FirstOrDefault(t => t.AssetId == assetId);

        Assert.NotNull(updatedTarget);
        Assert.Equal(newTranslation.Id, updatedTarget.TranslationId);
    }

    [Fact]
    public async Task Create_TranslationWithDifferentLanguage_CreatesSeparateTarget()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var assetId = Guid.NewGuid();
        var translationEn = new Translation { Id = Guid.NewGuid(), AssetId = assetId, Language = "en" };
        var translationFr = new Translation { Id = Guid.NewGuid(), AssetId = assetId, Language = "fr" };

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var translationsLocalEf = new TranslationsLocalEF(serviceProvider);

        // Act
        await translationsLocalEf.Create(translationEn);
        await translationsLocalEf.Create(translationFr);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);
        var targetEn =
            assertContext.TargetAssetTranslations.FirstOrDefault(t => t.AssetId == assetId && t.Language == "en");
        var targetFr =
            assertContext.TargetAssetTranslations.FirstOrDefault(t => t.AssetId == assetId && t.Language == "fr");

        Assert.NotNull(targetEn);
        Assert.NotNull(targetFr);
        Assert.Equal(translationEn.Id, targetEn.TranslationId);
        Assert.Equal(translationFr.Id, targetFr.TranslationId);
    }

    [Fact]
    public async Task Update_ExistingTranslation_UpdatesContentAndVersion()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assetId = Guid.NewGuid();
        var translationId = Guid.NewGuid();
        var original = new TranslationEntity
        {
            Id = translationId,
            AssetId = assetId,
            Language = "en",
            Content = "Old Content",
            MajorVersion = 1,
            MinorVersion = 0
        };

        context.Translations.Add(original);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var updated = new Translation
        {
            Id = translationId,
            AssetId = assetId,
            Language = "en",
            Content = "Updated Content",
            MajorVersion = 2,
            MinorVersion = 1
        };

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var translationsLocalEf = new TranslationsLocalEF(serviceProvider);

        // Act
        await translationsLocalEf.Update(updated);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);
        var result = assertContext.Translations.FirstOrDefault(t => t.Id == translationId);

        Assert.NotNull(result);
        Assert.Equal("Updated Content", result.Content);
        Assert.Equal(2, result.MajorVersion);
        Assert.Equal(1, result.MinorVersion);
    }
}