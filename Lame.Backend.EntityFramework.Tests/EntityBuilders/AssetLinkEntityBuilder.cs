using Lame.Backend.EntityFramework.Models;

namespace Lame.Backend.EntityFramework.Tests.EntityBuilders;

public class AssetLinkEntityBuilder
{
    private readonly AssetLinkEntity _link;

    public AssetLinkEntityBuilder(AssetEntity asset, AssetEntity linkedAsset)
    {
        _link = new AssetLinkEntity
        {
            AssetEntity = asset,
            AssetEntityId = asset.Id,
            LinkedAssetEntity = linkedAsset,
            LinkedContentId = linkedAsset.Id,
            Synced = true
        };
    }

    public AssetLinkEntity Build()
    {
        return _link;
    }

    public AssetLinkEntityBuilder WithSynced(bool synced)
    {
        _link.Synced = synced;
        return this;
    }
}