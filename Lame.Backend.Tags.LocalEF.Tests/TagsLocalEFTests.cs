using Lame.Backend.EntityFramework;
using Lame.Backend.EntityFramework.Models;
using Lame.Backend.EntityFramework.Tests;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Backend.Tags.LocalEF.Tests;

public class TagsLocalEFTests
{
    [Fact]
    public async Task Get_WhenNoTagsExist_ReturnsEmptyList()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act
        var result = await tagsLocalEf.Get();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Get_WhenMultipleTagsExist_ReturnsTagsOrderedByName()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tags = new List<TagEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "zeta" },
            new() { Id = Guid.NewGuid(), Name = "alpha" },
            new() { Id = Guid.NewGuid(), Name = "beta" }
        };
        context.Tags.AddRange(tags);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act
        var result = await tagsLocalEf.Get();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("alpha", result[0].Name);
        Assert.Equal("beta", result[1].Name);
        Assert.Equal("zeta", result[2].Name);
    }

    [Fact]
    public async Task Get_WithExactMatch_ReturnsTagFirst()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tags = new List<TagEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "alpha" },
            new() { Id = Guid.NewGuid(), Name = "beta" },
            new() { Id = Guid.NewGuid(), Name = "alpha-2" }
        };
        context.Tags.AddRange(tags);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act
        var result = await tagsLocalEf.Get("alpha", 3);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("alpha", result[0].Name);
        Assert.Equal("alpha-2", result[1].Name);
    }

    [Fact]
    public async Task Get_WithStartsWithMatch_ReturnsStartsWithBeforeContains()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tags = new List<TagEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "start" },
            new() { Id = Guid.NewGuid(), Name = "starter" },
            new() { Id = Guid.NewGuid(), Name = "endstart" }
        };
        context.Tags.AddRange(tags);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act
        var result = await tagsLocalEf.Get("start", 3);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("start", result[0].Name);
        Assert.Equal("starter", result[1].Name);
        Assert.Equal("endstart", result[2].Name);
    }

    [Fact]
    public async Task Get_WithCaseInsensitiveSearch_ReturnsMatchingTags()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tags = new List<TagEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Alpha" },
            new() { Id = Guid.NewGuid(), Name = "BETA" },
            new() { Id = Guid.NewGuid(), Name = "gamma" }
        };
        context.Tags.AddRange(tags);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act
        var result = await tagsLocalEf.Get("beta", 2);

        // Assert
        Assert.Single(result);
        Assert.Equal("BETA", result[0].Name);
    }

    [Fact]
    public async Task Get_WithLimitLessThanResults_ReturnsLimitedResults()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tags = new List<TagEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "apple" },
            new() { Id = Guid.NewGuid(), Name = "apricot" },
            new() { Id = Guid.NewGuid(), Name = "banana" }
        };
        context.Tags.AddRange(tags);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act
        var result = await tagsLocalEf.Get("a", 2);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Get_WithNoMatchingTags_ReturnsEmptyList()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tags = new List<TagEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "apple" },
            new() { Id = Guid.NewGuid(), Name = "banana" }
        };
        context.Tags.AddRange(tags);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act
        var result = await tagsLocalEf.Get("zebra", 5);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTagsForResource_AssetWithTags_ReturnsAssetTags()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assetId = Guid.NewGuid();
        var tag = new TagEntity { Id = Guid.NewGuid(), Name = "alpha" };
        var asset = new AssetEntity { Id = assetId, InternalName = "asset", Tags = new List<TagEntity> { tag } };

        context.Tags.Add(tag);
        context.Assets.Add(asset);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act
        var result = await tagsLocalEf.GetTagsForResource(assetId);

        // Assert
        Assert.Single(result);
        Assert.Equal(tag.Id, result[0].Id);
    }

    [Fact]
    public async Task GetTagsForResource_TranslationWithTags_ReturnsTranslationTags()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var translationId = Guid.NewGuid();
        var tag = new TagEntity { Id = Guid.NewGuid(), Name = "alpha" };
        var translation = new TranslationEntity
            { Id = translationId, Language = "en", Tags = new List<TagEntity> { tag } };

        context.Tags.Add(tag);
        context.Translations.Add(translation);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act
        var result = await tagsLocalEf.GetTagsForResource(translationId);

        // Assert
        Assert.Single(result);
        Assert.Equal(tag.Id, result[0].Id);
    }

    [Fact]
    public async Task GetTagsForResource_ResourceWithNoTags_ReturnsEmptyList()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assetId = Guid.NewGuid();
        context.Assets.Add(new AssetEntity { Id = assetId, InternalName = "asset" });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act
        var result = await tagsLocalEf.GetTagsForResource(assetId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetResourcesWithTag_AssetType_ReturnsAssetIdsWithTag()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tag = new TagEntity { Id = Guid.NewGuid(), Name = "tag1" };
        var asset = new AssetEntity { Id = Guid.NewGuid(), InternalName = "asset", Tags = new List<TagEntity> { tag } };

        context.Tags.Add(tag);
        context.Assets.Add(asset);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act
        var result = await tagsLocalEf.GetResourcesWithTag(tag.Id, ResourceType.Asset);

        // Assert
        Assert.Single(result);
        Assert.Equal(asset.Id, result[0]);
    }

    [Fact]
    public async Task GetResourcesWithTag_TranslationType_ReturnsTranslationIdsWithTag()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tag = new TagEntity { Id = Guid.NewGuid(), Name = "tag1" };
        var translation = new TranslationEntity
            { Id = Guid.NewGuid(), Language = "en", Tags = new List<TagEntity> { tag } };

        context.Tags.Add(tag);
        context.Translations.Add(translation);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act
        var result = await tagsLocalEf.GetResourcesWithTag(tag.Id, ResourceType.Translation);

        // Assert
        Assert.Single(result);
        Assert.Equal(translation.Id, result[0]);
    }

    [Fact]
    public async Task GetResourcesWithTag_InvalidResourceType_ThrowsArgumentException()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await tagsLocalEf.GetResourcesWithTag(Guid.NewGuid(), (ResourceType)999);
        });
    }

    [Fact]
    public async Task AddTagToResource_TagDoesNotExist_CreatesTagAndAddsToAsset()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var asset = new AssetEntity { Id = Guid.NewGuid(), InternalName = "asset" };
        var tag = new Tag { Id = Guid.NewGuid(), Name = "new_tag" };

        context.Assets.Add(asset);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act
        await tagsLocalEf.AddTagToResource(tag, asset.Id, ResourceType.Asset);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);
        var updatedAsset = await assertContext.Assets
            .Include(a => a.Tags)
            .FirstAsync(a => a.Id == asset.Id, TestContext.Current.CancellationToken);

        Assert.Single(updatedAsset.Tags);
        Assert.Equal(tag.Id, updatedAsset.Tags.First().Id);
    }

    [Fact]
    public async Task AddTagToResource_TagExists_AddsTagToTranslation()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tagEntity = new TagEntity { Id = Guid.NewGuid(), Name = "existing_tag" };
        var translation = new TranslationEntity { Id = Guid.NewGuid(), Language = "en" };

        context.Tags.Add(tagEntity);
        context.Translations.Add(translation);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act
        await tagsLocalEf.AddTagToResource(new Tag { Id = tagEntity.Id, Name = tagEntity.Name }, translation.Id,
            ResourceType.Translation);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);
        var updatedTranslation =
            await assertContext.Translations
                .Include(t => t.Tags)
                .FirstAsync(t => t.Id == translation.Id, TestContext.Current.CancellationToken);
        Assert.Single(updatedTranslation.Tags);
        Assert.Equal(tagEntity.Id, updatedTranslation.Tags.First().Id);
    }

    [Fact]
    public async Task AddTagToResource_InvalidResourceType_ThrowsArgumentException()
    {
        // Arrange
        var tag = new Tag { Id = Guid.NewGuid(), Name = "tag" };

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await tagsLocalEf.AddTagToResource(tag, Guid.NewGuid(), (ResourceType)999);
        });
    }

    [Fact]
    public async Task AddTagToResource_ResourceDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tagEntity = new TagEntity { Id = Guid.NewGuid(), Name = "tag" };

        context.Tags.Add(tagEntity);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await tagsLocalEf.AddTagToResource(new Tag { Id = tagEntity.Id, Name = tagEntity.Name }, Guid.NewGuid(),
                ResourceType.Asset);
        });
    }

    [Fact]
    public async Task AddTagToResource_TagAlreadyOnResource_DoesNotDuplicate()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tagEntity = new TagEntity { Id = Guid.NewGuid(), Name = "tag" };
        var asset = new AssetEntity
            { Id = Guid.NewGuid(), InternalName = "asset", Tags = new List<TagEntity> { tagEntity } };

        context.Tags.Add(tagEntity);
        context.Assets.Add(asset);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act
        await tagsLocalEf.AddTagToResource(new Tag { Id = tagEntity.Id, Name = tagEntity.Name }, asset.Id,
            ResourceType.Asset);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);
        var updatedAsset = await assertContext.Assets
            .Include(a => a.Tags)
            .FirstAsync(a => a.Id == asset.Id, TestContext.Current.CancellationToken);

        Assert.Single(updatedAsset.Tags);
        Assert.Equal(tagEntity.Id, updatedAsset.Tags.First().Id);
    }

    [Fact]
    public async Task RemoveTagFromResource_TagOnAsset_RemovesTag()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tag = new TagEntity { Id = Guid.NewGuid(), Name = "tag" };
        var asset = new AssetEntity { Id = Guid.NewGuid(), InternalName = "asset", Tags = new List<TagEntity> { tag } };

        context.Tags.Add(tag);
        context.Assets.Add(asset);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act
        await tagsLocalEf.RemoveTagFromResource(tag.Id, asset.Id);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);
        var updatedAsset = await assertContext.Assets
            .Include(a => a.Tags)
            .FirstAsync(a => a.Id == asset.Id, TestContext.Current.CancellationToken);
        Assert.Empty(updatedAsset.Tags);
    }

    [Fact]
    public async Task RemoveTagFromResource_TagOnTranslation_RemovesTag()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tag = new TagEntity { Id = Guid.NewGuid(), Name = "tag" };
        var translation = new TranslationEntity
            { Id = Guid.NewGuid(), Language = "en", Tags = new List<TagEntity> { tag } };

        context.Tags.Add(tag);
        context.Translations.Add(translation);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act
        await tagsLocalEf.RemoveTagFromResource(tag.Id, translation.Id);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);
        var updatedTranslation =
            await assertContext.Translations
                .Include(t => t.Tags)
                .FirstAsync(t => t.Id == translation.Id, TestContext.Current.CancellationToken);

        Assert.Empty(updatedTranslation.Tags);
    }

    [Fact]
    public async Task RemoveTagFromResource_TagNotOnResource_NoChange()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var tag = new TagEntity { Id = Guid.NewGuid(), Name = "tag" };
        var asset = new AssetEntity { Id = Guid.NewGuid(), InternalName = "asset" };

        context.Tags.Add(tag);
        context.Assets.Add(asset);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var tagsLocalEf = new TagsLocalEF(serviceProvider);

        // Act
        await tagsLocalEf.RemoveTagFromResource(tag.Id, asset.Id);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);
        var updatedAsset = await assertContext.Assets
            .Include(a => a.Tags)
            .FirstAsync(a => a.Id == asset.Id, TestContext.Current.CancellationToken);

        Assert.Empty(updatedAsset.Tags);
    }
}