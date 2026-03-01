using Lame.Backend.EntityFramework.Models;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;

namespace Lame.Backend.EntityFramework;

public class AppDbContext : DbContext
{
    public DbSet<AssetEntity> Assets { get; set; }
    public DbSet<TranslationEntity> Translations { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Assets Table
        modelBuilder.Entity<AssetEntity>()
            .HasKey(sc => sc.Id);

        modelBuilder.Entity<AssetEntity>()
            .Property(s => s.Status)
            .HasDefaultValue(AssetStatus.Active);
        
        // Create many-to-many relationship with a link table
        modelBuilder.Entity<AssetEntity>()
            .HasMany(s => s.LinkedContent)
            .WithMany()
            .UsingEntity(j => j.ToTable("AssetLinks"));
        
        // Translations Table
        modelBuilder.Entity<TranslationEntity>()
            .HasKey(t => t.Id);
        
        modelBuilder.Entity<TranslationEntity>()
            .HasOne(t => t.Asset)
            .WithMany(sc => sc.Translations)
            .HasForeignKey(t => t.AssetId)
            .OnDelete(DeleteBehavior.Cascade);
        
    }

    public static string GetConnectionString()
    {
        var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var lameFolder = Path.Combine(documentsFolder, "LAME");
        Directory.CreateDirectory(lameFolder); // Ensure the folder exists
        return Path.Combine(lameFolder, "local.db");
    }
}