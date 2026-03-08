using Lame.DomainModel;

namespace Lame.Backend.Translations;

/// <summary>
///     Data repository interface for managing Translations
/// </summary>
public interface ITranslations
{
    Task<List<TranslationDto>> GetForAsset(Guid assetId);
    Task Create(Translation translation);
    Task Update(Translation translation);
}