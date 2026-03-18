using Lame.DomainModel;

namespace Lame.TestingHelpers;

public class AssetBuilder
{
    private readonly Asset _asset;

    public AssetBuilder()
    {
        _asset = new Asset
        {
            Id = Guid.NewGuid(),
            InternalName = "TestAsset",
            ContextNotes = "Test context notes",
            AssetType = AssetType.Text,
            CreatedAt = DateTime.UtcNow - TimeSpan.FromDays(1),
            LastUpdatedAt = DateTime.UtcNow,
            Status = AssetStatus.Active
        };
    }

    public Asset Build()
    {
        return _asset;
    }

    public AssetBuilder WithId(Guid id)
    {
        _asset.Id = id;
        return this;
    }

    public AssetBuilder WithInternalName(string internalName)
    {
        _asset.InternalName = internalName;
        return this;
    }

    public AssetBuilder WithContextNotes(string contextNotes)
    {
        _asset.ContextNotes = contextNotes;
        return this;
    }

    public AssetBuilder WithAssetType(AssetType assetType)
    {
        _asset.AssetType = assetType;
        return this;
    }

    public AssetBuilder WithCreatedAt(DateTime createdAt)
    {
        _asset.CreatedAt = createdAt;
        return this;
    }

    public AssetBuilder WithLastUpdatedAt(DateTime lastUpdatedAt)
    {
        _asset.LastUpdatedAt = lastUpdatedAt;
        return this;
    }

    public AssetBuilder WithStatus(AssetStatus status)
    {
        _asset.Status = status;
        return this;
    }
}