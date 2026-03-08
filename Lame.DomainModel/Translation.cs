namespace Lame.DomainModel;

/// <summary>
///     The translation of a piece of content.
/// </summary>
public class Translation
{
    public Guid Id { get; set; }

    /// <summary>
    ///     The ID of the asset that this translation is for
    /// </summary>
    public Guid AssetId { get; set; }

    /// <summary>
    ///     The translated content itself
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    ///     The language of the translation, should be in ISO 639-1 format (e.g. "en" for English, "fr" for French, etc.)
    /// </summary>
    public string Language { get; set; }

    /// <summary>
    ///     Major version of the translation, should be incremented when there are significant changes to the translation
    ///     that may require re-translation of dependent content. For example, if the source content changes significantly.
    /// </summary>
    public int MajorVersion { get; set; }

    /// <summary>
    ///     Minor version of the translation, should be incremented when there are minor changes to the translation that do
    ///     not require re-translation of dependent content. For example, if there are minor edits to the translation that
    ///     do not change the meaning significantly.
    /// </summary>
    public int MinorVersion { get; set; }

    /// <summary>
    ///     When this version of the translation was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}