using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Lame.Backend.EntityFramework;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Default location for design time:
        var documents = Environment.GetFolderPath(
            Environment.SpecialFolder.MyDocuments);

        var path = Path.Combine(documents, "LAME", "local.db");


        optionsBuilder.UseSqlite($"Data Source={path}");

        return new AppDbContext(optionsBuilder.Options);
    }
}