using Lame.Backend.Exports.Exporters;
using Lame.DomainModel;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Backend.Exports;

public class ExporterFactory : IExporterFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ExporterFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IExporter GetExporter(ExportFormatType exportType)
    {
        return exportType switch
        {
            ExportFormatType.JSON => _serviceProvider.GetRequiredService<JsonExporter>(),
            ExportFormatType.XLIFF => _serviceProvider.GetRequiredService<Xliff12Exporter>(),
            _ => throw new NotSupportedException($"Unsupported export format: {exportType}")
        };
    }
}