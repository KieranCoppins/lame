using Lame.Backend.EntityFramework;
using Lame.Backend.EntityFramework.Models;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Backend.Translations.LocalEF;

public class TranslationsLocalEF : ITranslations
{
    private readonly IServiceProvider _serviceProvider;

    public TranslationsLocalEF(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<List<Translation>> Get()
    {
        return await Task.Run(() =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            return context.Translations
                .Select(entity => (Translation)entity)
                .ToListAsync();
        });
    }

    public async Task<Translation?> Get(Guid id)
    {
        return await Task.Run(() =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            return context.Translations
                .Where(entity => entity.Id == id)
                .Select(entity => (Translation)entity)
                .FirstOrDefaultAsync();
        });
    }

    public async Task<List<Translation>> GetForAsset(Guid assetId)
    {
        return await Task.Run(() =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            return context.Translations
                .Where(entity => entity.AssetId == assetId)
                .Select(entity => (Translation)entity)
                .ToListAsync();
        });
    }

    public Task Create(Translation translation)
    {
        return Task.Run(() =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            translation.CreatedAt = DateTime.UtcNow;
            context.Translations.Add(MapToEntity(translation));
            return context.SaveChangesAsync();
        });
    }

    public Task Update(Translation translation)
    {
        return Task.Run(() =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Translations.Update(MapToEntity(translation));
            return context.SaveChangesAsync();
        });
    }

    private static TranslationEntity MapToEntity(Translation translation)
    {
        return new TranslationEntity
        {
            Id = translation.Id,
            AssetId = translation.AssetId,
            CreatedAt = translation.CreatedAt,
            Language = translation.Language,
            Content = translation.Content,
            MajorVersion = translation.MajorVersion,
            MinorVersion = translation.MinorVersion
        };
    }
}