using System.Security;
using System.Text;
using Lame.Backend.Exports.Models;

namespace Lame.Backend.Exports.Exporters;

public class Xliff12Exporter : IExporter
{
    public byte[] Export(List<AssetExportData> records, string sourceLanguage, string targetLanguage)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<xliff version=\"1.2\" xmlns=\"urn:oasis:names:tc:xliff:document:1.2\">");
        sb.AppendLine(
            $"\t<file source-language=\"{sourceLanguage}\" target-language=\"{targetLanguage}\" datatype=\"plaintext\">");
        sb.AppendLine("\t\t<body>");

        foreach (var record in records)
        {
            sb.AppendLine(
                $"\t\t\t<trans-unit id=\"{record.Id}\" resname=\"{record.InternalName}\" xml:space=\"preserve\">");
            sb.AppendLine($"\t\t\t\t<source>{SecurityElement.Escape(record.SourceTranslation?.Content)}</source>");
            sb.AppendLine($"\t\t\t\t<target>{SecurityElement.Escape(record.TargetTranslation?.Content)}</target>");
            sb.AppendLine($"\t\t\t\t<note>{SecurityElement.Escape(record.Context)}</note>");
            sb.AppendLine("\t\t\t</trans-unit>");
        }

        sb.AppendLine("\t\t</body>");
        sb.AppendLine("\t</file>");
        sb.AppendLine("</xliff>");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}