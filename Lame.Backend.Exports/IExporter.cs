using Lame.Backend.Exports.Models;

namespace Lame.Backend.Exports;

public interface IExporter
{
    public byte[] Export(List<AssetExportData> records, string sourceLanguage, string targetLanguage);
}