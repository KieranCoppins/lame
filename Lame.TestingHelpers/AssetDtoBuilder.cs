using Lame.DomainModel;

namespace Lame.TestingHelpers;

public class AssetDtoBuilder
{
    private readonly AssetDto _assetDto;

    public AssetDtoBuilder()
    {
        _assetDto = new AssetDto
        {
            Id = Guid.NewGuid(),
            InternalName = "TestAsset",
            ContextNotes = "Test context notes",
            AssetType = AssetType.Text,
            CreatedAt = DateTime.UtcNow - TimeSpan.FromDays(1),
            LastUpdatedAt = DateTime.UtcNow,
            NumTranslations = 1,
            Status = AssetStatus.Active,
            AssetLinks = []
        };
    }

    public AssetDto Build()
    {
        return _assetDto;
    }

    public AssetDtoBuilder WithId(Guid id)
    {
        _assetDto.Id = id;
        return this;
    }

    public AssetDtoBuilder WithInternalName(string internalName)
    {
        _assetDto.InternalName = internalName;
        return this;
    }

    public AssetDtoBuilder WithContextNotes(string contextNotes)
    {
        _assetDto.ContextNotes = contextNotes;
        return this;
    }

    public AssetDtoBuilder WithAssetType(AssetType assetType)
    {
        _assetDto.AssetType = assetType;
        return this;
    }

    public AssetDtoBuilder WithCreatedAt(DateTime createdAt)
    {
        _assetDto.CreatedAt = createdAt;
        return this;
    }

    public AssetDtoBuilder WithLastUpdatedAt(DateTime lastUpdatedAt)
    {
        _assetDto.LastUpdatedAt = lastUpdatedAt;
        return this;
    }

    public AssetDtoBuilder WithNumTranslations(int numTranslations)
    {
        _assetDto.NumTranslations = numTranslations;
        return this;
    }

    public AssetDtoBuilder WithStatus(AssetStatus status)
    {
        _assetDto.Status = status;
        return this;
    }

    public AssetDtoBuilder AddLinkedAsset(AssetDto linkedAsset)
    {
        var link = new AssetLink
        {
            AssetEntityId = _assetDto.Id,
            LinkedContentId = linkedAsset.Id
        };

        _assetDto.AssetLinks.Add(link);
        return this;
    }
}