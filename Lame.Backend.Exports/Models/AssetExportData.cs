namespace Lame.Backend.Exports.Models;

public class AssetExportData
{
    public Guid Id { get; set; }

    public string InternalName { get; set; }

    public string Context { get; set; }

    public TranslationExportData? SourceTranslation { get; set; }

    public TranslationExportData? TargetTranslation { get; set; }
}