using Lame.Backend.EntityFramework;
using Lame.Backend.EntityFramework.Models;
using Lame.Backend.EntityFramework.Tests;
using Lame.Backend.EntityFramework.Tests.EntityBuilders;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Backend.AssetLinks.LocalEF.Tests;

public class AssetLinksLocalEFTests
{
    [Fact]
    public async Task LinkAssets_BothAssetsExist_LinksAssets()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new AssetEntityBuilder().WithStatus(AssetStatus.Active).Build(),
            new AssetEntityBuilder().WithStatus(AssetStatus.Active).Build()
        };

        context.Assets.AddRange(assets);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetLinksLocalEf = new AssetLinksLocalEF(serviceProvider);

        // Act
        await assetLinksLocalEf.Create(assets[0].Id, assets[1].Id);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var resultA = await assertContext.AssetLinks
            .FirstOrDefaultAsync(a => a.AssetEntityId == assets[0].Id && a.LinkedContentId == assets[1].Id,
                TestContext.Current.CancellationToken);

        var resultB = await assertContext.AssetLinks
            .FirstOrDefaultAsync(a => a.AssetEntityId == assets[1].Id && a.LinkedContentId == assets[0].Id,
                TestContext.Current.CancellationToken);

        Assert.NotNull(resultA);
        Assert.NotNull(resultB);
    }

    [Fact]
    public async Task LinkAssets_AssetAExistsOnly_NoLinksCreated()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new AssetEntityBuilder().WithStatus(AssetStatus.Active).Build(),
            new AssetEntityBuilder().WithStatus(AssetStatus.Active).Build()
        };

        context.Assets.AddRange(assets);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetLinksLocalEf = new AssetLinksLocalEF(serviceProvider);
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            assetLinksLocalEf.Create(assets[0].Id, nonExistentId));

        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var resultA = await assertContext.AssetLinks
            .FirstOrDefaultAsync(a => a.AssetEntityId == assets[0].Id && a.LinkedContentId == nonExistentId,
                TestContext.Current.CancellationToken);

        var resultB = await assertContext.AssetLinks
            .FirstOrDefaultAsync(a => a.AssetEntityId == nonExistentId && a.LinkedContentId == assets[0].Id,
                TestContext.Current.CancellationToken);

        Assert.Null(resultA);
        Assert.Null(resultB);
    }

    [Fact]
    public async Task LinkAssets_AssetBExistsOnly_NoLinksCreated()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new AssetEntityBuilder().WithStatus(AssetStatus.Active).Build(),
            new AssetEntityBuilder().WithStatus(AssetStatus.Active).Build()
        };

        context.Assets.AddRange(assets);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetLinksLocalEf = new AssetLinksLocalEF(serviceProvider);

        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            assetLinksLocalEf.Create(nonExistentId, assets[1].Id));

        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var resultA = await assertContext.AssetLinks
            .FirstOrDefaultAsync(a => a.AssetEntityId == nonExistentId && a.LinkedContentId == assets[1].Id,
                TestContext.Current.CancellationToken);

        var resultB = await assertContext.AssetLinks
            .FirstOrDefaultAsync(a => a.AssetEntityId == assets[1].Id && a.LinkedContentId == nonExistentId,
                TestContext.Current.CancellationToken);

        Assert.Null(resultA);
        Assert.Null(resultB);
    }

    [Fact]
    public async Task UnLinkAssets_BothAssetsExist_UnLinksAssets()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new AssetEntityBuilder().WithStatus(AssetStatus.Active).Build(),
            new AssetEntityBuilder().WithStatus(AssetStatus.Active).Build()
        };

        var link01 = new AssetLinkEntityBuilder(assets[0], assets[1]).Build();
        var link10 = new AssetLinkEntityBuilder(assets[1], assets[0]).Build();

        context.Assets.AddRange(assets);
        context.AssetLinks.AddRange(link01, link10);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetLinksLocalEf = new AssetLinksLocalEF(serviceProvider);

        // Act
        await assetLinksLocalEf.Delete(assets[0].Id, assets[1].Id);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var resultA = await assertContext.AssetLinks
            .FirstOrDefaultAsync(a => a.AssetEntityId == assets[0].Id && a.LinkedContentId == assets[1].Id,
                TestContext.Current.CancellationToken);

        var resultB = await assertContext.AssetLinks
            .FirstOrDefaultAsync(a => a.AssetEntityId == assets[1].Id && a.LinkedContentId == assets[0].Id,
                TestContext.Current.CancellationToken);

        Assert.Null(resultA);
        Assert.Null(resultB);
    }

    [Fact]
    public async Task UnLinkAssets_AssetAExistsOnly_LinksUnchanged()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new AssetEntityBuilder().WithStatus(AssetStatus.Active).Build(),
            new AssetEntityBuilder().WithStatus(AssetStatus.Active).Build()
        };

        var link01 = new AssetLinkEntityBuilder(assets[0], assets[1]).Build();
        var link10 = new AssetLinkEntityBuilder(assets[1], assets[0]).Build();

        context.Assets.AddRange(assets);
        context.AssetLinks.AddRange(link01, link10);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetLinksLocalEf = new AssetLinksLocalEF(serviceProvider);

        // Act
        await assetLinksLocalEf.Delete(assets[0].Id, Guid.NewGuid());

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var resultA = await assertContext.AssetLinks
            .FirstOrDefaultAsync(a => a.AssetEntityId == assets[0].Id && a.LinkedContentId == assets[1].Id,
                TestContext.Current.CancellationToken);

        var resultB = await assertContext.AssetLinks
            .FirstOrDefaultAsync(a => a.AssetEntityId == assets[1].Id && a.LinkedContentId == assets[0].Id,
                TestContext.Current.CancellationToken);

        Assert.NotNull(resultA);
        Assert.NotNull(resultB);
    }

    [Fact]
    public async Task UnLinkAssets_AssetBExistsOnly_LinksUnchanged()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assets = new List<AssetEntity>
        {
            new AssetEntityBuilder().WithStatus(AssetStatus.Active).Build(),
            new AssetEntityBuilder().WithStatus(AssetStatus.Active).Build()
        };

        var link01 = new AssetLinkEntityBuilder(assets[0], assets[1]).Build();
        var link10 = new AssetLinkEntityBuilder(assets[1], assets[0]).Build();

        context.Assets.AddRange(assets);
        context.AssetLinks.AddRange(link01, link10);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetLinksLocalEf = new AssetLinksLocalEF(serviceProvider);

        // Act
        await assetLinksLocalEf.Delete(Guid.NewGuid(), assets[1].Id);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var resultA = await assertContext.AssetLinks
            .FirstOrDefaultAsync(a => a.AssetEntityId == assets[0].Id && a.LinkedContentId == assets[1].Id,
                TestContext.Current.CancellationToken);

        var resultB = await assertContext.AssetLinks
            .FirstOrDefaultAsync(a => a.AssetEntityId == assets[1].Id && a.LinkedContentId == assets[0].Id,
                TestContext.Current.CancellationToken);

        Assert.NotNull(resultA);
        Assert.NotNull(resultB);
    }

    [Fact]
    public async Task SyncAssetLink_BothLinksExist_UpdatesSyncedStatus()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assetA = new AssetEntityBuilder().WithStatus(AssetStatus.Active).Build();
        var assetB = new AssetEntityBuilder().WithStatus(AssetStatus.Active).Build();

        context.Assets.AddRange(assetA, assetB);

        var linkA = new AssetLinkEntityBuilder(assetA, assetB).WithSynced(false).Build();
        var linkB = new AssetLinkEntityBuilder(assetB, assetA).WithSynced(false).Build();

        context.AssetLinks.AddRange(linkA, linkB);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetLinksLocalEf = new AssetLinksLocalEF(serviceProvider);

        // Act
        await assetLinksLocalEf.SyncAssetLink(assetA.Id, assetB.Id);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var updatedLinkA = await assertContext.AssetLinks
            .FirstAsync(a => a.AssetEntityId == assetA.Id && a.LinkedContentId == assetB.Id,
                TestContext.Current.CancellationToken);

        var updatedLinkB = await assertContext.AssetLinks
            .FirstAsync(a => a.AssetEntityId == assetB.Id && a.LinkedContentId == assetA.Id,
                TestContext.Current.CancellationToken);

        Assert.True(updatedLinkA.Synced);
        Assert.True(updatedLinkB.Synced);
    }

    [Fact]
    public async Task SyncAssetLink_LinksDoNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var assetA = new AssetEntityBuilder().WithStatus(AssetStatus.Active).Build();
        var assetB = new AssetEntityBuilder().WithStatus(AssetStatus.Active).Build();

        context.Assets.AddRange(assetA, assetB);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var assetLinksLocalEf = new AssetLinksLocalEF(serviceProvider);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            assetLinksLocalEf.SyncAssetLink(assetA.Id, assetB.Id));
    }
}