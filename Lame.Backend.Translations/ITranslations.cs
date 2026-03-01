using Lame.DomainModel;

namespace Lame.Backend.Translations;

/// <summary>
/// Data repository interface for managing Translations
/// </summary>
public interface ITranslations
{
    Task<List<Translation>> Get();
    Task<Translation?> Get(Guid id);
    Task<List<Translation>> GetForAsset(Guid assetId);
    Task Create(Translation translation);
    Task Update(Translation translation);
}