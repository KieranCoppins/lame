using Lame.DomainModel;

namespace Lame.Frontend.Tests.TestingHelpers;

public class ProjectStatisticsBuilder
{
    private readonly ProjectStatistics _statistics;

    public ProjectStatisticsBuilder()
    {
        _statistics = new ProjectStatistics
        {
            TotalAssets = 10,
            TotalLanguages = 5,
            MissingTranslations = 20,
            AssetsByType = new Dictionary<AssetType, int>(),
            AssetsByTag = new Dictionary<Tag, int>(),
            TranslationsByLanguage = new Dictionary<string, int>()
        };
    }

    public ProjectStatistics Build()
    {
        return _statistics;
    }

    public ProjectStatisticsBuilder WithTotalAssets(int totalAssets)
    {
        _statistics.TotalAssets = totalAssets;
        return this;
    }

    public ProjectStatisticsBuilder WithTotalLanguages(int totalLanguages)
    {
        _statistics.TotalLanguages = totalLanguages;
        return this;
    }

    public ProjectStatisticsBuilder WithMissingTranslations(int missingTranslations)
    {
        _statistics.MissingTranslations = missingTranslations;
        return this;
    }

    public ProjectStatisticsBuilder AddTranslationsByLanguage(string language, int translationCount)
    {
        _statistics.TranslationsByLanguage.Add(language, translationCount);
        return this;
    }

    public ProjectStatisticsBuilder AddTranslationsByTag(Tag tag, int assetCount)
    {
        _statistics.AssetsByTag.Add(tag, assetCount);
        return this;
    }

    public ProjectStatisticsBuilder AddTranslationsByAssetType(AssetType type, int assetCount)
    {
        _statistics.AssetsByType.Add(type, assetCount);
        return this;
    }
}