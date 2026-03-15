using System.Text;
using System.Text.Json;
using Lame.Backend.Exports.Models;

namespace Lame.Backend.Exports.Exporters;

public class JsonExporter : IExporter
{
    public byte[] Export(List<AssetExportData> records, string sourceLanguage, string targetLanguage)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        // Export an array of objects with the internal name and the target translation content.
        var json = JsonSerializer.Serialize(
            records.Select(r => new
            {
                r.InternalName,
                r.TargetTranslation?.Content
            }), options);

        return Encoding.UTF8.GetBytes(json);
    }
}