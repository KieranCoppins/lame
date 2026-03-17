using Lame.Backend.EntityFramework;
using Lame.Backend.Exports.Models;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Backend.Exports.LocalEF;

public class ExportsLocalEF : IExports
{
    private readonly IExporterFactory _exporterFactory;
    private readonly IServiceProvider _serviceProvider;

    public ExportsLocalEF(IServiceProvider serviceProvider, IExporterFactory exporterFactory)
    {
        _serviceProvider = serviceProvider;
        _exporterFactory = exporterFactory;
    }

    public Task<byte[]> Export(ExportOptions exportOptions)
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (exportOptions == null) throw new ArgumentNullException(nameof(exportOptions));
            if (exportOptions.LanguageCode == null) throw new ArgumentNullException(nameof(exportOptions.LanguageCode));

            var records = await context.Assets
                // Apply translation status filter
                .Where(a =>
                    // All filter applies no filter
                    exportOptions.TranslationStatusFilter == ExportTranslationStatusFilter.All ||

                    // Complete filter checks there is content in the target translation
                    (exportOptions.TranslationStatusFilter == ExportTranslationStatusFilter.Complete &&
                     a.Translations.Any(t =>
                         t.Language == exportOptions.LanguageCode && !string.IsNullOrEmpty(t.Content))) ||

                    // Missing filter checks there is no content in the target translation
                    (exportOptions.TranslationStatusFilter == ExportTranslationStatusFilter.Missing &&
                     !a.Translations.Any(t =>
                         t.Language == exportOptions.LanguageCode && !string.IsNullOrEmpty(t.Content)))
                )
                // Apply tag filters
                .Where(a =>
                    // Any filter
                    (exportOptions.TagFilter == ExportTagFilterType.Any &&
                     (exportOptions.Tags.Count == 0 ||
                      a.Tags.Any(t => exportOptions.Tags.Select(et => et.Id).Contains(t.Id)))) ||

                    // All filter
                    (exportOptions.TagFilter == ExportTagFilterType.All &&
                     a.Tags.All(t => exportOptions.Tags.Select(et => et.Id).Contains(t.Id)) &&
                     a.Tags.Count >= exportOptions.Tags.Count) ||

                    // Only filter
                    (exportOptions.TagFilter == ExportTagFilterType.Only &&
                     a.Tags.All(t => exportOptions.Tags.Select(et => et.Id).Contains(t.Id)) &&
                     a.Tags.Count == exportOptions.Tags.Count)
                )
                .Select(a => new AssetExportData
                {
                    // Asset Data
                    Id = a.Id,
                    InternalName = a.InternalName,
                    Context = a.ContextNotes ?? string.Empty,

                    // Get the targeted source translation (always english) and create translation export data
                    SourceTranslation = a.TargetedTranslations
                        .Where(t => t.Language == "en")
                        .Select(t =>
                            new TranslationExportData
                            {
                                Id = t.TranslationId,
                                Content = t.Translation.Content ?? string.Empty
                            }).FirstOrDefault(),


                    // Get the targeted target translation (in export options) and create translation export data
                    TargetTranslation = a.TargetedTranslations
                        .Where(t => t.Language == exportOptions.LanguageCode)
                        .Select(t =>
                            new TranslationExportData
                            {
                                Id = t.TranslationId,
                                Content = t.Translation.Content ?? string.Empty
                            }).FirstOrDefault()
                }).ToListAsync();

            return _exporterFactory
                .GetExporter(exportOptions.Format)
                .Export(records, "en", exportOptions.LanguageCode);
        });
    }
}