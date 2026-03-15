using Lame.Backend.EntityFramework;
using Lame.Backend.EntityFramework.Models;
using Lame.Backend.EntityFramework.Tests;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Backend.Languages.LocalEF.Tests;

public class LanguagesLocalEFTests
{
    [Fact]
    public async Task Get_WhenLanguagesExist_ReturnsAllLanguages()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

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

        var languagesLocalEf = new LanguagesLocalEF(serviceProvider);

        // Act
        var result = await languagesLocalEf.Get();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, l => l.LanguageCode == "en");
        Assert.Contains(result, l => l.LanguageCode == "fr");
    }

    [Fact]
    public async Task Get_WhenNoLanguagesExist_ReturnsEmptyList()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var context = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var languagesLocalEf = new LanguagesLocalEF(serviceProvider);

        // Act
        var result = await languagesLocalEf.Get();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task RegisterLanguage_ValidLanguage_AddsLanguageToDatabase()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var language = new Language { LanguageCode = "es" };

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var languagesLocalEf = new LanguagesLocalEF(serviceProvider);

        // Act
        await languagesLocalEf.RegisterLanguage(language);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);
        var result = await assertContext.Languages
            .FirstOrDefaultAsync(l => l.LanguageCode == "es", TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("es", result.LanguageCode);
    }

    [Fact]
    public async Task RegisterLanguage_EmptyLanguageCode_DoesNotAddLanguage()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var language = new Language { LanguageCode = "" };

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var languagesLocalEf = new LanguagesLocalEF(serviceProvider);

        // Act
        await languagesLocalEf.RegisterLanguage(language);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);
        var count = await assertContext.Languages.CountAsync(TestContext.Current.CancellationToken);
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task RegisterLanguage_WhitespaceLanguageCode_DoesNotAddLanguage()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var language = new Language { LanguageCode = "   " };

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var languagesLocalEf = new LanguagesLocalEF(serviceProvider);

        // Act
        await languagesLocalEf.RegisterLanguage(language);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);
        var count = await assertContext.Languages.CountAsync(TestContext.Current.CancellationToken);
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task RegisterLanguage_LanguageCodeAlreadyExists_DoesNotAddDuplicate()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var language = new Language { LanguageCode = "it" };

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();

        var languagesLocalEf = new LanguagesLocalEF(serviceProvider);

        // Act
        await languagesLocalEf.RegisterLanguage(language);
        await languagesLocalEf.RegisterLanguage(language);

        // Assert
        await using var assertContext = EntityFrameworkTestingHelpers.CreateMemoryDatabase(dbName);
        var count = await assertContext.Languages.CountAsync(l => l.LanguageCode == "it",
            TestContext.Current.CancellationToken);
        Assert.Equal(1, count);
    }
}