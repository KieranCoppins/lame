using Lame.Backend.EntityFramework;
using Lame.Backend.EntityFramework.Models;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Backend.Languages.LocalEF;

public class LanguagesLocalEF : ILanguages
{
    private readonly IServiceProvider _serviceProvider;

    public LanguagesLocalEF(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<List<Language>> Get()
    {
        return await Task.Run(() =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            return context.Languages
                .Select(entity => (Language)entity)
                .ToListAsync();
        });
    }

    public Task RegisterLanguage(Language language)
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (string.IsNullOrWhiteSpace(language?.LanguageCode)) return;

            var existingLanguage = await context.Languages
                .FirstOrDefaultAsync(l => l.LanguageCode == language.LanguageCode);

            if (existingLanguage != null) return;

            context.Languages.Add(MapToEntity(language));
            await context.SaveChangesAsync();
        });
    }

    private LanguageEntity MapToEntity(Language language)
    {
        return new LanguageEntity
        {
            LanguageCode = language.LanguageCode
        };
    }
}