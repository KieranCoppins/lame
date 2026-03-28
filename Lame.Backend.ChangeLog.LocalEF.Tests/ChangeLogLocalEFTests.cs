using Lame.Backend.EntityFramework;
using Lame.Backend.EntityFramework.Models;
using Lame.Backend.EntityFramework.Tests;
using Lame.Backend.EntityFramework.Tests.EntityBuilders;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Backend.ChangeLog.LocalEF.Tests;

public class ChangeLogLocalEFTests
{
    [Fact]
    public async Task Get_WithMultiplePages_ReturnsCorrectPageItems()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var entries = Enumerable.Range(0, 15)
            .Select(i => new ChangeLogEntity
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow.AddMinutes(-i),
                Message = $"Entry {i}",
                ResourceAction = ResourceAction.Created,
                ResourceId = Guid.NewGuid(),
                ResourceType = ResourceType.Asset
            }).ToList();

        context.ChangeLogEntries.AddRange(entries);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var changeLogLocalEf = new ChangeLogLocalEF(serviceProvider);

        // Act
        var result = await changeLogLocalEf.Get(1, 5);

        // Assert
        Assert.Equal(1, result.Page);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(5, result.Items.Count);
        Assert.Equal(entries[5].Message, result.Items[0].Message);
    }

    [Fact]
    public async Task Get_PageOutOfRange_ReturnsEmptyItems()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var entries = Enumerable.Range(0, 3)
            .Select(i => new ChangeLogEntity
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow.AddMinutes(i),
                Message = $"Entry {i}",
                ResourceAction = ResourceAction.Created,
                ResourceId = Guid.NewGuid(),
                ResourceType = ResourceType.Asset
            }).ToList();

        context.ChangeLogEntries.AddRange(entries);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var changeLogLocalEf = new ChangeLogLocalEF(serviceProvider);

        // Act
        var result = await changeLogLocalEf.Get(2, 2);

        // Assert
        Assert.Equal(2, result.Page);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Get_NoEntries_ReturnsEmptyItemsAndZeroTotalPages()
    {
        // Arrage
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var changeLogLocalEf = new ChangeLogLocalEF(serviceProvider);

        // Act
        var result = await changeLogLocalEf.Get(0, 10);

        // Assert
        Assert.Equal(0, result.Page);
        Assert.Equal(0, result.TotalPages);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Get_PageSizeZero_ReturnsEmptyItemsAndZeroTotalPages()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var entries = Enumerable.Range(0, 5)
            .Select(i => new ChangeLogEntity
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow.AddMinutes(i),
                Message = $"Entry {i}",
                ResourceAction = ResourceAction.Created,
                ResourceId = Guid.NewGuid(),
                ResourceType = ResourceType.Asset
            }).ToList();

        context.ChangeLogEntries.AddRange(entries);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var changeLogLocalEf = new ChangeLogLocalEF(serviceProvider);

        // Act
        var result = await changeLogLocalEf.Get(0, 0);

        // Assert
        Assert.Equal(0, result.Page);
        Assert.Equal(0, result.TotalPages);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Create_ValidChangeLogEntry_PersistsEntity()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var changeLogEntry = new ChangeLogEntry
        {
            Message = "Created asset",
            ResourceAction = ResourceAction.Created,
            ResourceId = Guid.NewGuid(),
            ResourceType = ResourceType.Asset
        };

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var changeLogLocalEf = new ChangeLogLocalEF(serviceProvider);

        // Act
        await changeLogLocalEf.Create(changeLogEntry);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);
        var entity = await assertContext.ChangeLogEntries.FirstOrDefaultAsync();

        Assert.NotNull(entity);
        Assert.Equal(changeLogEntry.Message, entity.Message);
        Assert.Equal(changeLogEntry.ResourceAction, entity.ResourceAction);
        Assert.Equal(changeLogEntry.ResourceId, entity.ResourceId);
        Assert.Equal(changeLogEntry.ResourceType, entity.ResourceType);
        Assert.True((DateTime.UtcNow - entity.Date).TotalSeconds < 10);
    }

    [Fact]
    public async Task Get_WithResourceIds_ReturnsChangesWithResourceIds()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var ResourceAId = Guid.NewGuid();
        var ResourceBId = Guid.NewGuid();
        var ResourceCId = Guid.NewGuid();

        var entries = new List<ChangeLogEntity>
        {
            new ChangeLogEntityBuilder().WithResourceId(ResourceAId).Build(),
            new ChangeLogEntityBuilder().WithResourceId(ResourceAId).Build(),
            new ChangeLogEntityBuilder().WithResourceId(ResourceAId).Build(),
            new ChangeLogEntityBuilder().WithResourceId(ResourceBId).Build(),
            new ChangeLogEntityBuilder().WithResourceId(ResourceBId).Build(),
            new ChangeLogEntityBuilder().WithResourceId(ResourceCId).Build(),
            new ChangeLogEntityBuilder().WithResourceId(ResourceCId).Build()
        };

        context.ChangeLogEntries.AddRange(entries);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var changeLogLocalEf = new ChangeLogLocalEF(serviceProvider);

        // Act
        var result = await changeLogLocalEf.Get(0, 25, new List<Guid> { ResourceAId, ResourceCId });

        // Assert
        Assert.Equal(5, result.Items.Count);
    }
}