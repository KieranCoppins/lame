using Lame.Backend.EntityFramework.Models;
using Lame.DomainModel;

namespace Lame.Backend.EntityFramework.Tests.EntityBuilders;

public class AssetEntityBuilder
{
    private readonly AssetEntity _asset;

    public AssetEntityBuilder()
    {
        _asset = new AssetEntity
        {
            Id = Guid.NewGuid(),
            InternalName = "asset1",
            ContextNotes = "context notes",
            AssetType = AssetType.Text,
            CreatedAt = DateTime.UtcNow - TimeSpan.FromDays(1),
            LastUpdatedAt = DateTime.UtcNow,
            Status = AssetStatus.Active,
            Translations = new List<TranslationEntity>(),
            LinkedTo = new List<AssetLinkEntity>(),
            LinkedFrom = new List<AssetLinkEntity>(),
            TargetedTranslations = new List<TargetAssetTranslationEntity>(),
            Tags = new List<TagEntity>()
        };
    }

    public AssetEntity Build()
    {
        return _asset;
    }

    public AssetEntityBuilder WithId(Guid id)
    {
        _asset.Id = id;
        return this;
    }

    public AssetEntityBuilder WithInternalName(string internalName)
    {
        _asset.InternalName = internalName;
        return this;
    }

    public AssetEntityBuilder WithContextNotes(string contextNotes)
    {
        _asset.ContextNotes = contextNotes;
        return this;
    }

    public AssetEntityBuilder WithAssetType(AssetType assetType)
    {
        _asset.AssetType = assetType;
        return this;
    }

    public AssetEntityBuilder WithStatus(AssetStatus status)
    {
        _asset.Status = status;
        return this;
    }

    public AssetEntityBuilder WithCreatedAt(DateTime createdAt)
    {
        _asset.CreatedAt = createdAt;
        return this;
    }

    public AssetEntityBuilder WithLastUpdatedAt(DateTime lastUpdatedAt)
    {
        _asset.LastUpdatedAt = lastUpdatedAt;
        return this;
    }

    public AssetEntityBuilder AddTranslation(TranslationEntity translation)
    {
        _asset.Translations.Add(translation);
        return this;
    }

    public AssetEntityBuilder AddLinkedContent(AssetEntity linkedAsset, bool synced = true)
    {
        var linkTo = new AssetLinkEntity
        {
            AssetEntityId = _asset.Id,
            AssetEntity = _asset,
            LinkedContentId = linkedAsset.Id,
            LinkedAssetEntity = linkedAsset,
            Synced = synced
        };

        var linkFrom = new AssetLinkEntity
        {
            AssetEntityId = linkedAsset.Id,
            AssetEntity = linkedAsset,
            LinkedContentId = _asset.Id,
            LinkedAssetEntity = _asset,
            Synced = synced
        };

        _asset.LinkedTo.Add(linkTo);
        linkedAsset.LinkedFrom.Add(linkFrom);
        return this;
    }

    public AssetEntityBuilder AddTargetedTranslation(TargetAssetTranslationEntity targetedTranslation)
    {
        _asset.TargetedTranslations.Add(targetedTranslation);
        return this;
    }

    public AssetEntityBuilder AddTag(TagEntity tag)
    {
        _asset.Tags.Add(tag);
        return this;
    }
}