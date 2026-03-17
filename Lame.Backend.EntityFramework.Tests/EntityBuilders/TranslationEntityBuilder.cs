using Lame.Backend.EntityFramework.Models;

namespace Lame.Backend.EntityFramework.Tests.EntityBuilders;

public class TranslationEntityBuilder
{
    private readonly TranslationEntity _translation;

    public TranslationEntityBuilder(AssetEntity asset)
    {
        _translation = new TranslationEntity
        {
            Id = Guid.NewGuid(),
            Language = "en",
            Content = "Hello",
            Asset = asset,
            Tags = new List<TagEntity>(),
            CreatedAt = DateTime.UtcNow - TimeSpan.FromDays(1),
            MajorVersion = 1,
            MinorVersion = 0
        };
    }

    public TranslationEntity Build()
    {
        return _translation;
    }

    public TranslationEntityBuilder WithId(Guid id)
    {
        _translation.Id = id;
        return this;
    }

    public TranslationEntityBuilder WithLanguage(string language)
    {
        _translation.Language = language;
        return this;
    }

    public TranslationEntityBuilder WithContent(string content)
    {
        _translation.Content = content;
        return this;
    }

    public TranslationEntityBuilder WithCreatedAt(DateTime createdAt)
    {
        _translation.CreatedAt = createdAt;
        return this;
    }

    public TranslationEntityBuilder WithVersion(int majorVersion, int minorVersion)
    {
        _translation.MajorVersion = majorVersion;
        _translation.MinorVersion = minorVersion;
        return this;
    }

    public TranslationEntityBuilder AddTag(TagEntity tags)
    {
        _translation.Tags.Add(tags);
        return this;
    }
}