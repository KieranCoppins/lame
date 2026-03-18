namespace Lame.DomainModel;

public class ImportOptions
{
    public List<ImportData> ImportData { get; set; }

    /// <summary>
    ///     Any assets provided in import data that do not exist in the project will be created
    /// </summary>
    public bool CreateMissingAssets { get; set; }

    /// <summary>
    ///     When creating new translations, increment the major version.
    ///     Note, if the source content is unchanged with the existing source content, the major version will match
    ///     the existing source content's major version.
    ///     Target translation will always be created with the same major version as the source translation
    ///     regardless of this option.
    /// </summary>
    public bool ContainsMajorChanges { get; set; }

    /// <summary>
    ///     Any existing assets in the export will have their properties such as internal name and context notes updated to
    ///     match the xliff file
    /// </summary>
    public bool OverwriteExistingAssetProperties { get; set; }
}