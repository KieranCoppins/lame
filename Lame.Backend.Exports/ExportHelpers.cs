using System.Security;
using System.Text;
using System.Text.Json;
using Lame.Backend.Exports.Models;

namespace Lame.Backend.Exports;

public static class ExportHelpers
{
    public static byte[] ExportToJson(IEnumerable<TranslationRecord> records)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(records, options);

        return Encoding.UTF8.GetBytes(json);
    }

    // Creates a XLIFF 1.2 file with the given translation records. The key is the source language record, and the value is the target language record.
    public static byte[] ExportToXliff12(
        List<Tuple<AssetMetaData, TranslationRecord, TranslationRecord?>> records,
        string sourceLanguageCode,
        string targetLanguageCode)
    {
        if (records.Count <= 0 && records.First().Item2 == null)
            throw new NullReferenceException("No records found");

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<xliff version=\"1.2\" xmlns=\"urn:oasis:names:tc:xliff:document:1.2\">");
        sb.AppendLine(
            $"\t<file source-language=\"{sourceLanguageCode}\" target-language=\"{targetLanguageCode}\" datatype=\"plaintext\">");
        sb.AppendLine("\t\t<body>");

        foreach (var record in records)
        {
            sb.AppendLine($"\t\t\t<trans-unit id=\"{record.Item2.Id}\" xml:space=\"preserve\">");
            sb.AppendLine($"\t\t\t\t<source>{SecurityElement.Escape(record.Item2.Content)}</source>");
            sb.AppendLine($"\t\t\t\t<target>{SecurityElement.Escape(record.Item3?.Content)}</target>");
            sb.AppendLine($"\t\t\t\t<note>{SecurityElement.Escape(record.Item1.Context)}</note>");
            sb.AppendLine("\t\t\t</trans-unit>");
        }

        sb.AppendLine("\t\t</body>");
        sb.AppendLine("\t</file>");
        sb.AppendLine("</xliff>");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}