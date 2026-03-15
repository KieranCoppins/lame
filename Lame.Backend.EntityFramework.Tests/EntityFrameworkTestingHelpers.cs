using Microsoft.EntityFrameworkCore;

namespace Lame.Backend.EntityFramework.Tests;

public static class EntityFrameworkTestingHelpers
{
    public static AppDbContext CreateMemoryDatabase(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new AppDbContext(options);
    }
}