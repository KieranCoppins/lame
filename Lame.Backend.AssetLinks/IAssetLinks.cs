using Lame.DomainModel;

namespace Lame.Backend.AssetLinks;

public interface IAssetLinks
{
    public Task<List<AssetLink>> GetAssetLinks();
    public Task DesyncAssetLinks(Guid assetId);
    public Task SyncAssetLink(Guid assetId, Guid linkedAssetId, bool synced = true);
    public Task<AssetLink> Create(Guid assetId, Guid linkedAssetId);
    public Task Delete(Guid assetId, Guid linkedAssetId);
}