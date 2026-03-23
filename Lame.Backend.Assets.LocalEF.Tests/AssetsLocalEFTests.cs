using Lame.Backend.EntityFramework;
using Lame.Backend.EntityFramework.Models;
using Lame.Backend.EntityFramework.Tests;
using Lame.Backend.EntityFramework.Tests.EntityBuilders;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Backend.Assets.LocalEF.Tests;

public class AssetsLocalEFTests
{
    [Fact]
    public async Task Get_WithDeletedAssets_ReturnsAllNonDeletedAssets()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new() { Id = Guid.NewGuid(), InternalName = "asset_a", Status = AssetStatus.Active },
            new() { Id = Guid.NewGuid(), InternalName = "asset_b", Status = AssetStatus.Deleted }
        };

        context.Assets.AddRange(assets);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetsLocalEf = new AssetsLocalEf(serviceProvider);

        // Act
        var result = await assetsLocalEf.Get();

        // Assert
        Assert.Single(result);
        Assert.Equal(assets[0].Id, result[0].Id);
    }

    [Fact]
    public async Task Get_SearchWithDeletedAssets_ReturnsNonDeletedAssets()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new() { Id = Guid.NewGuid(), InternalName = "ui_asset_a", Status = AssetStatus.Active },
            new() { Id = Guid.NewGuid(), InternalName = "ui_asset_b", Status = AssetStatus.Deleted },
            new() { Id = Guid.NewGuid(), InternalName = "npc_asset_a", Status = AssetStatus.Active },
            new() { Id = Guid.NewGuid(), InternalName = "npc_asset_b", Status = AssetStatus.Active }
        };

        context.Assets.AddRange(assets);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetsLocalEf = new AssetsLocalEf(serviceProvider);

        // Act
        var result = await assetsLocalEf.Get("ui", 3);

        // Assert
        Assert.Single(result);
        Assert.Equal(assets[0].Id, result[0].Id);
    }


    [Fact]
    public async Task Get_SearchWithLimit_ReturnsLimited()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new() { Id = Guid.NewGuid(), InternalName = "ui_asset_a", Status = AssetStatus.Active },
            new() { Id = Guid.NewGuid(), InternalName = "ui_asset_b", Status = AssetStatus.Deleted },
            new() { Id = Guid.NewGuid(), InternalName = "npc_asset_a", Status = AssetStatus.Active },
            new() { Id = Guid.NewGuid(), InternalName = "npc_asset_b", Status = AssetStatus.Active }
        };

        context.Assets.AddRange(assets);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetsLocalEf = new AssetsLocalEf(serviceProvider);

        // Act
        var result = await assetsLocalEf.Get("npc", 1);

        // Assert
        Assert.Single(result);
        Assert.Equal(assets[2].Id, result[0].Id);
    }

    [Fact]
    public async Task Get_SearchWithNoDeletedAssets_ReturnsAllAssets()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new() { Id = Guid.NewGuid(), InternalName = "ui_asset_a", Status = AssetStatus.Active },
            new() { Id = Guid.NewGuid(), InternalName = "ui_asset_b", Status = AssetStatus.Deleted },
            new() { Id = Guid.NewGuid(), InternalName = "npc_asset_a", Status = AssetStatus.Active },
            new() { Id = Guid.NewGuid(), InternalName = "npc_asset_b", Status = AssetStatus.Active }
        };

        context.Assets.AddRange(assets);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetsLocalEf = new AssetsLocalEf(serviceProvider);

        // Act - limit is greater than we need so it shouldn't matter
        var result = await assetsLocalEf.Get("npc", 3);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(assets[2].Id, result[0].Id);
        Assert.Equal(assets[3].Id, result[1].Id);
    }

    [Fact]
    public async Task Get_SearchContaining_ReturnsNonDeletedAssetsAlphabetical()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new() { Id = Guid.NewGuid(), InternalName = "ui_asset_a", Status = AssetStatus.Active },
            new() { Id = Guid.NewGuid(), InternalName = "ui_asset_b", Status = AssetStatus.Deleted },
            new() { Id = Guid.NewGuid(), InternalName = "npc_asset_a", Status = AssetStatus.Active },
            new() { Id = Guid.NewGuid(), InternalName = "npc_asset_b", Status = AssetStatus.Active }
        };

        context.Assets.AddRange(assets);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetsLocalEf = new AssetsLocalEf(serviceProvider);

        // Act - limit is greater than we need so it shouldn't matter
        var result = await assetsLocalEf.Get("asset", 4);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(assets[2].Id, result[0].Id);
        Assert.Equal(assets[3].Id, result[1].Id);
        Assert.Equal(assets[0].Id, result[2].Id);
    }

    [Fact]
    public async Task Get_SearchTag_ReturnsNonDeletedAssetsWithTag()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assetTags = new List<TagEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "tag1" },
            new() { Id = Guid.NewGuid(), Name = "tag2" }
        };

        var assets = new List<AssetEntity>
        {
            new() { Id = Guid.NewGuid(), InternalName = "ui_asset_a", Status = AssetStatus.Active, Tags = assetTags },
            new() { Id = Guid.NewGuid(), InternalName = "ui_asset_b", Status = AssetStatus.Deleted, Tags = assetTags },
            new() { Id = Guid.NewGuid(), InternalName = "npc_asset_a", Status = AssetStatus.Active },
            new() { Id = Guid.NewGuid(), InternalName = "npc_asset_b", Status = AssetStatus.Active, Tags = assetTags }
        };

        context.Assets.AddRange(assets);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetsLocalEf = new AssetsLocalEf(serviceProvider);

        // Act - limit is greater than we need so it shouldn't matter
        var result = await assetsLocalEf.Get("tag1", 4);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(assets[3].Id, result[0].Id);
        Assert.Equal(assets[0].Id, result[1].Id);
    }

    [Fact]
    public async Task Get_ById_ReturnsNonDeletedAsset()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new() { Id = Guid.NewGuid(), InternalName = "asset_a", Status = AssetStatus.Active },
            new() { Id = Guid.NewGuid(), InternalName = "asset_b", Status = AssetStatus.Deleted }
        };

        context.Assets.AddRange(assets);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetsLocalEf = new AssetsLocalEf(serviceProvider);

        // Act
        var result = await assetsLocalEf.Get(assets[0].Id);

        // Assert
        Assert.Equal(assets[0].Id, result?.Id);
    }

    [Fact]
    public async Task Get_ById_ReturnsDeletedAsset()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new() { Id = Guid.NewGuid(), InternalName = "asset_a", Status = AssetStatus.Active },
            new() { Id = Guid.NewGuid(), InternalName = "asset_b", Status = AssetStatus.Deleted }
        };

        context.Assets.AddRange(assets);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetsLocalEf = new AssetsLocalEf(serviceProvider);

        // Act
        var result = await assetsLocalEf.Get(assets[1].Id);

        // Assert
        Assert.Equal(assets[1].Id, result?.Id);
    }

    [Fact]
    public async Task GetLinkedAssets_HasActiveLinkedAssets_ReturnsAll()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new() { Id = Guid.NewGuid(), InternalName = "asset_a", Status = AssetStatus.Active },
            new() { Id = Guid.NewGuid(), InternalName = "asset_b", Status = AssetStatus.Active },
            new() { Id = Guid.NewGuid(), InternalName = "asset_c", Status = AssetStatus.Active }
        };

        var link01 = new AssetLinkEntityBuilder(assets[0], assets[1]).Build();
        var link02 = new AssetLinkEntityBuilder(assets[0], assets[2]).Build();
        var link10 = new AssetLinkEntityBuilder(assets[1], assets[0]).Build();
        var link20 = new AssetLinkEntityBuilder(assets[2], assets[0]).Build();

        context.Assets.AddRange(assets);
        context.AssetLinks.AddRange(link01, link02, link10, link20);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetsLocalEf = new AssetsLocalEf(serviceProvider);

        // Act
        var result = await assetsLocalEf.GetLinkedAssets(assets[0].Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(assets[1].Id, result[0].Id);
        Assert.Equal(assets[2].Id, result[1].Id);
    }

    [Fact]
    public async Task GetLinkedAssets_HasDeletedLinkedAssets_ReturnsNonDeleted()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new() { Id = Guid.NewGuid(), InternalName = "asset_a", Status = AssetStatus.Active },
            new() { Id = Guid.NewGuid(), InternalName = "asset_b", Status = AssetStatus.Active },
            new() { Id = Guid.NewGuid(), InternalName = "asset_c", Status = AssetStatus.Deleted }
        };

        var link01 = new AssetLinkEntityBuilder(assets[0], assets[1]).Build();
        var link02 = new AssetLinkEntityBuilder(assets[0], assets[2]).Build();
        var link10 = new AssetLinkEntityBuilder(assets[1], assets[0]).Build();
        var link20 = new AssetLinkEntityBuilder(assets[2], assets[0]).Build();

        context.Assets.AddRange(assets);
        context.AssetLinks.AddRange(link01, link02, link10, link20);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetsLocalEf = new AssetsLocalEf(serviceProvider);

        // Act
        var result = await assetsLocalEf.GetLinkedAssets(assets[0].Id);

        // Assert
        Assert.Single(result);
        Assert.Equal(assets[1].Id, result[0].Id);
    }

    [Fact]
    public async Task Create_ValidAsset_CreatesAssetEntity()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();

        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            InternalName = "asset_a"
        };

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetsLocalEf = new AssetsLocalEf(serviceProvider);

        // Act
        await assetsLocalEf.Create(asset);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var result = await assertContext.Assets
            .FirstAsync(a => a.Id == asset.Id, TestContext.Current.CancellationToken);

        Assert.Equal(asset.Id, result.Id);
        Assert.Equal(asset.InternalName, result.InternalName);
        Assert.Equal(AssetStatus.Active, result.Status);
        Assert.NotEqual(default, result.CreatedAt);
        Assert.NotEqual(default, result.LastUpdatedAt);
    }

    [Fact]
    public async Task Update_NewProperties_UpdatesProperties()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new() { Id = Guid.NewGuid(), InternalName = "asset_a", Status = AssetStatus.Active }
        };

        var assetToUpdate = new Asset
        {
            Id = assets[0].Id,
            InternalName = "updated_asset_a"
        };

        context.Assets.AddRange(assets);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetsLocalEf = new AssetsLocalEf(serviceProvider);

        // Act
        await assetsLocalEf.Update(assetToUpdate);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var resultA = await assertContext.Assets
            .FirstAsync(a => a.Id == assets[0].Id, TestContext.Current.CancellationToken);

        Assert.Equal(resultA.InternalName, assetToUpdate.InternalName);
        Assert.NotEqual(default, resultA.LastUpdatedAt);
    }

    [Fact]
    public async Task Delete_ExistingAsset_SetsStatusToDeleted()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new() { Id = Guid.NewGuid(), InternalName = "asset_a", Status = AssetStatus.Active }
        };

        context.Assets.AddRange(assets);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetsLocalEf = new AssetsLocalEf(serviceProvider);

        // Act
        await assetsLocalEf.Delete(assets[0].Id);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var resultA = await assertContext.Assets
            .FirstAsync(a => a.Id == assets[0].Id, TestContext.Current.CancellationToken);

        Assert.Equal(AssetStatus.Deleted, resultA.Status);
    }

    [Fact]
    public async Task Delete_AssetNonExistent_NoChange()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new() { Id = Guid.NewGuid(), InternalName = "asset_a", Status = AssetStatus.Active }
        };

        context.Assets.AddRange(assets);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetsLocalEf = new AssetsLocalEf(serviceProvider);

        // Act
        await assetsLocalEf.Delete(Guid.NewGuid());

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var resultA = await assertContext.Assets
            .FirstAsync(a => a.Id == assets[0].Id, TestContext.Current.CancellationToken);

        Assert.Equal(AssetStatus.Active, resultA.Status);
    }

    [Fact]
    public async Task Get_WithMultipleIds_ReturnsMatchingAssets()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new AssetEntityBuilder().WithStatus(AssetStatus.Active).Build(),
            new AssetEntityBuilder().WithStatus(AssetStatus.Active).Build(),
            new AssetEntityBuilder().WithStatus(AssetStatus.Deleted).Build()
        };

        context.Assets.AddRange(assets);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetsLocalEf = new AssetsLocalEf(serviceProvider);

        var ids = new List<Guid> { assets[0].Id, assets[2].Id };

        var result = await assetsLocalEf.Get(ids);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, a => a.Id == assets[0].Id);
        Assert.Contains(result, a => a.Id == assets[2].Id);
    }

    [Fact]
    public async Task Get_WithEmptyIdList_ReturnsEmptyList()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new() { Id = Guid.NewGuid(), InternalName = "asset_a", Status = AssetStatus.Active }
        };

        context.Assets.AddRange(assets);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetsLocalEf = new AssetsLocalEf(serviceProvider);

        var result = await assetsLocalEf.Get(new List<Guid>());

        Assert.Empty(result);
    }

    [Fact]
    public async Task Get_WithNonExistentIds_ReturnsEmptyList()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new() { Id = Guid.NewGuid(), InternalName = "asset_a", Status = AssetStatus.Active }
        };

        context.Assets.AddRange(assets);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetsLocalEf = new AssetsLocalEf(serviceProvider);

        var result = await assetsLocalEf.Get(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

        Assert.Empty(result);
    }
}