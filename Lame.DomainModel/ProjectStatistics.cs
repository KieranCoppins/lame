namespace Lame.DomainModel;

public class ProjectStatistics
{
    /// <summary>
    ///     The total amount of assets in the project
    /// </summary>
    public int TotalAssets { get; set; }

    /// <summary>
    ///     The total amount of translations that are missing for the project
    /// </summary>
    public int MissingTranslations { get; set; }

    /// <summary>
    ///     The total amount of languages in the project
    /// </summary>
    public int TotalLanguages { get; set; }

    /// <summary>
    ///     The total amount of asset links that have synced equal to false
    /// </summary>
    public int TotalOutOfSyncLinks { get; set; }

    /// <summary>
    ///     The total amount of translations for each language
    /// </summary>
    public Dictionary<string, int> TranslationsByLanguage { get; set; }

    /// <summary>
    ///     The total amount of assets for each tag
    /// </summary>
    public Dictionary<Tag, int> AssetsByTag { get; set; }

    /// <summary>
    ///     The total amount of assets for each asset type
    /// </summary>
    public Dictionary<AssetType, int> AssetsByType { get; set; }
}