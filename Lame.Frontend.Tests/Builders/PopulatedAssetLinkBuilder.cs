using Lame.DomainModel;
using Lame.Frontend.Models;

namespace Lame.Frontend.Tests.Builders;

public class PopulatedAssetLinkBuilder
{
    private readonly PopulatedAssetLink _assetLink;

    public PopulatedAssetLinkBuilder(AssetDto assetA, AssetDto assetB)
    {
        _assetLink = new PopulatedAssetLink
        {
            Asset = assetA,
            AssetEntityId = assetA.Id,
            LinkedAsset = assetB,
            LinkedContentId = assetB.Id,
            Synced = true
        };
    }

    public PopulatedAssetLink Build()
    {
        return _assetLink;
    }

    public PopulatedAssetLinkBuilder WithSynced(bool synced)
    {
        _assetLink.Synced = synced;
        return this;
    }
}