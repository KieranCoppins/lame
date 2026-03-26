using Lame.Backend.ChangeLog;
using Lame.Backend.EntityFramework;
using Lame.Backend.EntityFramework.Tests;
using Lame.Backend.EntityFramework.Tests.EntityBuilders;
using Lame.DomainModel;
using Lame.TestingHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Lame.Backend.Imports.LocalEF.Tests;

public class ImportsLocalEFTests
{
    [Fact]
    public async Task Import_NoImportData_DoesNothing()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var changeLogService = new Mock<IChangeLog>();

        var imports = new ImportsLocalEF(serviceProvider, changeLogService.Object);

        var importOptions = new ImportOptions
        {
            ImportData = new List<ImportData>()
        };

        // Act
        await imports.Import(importOptions);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        Assert.Equal(0, assertContext.Assets.Count());
        Assert.Equal(0, assertContext.Translations.Count());
    }

    [Fact]
    public async Task Import_NewAssetAndCreateMissingTrue_CreatesNewAsset()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var changeLogService = new Mock<IChangeLog>();

        var imports = new ImportsLocalEF(serviceProvider, changeLogService.Object);

        var assetToImport = new AssetBuilder().Build();
        var sourceTransToImport = new TranslationBuilder()
            .WithAssetId(assetToImport.Id)
            .WithLanguageCode("en")
            .Build();

        var targetTransToImport = new TranslationBuilder()
            .WithAssetId(assetToImport.Id)
            .WithLanguageCode("fr")
            .Build();

        var importOptions = new ImportOptions
        {
            ImportData = new List<ImportData>
            {
                new()
                {
                    Asset = assetToImport,
                    SourceTranslation = sourceTransToImport,
                    TargetTranslation = targetTransToImport
                }
            },
            CreateMissingAssets = true
        };

        // Act
        await imports.Import(importOptions);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        Assert.Equal(1, assertContext.Assets.Count());
        Assert.Equal(2, assertContext.Translations.Count());

        var assetEntity = await assertContext.Assets.FirstAsync(TestContext.Current.CancellationToken);
        Assert.Equal(assetToImport.Id, assetEntity.Id);
    }

    [Fact]
    public async Task Import_NewAssetAndCreateMissingFalse_DoesNotCreateNewAsset()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var changeLogService = new Mock<IChangeLog>();

        var imports = new ImportsLocalEF(serviceProvider, changeLogService.Object);

        var assetToImport = new AssetBuilder().Build();
        var sourceTransToImport = new TranslationBuilder()
            .WithAssetId(assetToImport.Id)
            .WithLanguageCode("en")
            .Build();

        var targetTransToImport = new TranslationBuilder()
            .WithAssetId(assetToImport.Id)
            .WithLanguageCode("fr")
            .Build();

        var importOptions = new ImportOptions
        {
            ImportData = new List<ImportData>
            {
                new()
                {
                    Asset = assetToImport,
                    SourceTranslation = sourceTransToImport,
                    TargetTranslation = targetTransToImport
                }
            },
            CreateMissingAssets = false
        };

        // Act
        await imports.Import(importOptions);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        Assert.Equal(0, assertContext.Assets.Count());
        Assert.Equal(0, assertContext.Translations.Count());
    }

    [Fact]
    public async Task Import_ExistingAssetAndOverwriteExistingTrue_OverwritesExistingAsset()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var existingAsset = new AssetEntityBuilder()
            .WithInternalName("OldName")
            .WithContextNotes("OldNotes")
            .Build();
        var existingSourceTrans = new TranslationEntityBuilder(existingAsset)
            .WithLanguage("en")
            .Build();
        var targetSourceTrans = TargetAssetTranslationEntityBuilder.Build(existingAsset, existingSourceTrans);

        context.Assets.Add(existingAsset);
        context.Translations.Add(existingSourceTrans);
        context.TargetAssetTranslations.Add(targetSourceTrans);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var changeLogService = new Mock<IChangeLog>();

        var imports = new ImportsLocalEF(serviceProvider, changeLogService.Object);

        var assetToImport = new AssetBuilder()
            .WithId(existingAsset.Id)
            .WithInternalName("NewName")
            .WithContextNotes("NewNotes")
            .Build();
        var sourceTransToImport = new TranslationBuilder()
            .WithAssetId(assetToImport.Id)
            .WithLanguageCode("en")
            .Build();

        var targetTransToImport = new TranslationBuilder()
            .WithAssetId(assetToImport.Id)
            .WithLanguageCode("fr")
            .Build();

        var importOptions = new ImportOptions
        {
            ImportData = new List<ImportData>
            {
                new()
                {
                    Asset = assetToImport,
                    SourceTranslation = sourceTransToImport,
                    TargetTranslation = targetTransToImport
                }
            },
            OverwriteExistingAssetProperties = true
        };

        // Act
        await imports.Import(importOptions);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assetEntity = await assertContext.Assets.FirstAsync(TestContext.Current.CancellationToken);
        Assert.Equal("NewName", assetEntity.InternalName);
        Assert.Equal("NewNotes", assetEntity.ContextNotes);
        Assert.True(assetEntity.LastUpdatedAt > existingAsset.LastUpdatedAt);
    }

    [Fact]
    public async Task Import_ExistingAssetAndOverwriteExistingFalse_DoesNotOverwriteExistingAsset()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var existingAsset = new AssetEntityBuilder()
            .WithInternalName("OldName")
            .WithContextNotes("OldNotes")
            .Build();
        var existingSourceTrans = new TranslationEntityBuilder(existingAsset)
            .WithLanguage("en")
            .Build();
        var targetSourceTrans = TargetAssetTranslationEntityBuilder.Build(existingAsset, existingSourceTrans);

        context.Assets.Add(existingAsset);
        context.Translations.Add(existingSourceTrans);
        context.TargetAssetTranslations.Add(targetSourceTrans);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var changeLogService = new Mock<IChangeLog>();

        var imports = new ImportsLocalEF(serviceProvider, changeLogService.Object);

        var assetToImport = new AssetBuilder()
            .WithId(existingAsset.Id)
            .WithInternalName("NewName")
            .WithContextNotes("NewNotes")
            .Build();
        var sourceTransToImport = new TranslationBuilder()
            .WithAssetId(assetToImport.Id)
            .WithLanguageCode("en")
            .Build();

        var targetTransToImport = new TranslationBuilder()
            .WithAssetId(assetToImport.Id)
            .WithLanguageCode("fr")
            .Build();

        var importOptions = new ImportOptions
        {
            ImportData = new List<ImportData>
            {
                new()
                {
                    Asset = assetToImport,
                    SourceTranslation = sourceTransToImport,
                    TargetTranslation = targetTransToImport
                }
            },
            OverwriteExistingAssetProperties = false
        };

        // Act
        await imports.Import(importOptions);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assetEntity = await assertContext.Assets.FirstAsync(TestContext.Current.CancellationToken);
        Assert.Equal("OldName", assetEntity.InternalName);
        Assert.Equal("OldNotes", assetEntity.ContextNotes);
        Assert.True(assetEntity.LastUpdatedAt == existingAsset.LastUpdatedAt);
    }

    [Theory]
    [InlineData(2, 5,
        1, 3,
        3, 0)]
    [InlineData(2, 5,
        3, 3,
        4, 0)]
    [InlineData(4, 5,
        4, 3,
        5, 0)]
    public async Task
        Import_ExistingAssetWithMajorChangesAndNewSourceTranslation_CreatesSourceAndTargetWithNewMajorVersion(
            int existingSourceMajorVersion, int existingSourceMinorVersion,
            int existingTargetMajorVersion, int existingTargetMinorVersion,
            int expectedMajorVersion, int expectedMinorVersion)
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var existingAsset = new AssetEntityBuilder().Build();
        var existingSourceTrans = new TranslationEntityBuilder(existingAsset)
            .WithContent("OldContent")
            .WithLanguage("en")
            .WithVersion(existingSourceMajorVersion, existingSourceMinorVersion)
            .Build();
        var targetSourceTrans = TargetAssetTranslationEntityBuilder.Build(existingAsset, existingSourceTrans);

        var existingTargetTrans = new TranslationEntityBuilder(existingAsset)
            .WithContent("AncienContenu")
            .WithLanguage("fr")
            .WithVersion(existingTargetMajorVersion, existingTargetMinorVersion)
            .Build();
        var targetTargetTrans = TargetAssetTranslationEntityBuilder.Build(existingAsset, existingTargetTrans);

        context.Assets.Add(existingAsset);
        context.Translations.AddRange(existingSourceTrans, existingTargetTrans);
        context.TargetAssetTranslations.AddRange(targetSourceTrans, targetTargetTrans);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var changeLogService = new Mock<IChangeLog>();

        var imports = new ImportsLocalEF(serviceProvider, changeLogService.Object);

        var assetToImport = new AssetBuilder().WithId(existingAsset.Id).Build();
        var sourceTransToImport = new TranslationBuilder()
            .WithAssetId(assetToImport.Id)
            .WithContent("NewContent")
            .WithLanguageCode("en")
            .Build();
        var targetTransToImport = new TranslationBuilder()
            .WithAssetId(assetToImport.Id)
            .WithContent("NouveauContenu")
            .WithLanguageCode("fr")
            .Build();

        var importOptions = new ImportOptions
        {
            ImportData = new List<ImportData>
            {
                new()
                {
                    Asset = assetToImport,
                    SourceTranslation = sourceTransToImport,
                    TargetTranslation = targetTransToImport
                }
            },
            ContainsMajorChanges = true
        };

        // Act
        await imports.Import(importOptions);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        Assert.Equal(1, assertContext.Assets.Count());
        Assert.Equal(4, assertContext.Translations.Count());

        var createdSourceTrans = await assertContext.Translations.FirstAsync(x => x.Id == sourceTransToImport.Id,
            TestContext.Current.CancellationToken);

        Assert.Equal(expectedMajorVersion, createdSourceTrans.MajorVersion);
        Assert.Equal(expectedMinorVersion, createdSourceTrans.MinorVersion);

        var createdTargetTrans = await assertContext.Translations.FirstAsync(x => x.Id == targetTransToImport.Id,
            TestContext.Current.CancellationToken);

        Assert.Equal(expectedMajorVersion, createdTargetTrans.MajorVersion);
        Assert.Equal(expectedMinorVersion, createdTargetTrans.MinorVersion);
    }

    [Theory]
    [InlineData(2, 5,
        1, 3,
        2, 0)]
    [InlineData(2, 5,
        3, 3,
        2, 0)]
    [InlineData(4, 5,
        4, 3,
        4, 4)]
    public async Task
        Import_ExistingAssetWithMajorChangesAndExistingSourceTranslation_CreatesTargetWithExistingMajorVersion(
            int existingSourceMajorVersion, int existingSourceMinorVersion,
            int existingTargetMajorVersion, int existingTargetMinorVersion,
            int expectedMajorVersion, int expectedMinorVersion)
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var existingAsset = new AssetEntityBuilder().Build();
        var existingSourceTrans = new TranslationEntityBuilder(existingAsset)
            .WithContent("OldContent")
            .WithLanguage("en")
            .WithVersion(existingSourceMajorVersion, existingSourceMinorVersion)
            .Build();
        var targetSourceTrans = TargetAssetTranslationEntityBuilder.Build(existingAsset, existingSourceTrans);

        var existingTargetTrans = new TranslationEntityBuilder(existingAsset)
            .WithContent("AncienContenu")
            .WithLanguage("fr")
            .WithVersion(existingTargetMajorVersion, existingTargetMinorVersion)
            .Build();
        var targetTargetTrans = TargetAssetTranslationEntityBuilder.Build(existingAsset, existingTargetTrans);

        context.Assets.Add(existingAsset);
        context.Translations.AddRange(existingSourceTrans, existingTargetTrans);
        context.TargetAssetTranslations.AddRange(targetSourceTrans, targetTargetTrans);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var changeLogService = new Mock<IChangeLog>();

        var imports = new ImportsLocalEF(serviceProvider, changeLogService.Object);

        var assetToImport = new AssetBuilder().WithId(existingAsset.Id).Build();
        var sourceTransToImport = new TranslationBuilder()
            .WithAssetId(assetToImport.Id)
            .WithContent("OldContent")
            .WithLanguageCode("en")
            .Build();
        var targetTransToImport = new TranslationBuilder()
            .WithAssetId(assetToImport.Id)
            .WithContent("NouveauContenu")
            .WithLanguageCode("fr")
            .Build();

        var importOptions = new ImportOptions
        {
            ImportData = new List<ImportData>
            {
                new()
                {
                    Asset = assetToImport,
                    SourceTranslation = sourceTransToImport,
                    TargetTranslation = targetTransToImport
                }
            },
            ContainsMajorChanges = true
        };

        // Act
        await imports.Import(importOptions);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        Assert.Equal(1, assertContext.Assets.Count());
        Assert.Equal(3, assertContext.Translations.Count());

        var createdSourceTrans = await assertContext.Translations.FirstOrDefaultAsync(
            x => x.Id == sourceTransToImport.Id,
            TestContext.Current.CancellationToken);
        Assert.Null(createdSourceTrans);

        var createdTargetTrans = await assertContext.Translations.FirstAsync(x => x.Id == targetTransToImport.Id,
            TestContext.Current.CancellationToken);

        Assert.Equal(expectedMajorVersion, createdTargetTrans.MajorVersion);
        Assert.Equal(expectedMinorVersion, createdTargetTrans.MinorVersion);
    }

    [Theory]
    [InlineData(2, 2,
        1, 3,
        2, 3, 0)]
    [InlineData(2, 7,
        3, 3,
        2, 8, 0)]
    [InlineData(4, 5,
        4, 3,
        4, 6, 4)]
    public async Task
        Import_ExistingAssetWithMinorChangesAndNewSourceTranslation_CreatesSourceAndTargetWithExistingMajorVersion(
            int existingSourceMajorVersion, int existingSourceMinorVersion,
            int existingTargetMajorVersion, int existingTargetMinorVersion,
            int expectedMajorVersion, int expectedSourceMinorVersion, int expectedTargetMinorVersion)
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var existingAsset = new AssetEntityBuilder().Build();
        var existingSourceTrans = new TranslationEntityBuilder(existingAsset)
            .WithContent("OldContent")
            .WithLanguage("en")
            .WithVersion(existingSourceMajorVersion, existingSourceMinorVersion)
            .Build();
        var targetSourceTrans = TargetAssetTranslationEntityBuilder.Build(existingAsset, existingSourceTrans);

        var existingTargetTrans = new TranslationEntityBuilder(existingAsset)
            .WithContent("AncienContenu")
            .WithLanguage("fr")
            .WithVersion(existingTargetMajorVersion, existingTargetMinorVersion)
            .Build();
        var targetTargetTrans = TargetAssetTranslationEntityBuilder.Build(existingAsset, existingTargetTrans);

        context.Assets.Add(existingAsset);
        context.Translations.AddRange(existingSourceTrans, existingTargetTrans);
        context.TargetAssetTranslations.AddRange(targetSourceTrans, targetTargetTrans);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var changeLogService = new Mock<IChangeLog>();

        var imports = new ImportsLocalEF(serviceProvider, changeLogService.Object);

        var assetToImport = new AssetBuilder().WithId(existingAsset.Id).Build();
        var sourceTransToImport = new TranslationBuilder()
            .WithAssetId(assetToImport.Id)
            .WithContent("NewContent")
            .WithLanguageCode("en")
            .Build();
        var targetTransToImport = new TranslationBuilder()
            .WithAssetId(assetToImport.Id)
            .WithContent("NouveauContenu")
            .WithLanguageCode("fr")
            .Build();

        var importOptions = new ImportOptions
        {
            ImportData = new List<ImportData>
            {
                new()
                {
                    Asset = assetToImport,
                    SourceTranslation = sourceTransToImport,
                    TargetTranslation = targetTransToImport
                }
            },
            ContainsMajorChanges = false
        };

        // Act
        await imports.Import(importOptions);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        Assert.Equal(1, assertContext.Assets.Count());
        Assert.Equal(4, assertContext.Translations.Count());

        var createdSourceTrans = await assertContext.Translations.FirstAsync(x => x.Id == sourceTransToImport.Id,
            TestContext.Current.CancellationToken);

        Assert.Equal(expectedMajorVersion, createdSourceTrans.MajorVersion);
        Assert.Equal(expectedSourceMinorVersion, createdSourceTrans.MinorVersion);

        var createdTargetTrans = await assertContext.Translations.FirstAsync(x => x.Id == targetTransToImport.Id,
            TestContext.Current.CancellationToken);

        Assert.Equal(expectedMajorVersion, createdTargetTrans.MajorVersion);
        Assert.Equal(expectedTargetMinorVersion, createdTargetTrans.MinorVersion);
    }

    [Theory]
    [InlineData(2, 5,
        1, 3,
        2, 0)]
    [InlineData(2, 5,
        3, 3,
        2, 0)]
    [InlineData(4, 5,
        4, 3,
        4, 4)]
    public async Task
        Import_ExistingAssetWithMinorChangesAndExistingSourceTranslation_CreatesTargetWithExistingMajorVersion(
            int existingSourceMajorVersion, int existingSourceMinorVersion,
            int existingTargetMajorVersion, int existingTargetMinorVersion,
            int expectedMajorVersion, int expectedTargetMinorVersion)
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var existingAsset = new AssetEntityBuilder().Build();
        var existingSourceTrans = new TranslationEntityBuilder(existingAsset)
            .WithContent("OldContent")
            .WithLanguage("en")
            .WithVersion(existingSourceMajorVersion, existingSourceMinorVersion)
            .Build();
        var targetSourceTrans = TargetAssetTranslationEntityBuilder.Build(existingAsset, existingSourceTrans);

        var existingTargetTrans = new TranslationEntityBuilder(existingAsset)
            .WithContent("AncienContenu")
            .WithLanguage("fr")
            .WithVersion(existingTargetMajorVersion, existingTargetMinorVersion)
            .Build();
        var targetTargetTrans = TargetAssetTranslationEntityBuilder.Build(existingAsset, existingTargetTrans);

        context.Assets.Add(existingAsset);
        context.Translations.AddRange(existingSourceTrans, existingTargetTrans);
        context.TargetAssetTranslations.AddRange(targetSourceTrans, targetTargetTrans);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var changeLogService = new Mock<IChangeLog>();

        var imports = new ImportsLocalEF(serviceProvider, changeLogService.Object);

        var assetToImport = new AssetBuilder().WithId(existingAsset.Id).Build();
        var sourceTransToImport = new TranslationBuilder()
            .WithAssetId(assetToImport.Id)
            .WithContent("OldContent")
            .WithLanguageCode("en")
            .Build();
        var targetTransToImport = new TranslationBuilder()
            .WithAssetId(assetToImport.Id)
            .WithContent("NouveauContenu")
            .WithLanguageCode("fr")
            .Build();

        var importOptions = new ImportOptions
        {
            ImportData = new List<ImportData>
            {
                new()
                {
                    Asset = assetToImport,
                    SourceTranslation = sourceTransToImport,
                    TargetTranslation = targetTransToImport
                }
            },
            ContainsMajorChanges = false
        };

        // Act
        await imports.Import(importOptions);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        Assert.Equal(1, assertContext.Assets.Count());
        Assert.Equal(3, assertContext.Translations.Count());

        var createdSourceTrans = await assertContext.Translations.FirstOrDefaultAsync(
            x => x.Id == sourceTransToImport.Id,
            TestContext.Current.CancellationToken);
        Assert.Null(createdSourceTrans);

        var createdTargetTrans = await assertContext.Translations.FirstAsync(x => x.Id == targetTransToImport.Id,
            TestContext.Current.CancellationToken);

        Assert.Equal(expectedMajorVersion, createdTargetTrans.MajorVersion);
        Assert.Equal(expectedTargetMinorVersion, createdTargetTrans.MinorVersion);
    }

    [Fact]
    public async Task Import_TranslationCreated_CreatesTargets()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var changeLogService = new Mock<IChangeLog>();

        var imports = new ImportsLocalEF(serviceProvider, changeLogService.Object);

        var assetToImport = new AssetBuilder().Build();
        var sourceTransToImport = new TranslationBuilder()
            .WithAssetId(assetToImport.Id)
            .WithLanguageCode("en")
            .Build();

        var targetTransToImport = new TranslationBuilder()
            .WithAssetId(assetToImport.Id)
            .WithLanguageCode("fr")
            .Build();

        var importOptions = new ImportOptions
        {
            ImportData = new List<ImportData>
            {
                new()
                {
                    Asset = assetToImport,
                    SourceTranslation = sourceTransToImport,
                    TargetTranslation = targetTransToImport
                }
            },
            CreateMissingAssets = true
        };

        // Act
        await imports.Import(importOptions);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        Assert.Equal(2, assertContext.TargetAssetTranslations.Count());
    }

    [Fact]
    public async Task Import_AssetsModifiedAndCreatedAndTranslationsCreated_CreatesChangeLogWithCorrectValues()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var existingAssetA = new AssetEntityBuilder().Build();
        var existingAssetB = new AssetEntityBuilder().Build();
        var existingAssetC = new AssetEntityBuilder().Build();

        var existingEnTransA = new TranslationEntityBuilder(existingAssetA).WithLanguage("en").Build();
        var existingEnTransB = new TranslationEntityBuilder(existingAssetB).WithLanguage("en").Build();
        var existingEnTransC = new TranslationEntityBuilder(existingAssetC).WithLanguage("en").Build();

        var assetATarget = TargetAssetTranslationEntityBuilder.Build(existingAssetA, existingEnTransA);
        var assetBTarget = TargetAssetTranslationEntityBuilder.Build(existingAssetB, existingEnTransB);
        var assetCTarget = TargetAssetTranslationEntityBuilder.Build(existingAssetC, existingEnTransC);

        var existingEsTransA = new TranslationEntityBuilder(existingAssetA).WithLanguage("es").Build();
        var assetAEsTarget = TargetAssetTranslationEntityBuilder.Build(existingAssetA, existingEsTransA);

        await context.Assets.AddRangeAsync(existingAssetA, existingAssetB, existingAssetC);
        await context.Translations.AddRangeAsync(existingEnTransA, existingEnTransB, existingEnTransC,
            existingEsTransA);
        await context.TargetAssetTranslations.AddRangeAsync(assetATarget, assetBTarget, assetCTarget, assetAEsTarget);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var changeLogService = new Mock<IChangeLog>();

        var imports = new ImportsLocalEF(serviceProvider, changeLogService.Object);

        var brandNewAsset = new AssetEntityBuilder().Build();

        var importData = new ImportOptions
        {
            CreateMissingAssets = true,
            OverwriteExistingAssetProperties = true,
            ImportData =
            [
                new ImportData
                {
                    Asset = new AssetEntityBuilder()
                        .WithId(existingAssetA.Id)
                        .WithInternalName("New Internal Name")
                        .Build(),
                    SourceTranslation = new TranslationEntityBuilder(existingAssetA)
                        .WithLanguage("en")
                        .Build(),
                    TargetTranslation = new TranslationEntityBuilder(existingAssetA)
                        .WithLanguage("es")
                        .Build()
                },

                new ImportData
                {
                    Asset = new AssetEntityBuilder()
                        .WithId(existingAssetB.Id)
                        .Build(),
                    SourceTranslation = new TranslationEntityBuilder(existingAssetB)
                        .WithLanguage("en")
                        .Build(),
                    TargetTranslation = new TranslationEntityBuilder(existingAssetB)
                        .WithLanguage("es")
                        .Build()
                },

                new ImportData
                {
                    Asset = new AssetEntityBuilder()
                        .WithId(existingAssetC.Id)
                        .Build(),
                    SourceTranslation = new TranslationEntityBuilder(existingAssetC)
                        .WithLanguage("en")
                        .WithContent("Changed english content")
                        .Build(),
                    TargetTranslation = new TranslationEntityBuilder(existingAssetC)
                        .WithLanguage("es")
                        .Build()
                },

                new ImportData
                {
                    Asset = brandNewAsset,
                    SourceTranslation = new TranslationEntityBuilder(brandNewAsset)
                        .WithLanguage("en")
                        .Build(),
                    TargetTranslation = new TranslationEntityBuilder(brandNewAsset)
                        .WithLanguage("es")
                        .Build()
                }
            ]
        };

        // Act
        await imports.Import(importData);

        // Assert
        changeLogService.Verify(x => x.Create(It.Is<ChangeLogEntry>(entry =>
            entry.ResourceType == ResourceType.Translation &&
            entry.ResourceAction == ResourceAction.Imported &&
            entry.Message.Contains("Imported 1 asset") &&
            entry.Message.Contains("updated 1 asset") &&
            entry.Message.Contains("6 new translations")
        )), Times.Once);
    }
}