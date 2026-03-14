using Lame.DomainModel;

namespace Lame.Backend.Translations;

/// <summary>
///     Data repository interface for managing Translations
/// </summary>
public interface ITranslations
{
    /// <summary>
    ///     Gets all the targeted translations for the given asset, including missing translations with empty content for
    ///     supported languages that don't have a translation yet.
    /// </summary>
    /// <param name="assetId"></param>
    /// <returns></returns>
    Task<List<TranslationDto>> GetTargetedForAsset(Guid assetId);

    /// <summary>
    ///     Gets all the translations for the given asset and language, including all versions. Does not include missing
    ///     translations with empty content
    /// </summary>
    /// <param name="assetId"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    Task<List<TranslationDto>> GetAllForLanguageForAsset(Guid assetId, string language);

    /// <summary>
    ///     Sets the given translation as the targeted translation for its asset and language
    /// </summary>
    /// <param name="translationId"></param>
    /// <returns></returns>
    Task SetTargetTranslation(Guid translationId);

    Task Create(Translation translation);
    Task Update(Translation translation);
}