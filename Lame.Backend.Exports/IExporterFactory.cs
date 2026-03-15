using Lame.DomainModel;

namespace Lame.Backend.Exports;

public interface IExporterFactory
{
    IExporter GetExporter(ExportFormatType exportType);
}