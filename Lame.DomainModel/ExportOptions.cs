namespace Lame.DomainModel;

public class ExportOptions
{
    public ExportFormatType Format { get; set; }

    public ExportTagFilterType TagFilter { get; set; }

    public List<Tag> Tags { get; set; }

    public string LanguageCode { get; set; }

    public ExportTranslationStatusFilter TranslationStatusFilter { get; set; }
}