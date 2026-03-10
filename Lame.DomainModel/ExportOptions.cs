namespace Lame.DomainModel;

public class ExportOptions
{
    public ExportFormatType Format { get; set; }

    public string LanguageCode { get; set; }

    public ExportTranslationStatusFilter TranslationStatusFilter { get; set; }
}