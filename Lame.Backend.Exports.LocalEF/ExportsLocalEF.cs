using Lame.Backend.EntityFramework;
using Lame.Backend.Exports.Models;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Backend.Exports.LocalEF;

public class ExportsLocalEF : IExports
{
    private readonly IServiceProvider _serviceProvider;

    public ExportsLocalEF(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<byte[]> Export(ExportOptions exportOptions)
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (exportOptions.Format == ExportFormatType.JSON)
            {
                // Get all target translations
                var records = await context.Assets
                    .Include(x => x.Translations.Where(t => t.Language == exportOptions.LanguageCode))
                    .SelectMany(asset => asset.Translations)
                    .Select(t => new TranslationRecord
                    {
                        Id = t.Asset.Id,
                        Content = t.Content ?? string.Empty
                    })
                    .ToListAsync();

                return ExportHelpers.ExportToJson(records);
            }

            if (exportOptions.Format == ExportFormatType.XLIFF)
            {
                var pairs = await context.Assets
                    .Include(x =>
                        x.Translations.Where(t => t.Language == "en" || t.Language == exportOptions.LanguageCode))
                    .Select(asset => new Tuple<AssetMetaData, TranslationRecord, TranslationRecord?>(
                        new AssetMetaData
                        {
                            InternalName = asset.InternalName,
                            Context = asset.ContextNotes ?? string.Empty
                        },
                        asset.Translations
                            .Where(t => t.Language == "en")
                            .Select(t => new TranslationRecord
                            {
                                Id = t.Asset.Id,
                                Content = t.Content ?? string.Empty
                            })
                            .First(),
                        asset.Translations
                            .Where(t => t.Language == exportOptions.LanguageCode)
                            .Select(t => new TranslationRecord
                            {
                                Id = t.Asset.Id,
                                Content = t.Content ?? string.Empty
                            })
                            .FirstOrDefault()
                    ))
                    .ToListAsync();

                return ExportHelpers.ExportToXliff12(pairs, "en", exportOptions.LanguageCode);
            }

            return [];
        });
    }
}