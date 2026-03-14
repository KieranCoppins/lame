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

            var records = await context.Assets
                .Where(a =>
                    exportOptions.TranslationStatusFilter == ExportTranslationStatusFilter.All ||
                    (exportOptions.TranslationStatusFilter == ExportTranslationStatusFilter.Complete &&
                     a.Translations.Any(t =>
                         t.Language == exportOptions.LanguageCode && !string.IsNullOrEmpty(t.Content))) ||
                    (exportOptions.TranslationStatusFilter == ExportTranslationStatusFilter.Missing &&
                     !a.Translations.Any(t =>
                         t.Language == exportOptions.LanguageCode && !string.IsNullOrEmpty(t.Content)))
                )
                .Select(a => new AssetExportData
                {
                    // Asset Data
                    Id = a.Id,
                    InternalName = a.InternalName,
                    Context = a.ContextNotes ?? string.Empty,

                    // Get latest source translation (always english) and create translation export data
                    SourceTranslation = a.Translations
                        .Where(t => t.Language == "en")
                        .OrderByDescending(t => t.MajorVersion)
                        .ThenByDescending(t => t.MinorVersion)
                        .Select(t =>
                            new TranslationExportData
                            {
                                Id = t.AssetId,
                                Content = t.Content ?? string.Empty
                            }).FirstOrDefault(),


                    // Get latest target translation (in export options) and create translation export data
                    TargetTranslation = a.Translations
                        .Where(t => t.Language == exportOptions.LanguageCode)
                        .OrderByDescending(t => t.MajorVersion)
                        .ThenByDescending(t => t.MinorVersion)
                        .Select(t =>
                            new TranslationExportData
                            {
                                Id = t.AssetId,
                                Content = t.Content ?? string.Empty
                            }).FirstOrDefault()
                }).ToListAsync();

            return exportOptions.Format switch
            {
                ExportFormatType.JSON => ExportHelpers.ExportToJson(records),
                ExportFormatType.XLIFF => ExportHelpers.ExportToXliff12(records, "en", exportOptions.LanguageCode),
                _ => []
            };
        });
    }
}