using Lame.Backend.EntityFramework;
using Lame.Backend.EntityFramework.Models;
using Lame.Backend.EntityFramework.Tests;
using Lame.Backend.EntityFramework.Tests.EntityBuilders;
using Lame.Backend.Exports.Models;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Lame.Backend.Exports.LocalEF.Tests;

public class ExportsLocalEFTests
{
    [Fact]
    public async Task Export_WithAllTranslationStatusFilter_ExportsAllAssets()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var asset = new AssetEntityBuilder().Build();

        var translationEn = new TranslationEntity { Id = Guid.NewGuid(), Language = "en", Content = "Hello" };
        var targetTranslationEn = new TargetAssetTranslationEntity
        {
            AssetId = asset.Id,
            TranslationId = translationEn.Id,
            Asset = asset,
            Translation = translationEn,
            Language = "en"
        };

        var translationFr = new TranslationEntity { Id = Guid.NewGuid(), Language = "fr", Content = "Bonjour" };
        var targetTranslationFr = new TargetAssetTranslationEntity
        {
            AssetId = asset.Id,
            TranslationId = translationFr.Id,
            Asset = asset,
            Translation = translationFr,
            Language = "fr"
        };

        context.Assets.Add(asset);
        context.Translations.AddRange(translationEn, translationFr);
        context.TargetAssetTranslations.AddRange(targetTranslationEn, targetTranslationFr);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var exporterMock = new Mock<IExporter>();
        exporterMock.Setup(e => e.Export(It.IsAny<List<AssetExportData>>(), "en", "fr"))
            .Returns(new byte[] { 1, 2, 3 });

        var exporterFactoryMock = new Mock<IExporterFactory>();
        exporterFactoryMock.Setup(f => f.GetExporter(ExportFormatType.JSON)).Returns(exporterMock.Object);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var exportsLocalEf = new ExportsLocalEF(serviceProvider, exporterFactoryMock.Object);

        var options = new ExportOptions
        {
            Format = ExportFormatType.JSON,
            LanguageCode = "fr",
            TranslationStatusFilter = ExportTranslationStatusFilter.All,
            Tags = []
        };

        // Act
        var result = await exportsLocalEf.Export(options);

        // Assert
        Assert.Equal(new byte[] { 1, 2, 3 }, result);

        exporterMock.Verify(e =>
                e.Export(
                    It.Is<List<AssetExportData>>(records =>
                        // Assert that the one asset is provided
                        records.Count == 1 &&
                        records[0].Id == asset.Id &&
                        records[0].InternalName == asset.InternalName &&

                        // Assert that the asst's source translation is populated correctly
                        records[0].SourceTranslation != null &&
                        records[0].SourceTranslation.Id == translationEn.Id &&
                        records[0].SourceTranslation.Content == translationEn.Content &&

                        // Assert that the asst's target translation is populated correctly
                        records[0].TargetTranslation != null &&
                        records[0].TargetTranslation.Id == translationFr.Id &&
                        records[0].TargetTranslation.Content == translationFr.Content
                    ),
                    "en", "fr"),
            Times.Once);
    }

    [Fact]
    public async Task Export_WithNoAssets_ReturnsEmptyExport()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var exporterMock = new Mock<IExporter>();
        var exporterFactoryMock = new Mock<IExporterFactory>();
        exporterFactoryMock.Setup(f => f.GetExporter(ExportFormatType.JSON)).Returns(exporterMock.Object);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var exportsLocalEf = new ExportsLocalEF(serviceProvider, exporterFactoryMock.Object);

        var options = new ExportOptions
        {
            Format = ExportFormatType.JSON,
            LanguageCode = "fr",
            TranslationStatusFilter = ExportTranslationStatusFilter.All,
            Tags = []
        };

        // Act
        var result = await exportsLocalEf.Export(options);

        // Assert
        Assert.Empty(result);
        exporterMock.Verify(e => e.Export(
                It.Is<List<AssetExportData>>(l => l.Count == 0),
                "en",
                "fr"),
            Times.Once);
    }

    [Fact]
    public async Task Export_WithNullExportOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var exporterFactoryMock = new Mock<IExporterFactory>();
        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var exportsLocalEf = new ExportsLocalEF(serviceProvider, exporterFactoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => exportsLocalEf.Export(null));
    }

    [Fact]
    public async Task Export_WithNoMatchingTargetLanguage_ReturnsEmptyExport()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var asset = new AssetEntityBuilder().Build();
        var translationEn = new TranslationEntityBuilder(asset).WithLanguage("en").Build();
        var targetTranslationEn = TargetAssetTranslationEntityBuilder.Build(asset, translationEn);

        context.Assets.Add(asset);
        context.Translations.Add(translationEn);
        context.TargetAssetTranslations.Add(targetTranslationEn);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var exporterMock = new Mock<IExporter>();
        exporterMock.Setup(e => e.Export(It.Is<List<AssetExportData>>(l => l.Count == 0), "en", "de"))
            .Returns(new byte[0]);

        var exporterFactoryMock = new Mock<IExporterFactory>();
        exporterFactoryMock.Setup(f => f.GetExporter(ExportFormatType.JSON)).Returns(exporterMock.Object);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var exportsLocalEf = new ExportsLocalEF(serviceProvider, exporterFactoryMock.Object);

        var options = new ExportOptions
        {
            Format = ExportFormatType.JSON,
            LanguageCode = "de",
            TranslationStatusFilter = ExportTranslationStatusFilter.Complete,
            Tags = []
        };

        // Act
        var result = await exportsLocalEf.Export(options);

        // Assert
        Assert.Empty(result);
        exporterMock.Verify(e => e.Export(
                It.Is<List<AssetExportData>>(l => l.Count == 0),
                "en",
                "de"),
            Times.Once);
    }

    [Fact]
    public async Task Export_WithTagFilterAny_ReturnsAssetsWithAnyMatchingTag()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tag1 = new TagEntityBuilder().Build();
        var tag2 = new TagEntityBuilder().Build();

        var asset1 = new AssetEntityBuilder().AddTag(tag1).Build();
        var asset2 = new AssetEntityBuilder().AddTag(tag2).Build();
        var asset3 = new AssetEntityBuilder().Build();

        context.Tags.AddRange(tag1, tag2);
        context.Assets.AddRange(asset1, asset2, asset3);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var exporterMock = new Mock<IExporter>();
        exporterMock.Setup(e => e.Export(It.IsAny<List<AssetExportData>>(), "en", "fr"))
            .Returns(new byte[] { 1 });

        var exporterFactoryMock = new Mock<IExporterFactory>();
        exporterFactoryMock.Setup(f => f.GetExporter(ExportFormatType.JSON)).Returns(exporterMock.Object);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var exportsLocalEf = new ExportsLocalEF(serviceProvider, exporterFactoryMock.Object);

        var options = new ExportOptions
        {
            Format = ExportFormatType.JSON,
            LanguageCode = "fr",
            TagFilter = ExportTagFilterType.Any,
            Tags = [tag1, tag2]
        };

        // Act
        var result = await exportsLocalEf.Export(options);

        // Assert
        exporterMock.Verify(e =>
                e.Export(
                    It.Is<List<AssetExportData>>(records =>
                        records.Count == 2 &&
                        records.Any(r => r.Id == asset1.Id) &&
                        records.Any(r => r.Id == asset2.Id)
                    ),
                    "en", "fr"),
            Times.Once);
        Assert.Equal(new byte[] { 1 }, result);
    }

    [Fact]
    public async Task Export_WithTagFilterAnyAndNoTagsProvided_ExportsAllAssets()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tag1 = new TagEntityBuilder().Build();

        var asset1 = new AssetEntityBuilder().Build();
        var asset2 = new AssetEntityBuilder().AddTag(tag1).Build();

        context.Assets.AddRange(asset1, asset2);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var exporterMock = new Mock<IExporter>();
        exporterMock.Setup(e => e.Export(It.IsAny<List<AssetExportData>>(), "en", "fr"))
            .Returns(new byte[] { 4 });

        var exporterFactoryMock = new Mock<IExporterFactory>();
        exporterFactoryMock.Setup(f => f.GetExporter(ExportFormatType.JSON)).Returns(exporterMock.Object);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var exportsLocalEf = new ExportsLocalEF(serviceProvider, exporterFactoryMock.Object);

        var options = new ExportOptions
        {
            Format = ExportFormatType.JSON,
            LanguageCode = "fr",
            TagFilter = ExportTagFilterType.Any,
            Tags = []
        };

        // Act
        var result = await exportsLocalEf.Export(options);

        // Assert
        exporterMock.Verify(e =>
                e.Export(
                    It.Is<List<AssetExportData>>(records =>
                        records.Count == 2 &&
                        records.Any(r => r.Id == asset1.Id) &&
                        records.Any(r => r.Id == asset2.Id)
                    ),
                    "en", "fr"),
            Times.Once);
        Assert.Equal(new byte[] { 4 }, result);
    }

    [Fact]
    public async Task Export_WithTagFilterAnyAndTagsProvided_AssetWithNoTagsIsNotExported()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tag1 = new TagEntityBuilder().Build();
        var asset1 = new AssetEntityBuilder().Build();

        context.Tags.Add(tag1);
        context.Assets.Add(asset1);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var exporterMock = new Mock<IExporter>();
        exporterMock.Setup(e => e.Export(It.IsAny<List<AssetExportData>>(), "en", "fr"))
            .Returns(new byte[0]);

        var exporterFactoryMock = new Mock<IExporterFactory>();
        exporterFactoryMock.Setup(f => f.GetExporter(ExportFormatType.JSON)).Returns(exporterMock.Object);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var exportsLocalEf = new ExportsLocalEF(serviceProvider, exporterFactoryMock.Object);

        var options = new ExportOptions
        {
            Format = ExportFormatType.JSON,
            LanguageCode = "fr",
            TagFilter = ExportTagFilterType.Any,
            Tags = [tag1]
        };

        // Act
        var result = await exportsLocalEf.Export(options);

        // Assert
        exporterMock.Verify(e =>
                e.Export(
                    It.Is<List<AssetExportData>>(records => records.Count == 0),
                    "en", "fr"),
            Times.Once);
        Assert.Empty(result);
    }


    [Fact]
    public async Task Export_WithTagFilterAll_ReturnsAssetsWithAllTags()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tag1 = new TagEntityBuilder().Build();
        var tag2 = new TagEntityBuilder().Build();

        var asset1 = new AssetEntityBuilder().AddTag(tag1).AddTag(tag2).Build();
        var asset2 = new AssetEntityBuilder().AddTag(tag1).Build();

        context.Tags.AddRange(tag1, tag2);
        context.Assets.AddRange(asset1, asset2);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var exporterMock = new Mock<IExporter>();
        exporterMock.Setup(e => e.Export(It.IsAny<List<AssetExportData>>(), "en", "fr"))
            .Returns(new byte[] { 2 });

        var exporterFactoryMock = new Mock<IExporterFactory>();
        exporterFactoryMock.Setup(f => f.GetExporter(ExportFormatType.JSON)).Returns(exporterMock.Object);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var exportsLocalEf = new ExportsLocalEF(serviceProvider, exporterFactoryMock.Object);

        var options = new ExportOptions
        {
            Format = ExportFormatType.JSON,
            LanguageCode = "fr",
            TagFilter = ExportTagFilterType.All,
            Tags = [tag1, tag2]
        };

        // Act
        var result = await exportsLocalEf.Export(options);

        // Assert
        exporterMock.Verify(e =>
                e.Export(
                    It.Is<List<AssetExportData>>(records =>
                        records.Count == 1 &&
                        records[0].Id == asset1.Id
                    ),
                    "en", "fr"),
            Times.Once);
        Assert.Equal(new byte[] { 2 }, result);
    }

    [Fact]
    public async Task Export_WithTagFilterOnly_ReturnsAssetsWithOnlySpecifiedTags()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tag1 = new TagEntityBuilder().Build();
        var tag2 = new TagEntityBuilder().Build();

        var asset1 = new AssetEntityBuilder().AddTag(tag1).AddTag(tag2).Build();
        var asset2 = new AssetEntityBuilder().AddTag(tag1).Build();

        context.Tags.AddRange(tag1, tag2);
        context.Assets.AddRange(asset1, asset2);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var exporterMock = new Mock<IExporter>();
        exporterMock.Setup(e => e.Export(It.IsAny<List<AssetExportData>>(), "en", "fr"))
            .Returns(new byte[] { 3 });

        var exporterFactoryMock = new Mock<IExporterFactory>();
        exporterFactoryMock.Setup(f => f.GetExporter(ExportFormatType.JSON)).Returns(exporterMock.Object);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var exportsLocalEf = new ExportsLocalEF(serviceProvider, exporterFactoryMock.Object);

        var options = new ExportOptions
        {
            Format = ExportFormatType.JSON,
            LanguageCode = "fr",
            TagFilter = ExportTagFilterType.Only,
            Tags = [tag1]
        };

        // Act
        var result = await exportsLocalEf.Export(options);

        // Assert
        exporterMock.Verify(e =>
                e.Export(
                    It.Is<List<AssetExportData>>(records =>
                        records.Count == 1 &&
                        records[0].Id == asset2.Id
                    ),
                    "en", "fr"),
            Times.Once);
        Assert.Equal(new byte[] { 3 }, result);
    }

    [Fact]
    public async Task Export_WithTagFilterAndNoMatchingAssets_ReturnsEmpty()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tag1 = new TagEntityBuilder().Build();
        var tag2 = new TagEntityBuilder().Build();

        var asset1 = new AssetEntityBuilder().AddTag(tag1).Build();

        context.Tags.AddRange(tag1, tag2);
        context.Assets.Add(asset1);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var exporterMock = new Mock<IExporter>();
        exporterMock.Setup(e => e.Export(It.IsAny<List<AssetExportData>>(), "en", "fr"))
            .Returns(new byte[0]);

        var exporterFactoryMock = new Mock<IExporterFactory>();
        exporterFactoryMock.Setup(f => f.GetExporter(ExportFormatType.JSON)).Returns(exporterMock.Object);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var exportsLocalEf = new ExportsLocalEF(serviceProvider, exporterFactoryMock.Object);

        var options = new ExportOptions
        {
            Format = ExportFormatType.JSON,
            LanguageCode = "fr",
            TagFilter = ExportTagFilterType.All,
            Tags = [tag2]
        };

        // Act
        var result = await exportsLocalEf.Export(options);

        // Assert
        exporterMock.Verify(e =>
                e.Export(
                    It.Is<List<AssetExportData>>(records => records.Count == 0),
                    "en", "fr"),
            Times.Once);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Export_WithTagFilterOnlyAndNoTagsProvided_ExportsOnlyAssetsWithNoTags()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tag1 = new TagEntityBuilder().Build();
        var assetWithTag = new AssetEntityBuilder().AddTag(tag1).Build();
        var assetWithoutTag = new AssetEntityBuilder().Build();

        context.Tags.Add(tag1);
        context.Assets.AddRange(assetWithTag, assetWithoutTag);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var exporterMock = new Mock<IExporter>();
        exporterMock.Setup(e => e.Export(It.IsAny<List<AssetExportData>>(), "en", "fr"))
            .Returns(new byte[] { 5 });

        var exporterFactoryMock = new Mock<IExporterFactory>();
        exporterFactoryMock.Setup(f => f.GetExporter(ExportFormatType.JSON)).Returns(exporterMock.Object);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var exportsLocalEf = new ExportsLocalEF(serviceProvider, exporterFactoryMock.Object);

        var options = new ExportOptions
        {
            Format = ExportFormatType.JSON,
            LanguageCode = "fr",
            TagFilter = ExportTagFilterType.Only,
            Tags = []
        };

        // Act
        var result = await exportsLocalEf.Export(options);

        // Assert
        exporterMock.Verify(e =>
                e.Export(
                    It.Is<List<AssetExportData>>(records =>
                        records.Count == 1 &&
                        records[0].Id == assetWithoutTag.Id
                    ),
                    "en", "fr"),
            Times.Once);
        Assert.Equal(new byte[] { 5 }, result);
    }
}