using Lame.Backend.EntityFramework.Models;

namespace Lame.Backend.EntityFramework.Tests.EntityBuilders;

public static class TargetAssetTranslationEntityBuilder
{
    public static TargetAssetTranslationEntity Build(AssetEntity asset, TranslationEntity translation)
    {
        return new TargetAssetTranslationEntity
        {
            Asset = asset,
            Translation = translation,
            AssetId = asset.Id,
            TranslationId = translation.Id,
            Language = translation.Language
        };
    }
}