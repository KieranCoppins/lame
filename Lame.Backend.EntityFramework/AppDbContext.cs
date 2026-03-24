using Lame.Backend.EntityFramework.Models;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;

namespace Lame.Backend.EntityFramework;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<AssetEntity> Assets { get; set; }
    public DbSet<TranslationEntity> Translations { get; set; }
    public DbSet<TagEntity> Tags { get; set; }
    public DbSet<LanguageEntity> Languages { get; set; }
    public DbSet<TargetAssetTranslationEntity> TargetAssetTranslations { get; set; }
    public DbSet<AssetLinkEntity> AssetLinks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Assets Table
        modelBuilder.Entity<AssetEntity>()
            .HasKey(sc => sc.Id);

        modelBuilder.Entity<AssetEntity>()
            .Property(s => s.Status)
            .HasDefaultValue(AssetStatus.Active);

        // Asset Links Table
        modelBuilder.Entity<AssetLinkEntity>()
            .ToTable("AssetLinks")
            .HasKey(x => new { x.AssetEntityId, x.LinkedContentId });

        modelBuilder.Entity<AssetLinkEntity>()
            .HasOne(x => x.AssetEntity)
            .WithMany(x => x.LinkedTo)
            .HasForeignKey(x => x.AssetEntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AssetLinkEntity>()
            .HasOne(x => x.LinkedAssetEntity)
            .WithMany(x => x.LinkedFrom)
            .HasForeignKey(x => x.LinkedContentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Translations Table
        modelBuilder.Entity<TranslationEntity>()
            .HasKey(t => t.Id);

        modelBuilder.Entity<TranslationEntity>()
            .HasOne(t => t.Asset)
            .WithMany(sc => sc.Translations)
            .HasForeignKey(t => t.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Tags Table
        modelBuilder.Entity<TagEntity>()
            .HasKey(t => t.Id);

        // Create many-to-many relationship between Assets and Tags
        modelBuilder.Entity<AssetEntity>()
            .HasMany(s => s.Tags)
            .WithMany(t => t.Assets)
            .UsingEntity(j => j.ToTable("AssetTags"));

        // Create many-to-many relationship between Translations and Tags
        modelBuilder.Entity<TranslationEntity>()
            .HasMany(s => s.Tags)
            .WithMany(t => t.Translations)
            .UsingEntity(j => j.ToTable("TranslationTags"));

        // Languages Table
        modelBuilder.Entity<LanguageEntity>()
            .HasKey(l => l.LanguageCode);

        // Target Asset Translations table - used to store which translation version we are targeting for an asset's language
        modelBuilder.Entity<TargetAssetTranslationEntity>()
            .HasKey(t => new { t.AssetId, t.Language });

        modelBuilder.Entity<TargetAssetTranslationEntity>()
            .HasOne(t => t.Asset)
            .WithMany(a => a.TargetedTranslations)
            .HasForeignKey(t => t.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TargetAssetTranslationEntity>()
            .HasOne(t => t.Translation)
            .WithMany()
            .HasForeignKey(t => t.TranslationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}