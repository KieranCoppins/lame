using Lame.DomainModel;

namespace Lame.Backend.Assets;

/// <summary>
///     Data repository interface for managing Assets
/// </summary>
public interface IAssets
{
    Task<PaginatedResponse<AssetDto>> Get(int page, int pageSize);
    Task<List<AssetDto>> Get(string searchTerm, int limit = 10);
    Task<AssetDto?> Get(Guid id);
    Task<List<AssetDto>> Get(List<Guid> ids);
    Task Create(Asset asset);
    Task Update(Asset asset);
    Task Delete(Guid id);
}