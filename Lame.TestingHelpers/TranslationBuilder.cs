using Lame.DomainModel;

namespace Lame.TestingHelpers;

public class TranslationBuilder
{
    private readonly Translation _translation;

    public TranslationBuilder()
    {
        _translation = new Translation
        {
            Id = Guid.NewGuid(),
            AssetId = Guid.NewGuid(),
            Language = "en",
            Content = "Test translation content",
            CreatedAt = DateTime.UtcNow - TimeSpan.FromDays(1),
            MajorVersion = 1,
            MinorVersion = 0
        };
    }

    public Translation Build()
    {
        return _translation;
    }

    public TranslationBuilder WithId(Guid id)
    {
        _translation.Id = id;
        return this;
    }

    public TranslationBuilder WithAssetId(Guid assetId)
    {
        _translation.AssetId = assetId;
        return this;
    }

    public TranslationBuilder WithLanguageCode(string languageCode)
    {
        _translation.Language = languageCode;
        return this;
    }

    public TranslationBuilder WithContent(string content)
    {
        _translation.Content = content;
        return this;
    }

    public TranslationBuilder WithCreatedAt(DateTime createdAt)
    {
        _translation.CreatedAt = createdAt;
        return this;
    }

    public TranslationBuilder WithVersion(int majorVersion, int minorVersion)
    {
        _translation.MajorVersion = majorVersion;
        _translation.MinorVersion = minorVersion;
        return this;
    }
}