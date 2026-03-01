using Lame.DomainModel;

namespace Lame.Backend.Assets;

/// <summary>
/// Data repository interface for managing Assets
/// </summary>
public interface IAssets
{
    Task<List<Asset>> Get();
    Task<Asset?> Get(Guid id);
    Task Create(Asset asset);
    Task Update(Asset asset);
    Task Delete(Guid id);
}