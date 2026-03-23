namespace Lame.DomainModel;

/// <summary>
///     Internal definition of source content. This is the content that is being translated.
///     It is not the translation itself, but rather the source content that is being translated.
/// </summary>
public class Asset
{
    public Guid Id { get; set; }

    /// <summary>
    ///     Internal name of the content
    /// </summary>
    public string InternalName { get; set; }

    /// <summary>
    ///     The status of this content, allows soft deletion
    /// </summary>
    public AssetStatus Status { get; set; } = AssetStatus.Active;

    /// <summary>
    ///     The type of this content
    /// </summary>
    public AssetType AssetType { get; set; }

    /// <summary>
    ///     Context notes for the translators
    /// </summary>
    public string? ContextNotes { get; set; }

    /// <summary>
    ///     When this content was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     When the content was last updated, not the translations
    /// </summary>
    public DateTime LastUpdatedAt { get; set; }
}