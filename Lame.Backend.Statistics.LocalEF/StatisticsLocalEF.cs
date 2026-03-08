using Lame.Backend.EntityFramework;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Backend.Statistics.LocalEF;

public class StatisticsLocalEF : IStatistics
{
    private readonly IServiceProvider _serviceProvider;

    public StatisticsLocalEF(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<ProjectStatistics> GetProjectStatistics()
    {
        return await Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var statistics = new ProjectStatistics();

            // Total assets
            statistics.TotalAssets = await context.Assets
                .AsNoTracking()
                .CountAsync();

            // Total languages
            statistics.TotalLanguages = await context.Languages
                .AsNoTracking()
                .CountAsync();

            // Translations by language
            statistics.TranslationsByLanguage = await context.Languages
                .AsNoTracking()
                .GroupJoin(
                    context.Translations.AsNoTracking(),
                    l => l.LanguageCode,
                    t => t.Language,
                    (l, translations) => new
                    {
                        Language = l.LanguageCode,
                        Count = translations.Count()
                    })
                .ToDictionaryAsync(x => x.Language, x => x.Count);

            // Missing translations
            var totalAssets = statistics.TotalAssets;
            var totalLanguages = statistics.TotalLanguages;
            var totalTranslations = await context.Translations
                .AsNoTracking()
                .CountAsync();

            statistics.MissingTranslations = totalAssets * totalLanguages - totalTranslations;

            // Assets by type
            statistics.AssetsByType = await context.Assets
                .AsNoTracking()
                .GroupBy(a => a.AssetType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);

            // Assets by tag
            statistics.AssetsByTag = await context.Tags
                .AsNoTracking()
                .Select(t => new
                {
                    Tag = (Tag)t,
                    Count = t.Assets.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToDictionaryAsync(x => x.Tag, x => x.Count);

            return statistics;
        });
    }
}