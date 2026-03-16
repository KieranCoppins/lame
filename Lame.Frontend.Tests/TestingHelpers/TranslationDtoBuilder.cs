using Lame.DomainModel;

namespace Lame.Frontend.Tests.TestingHelpers;

public class TranslationDtoBuilder
{
    private readonly TranslationDto _translationDto;

    public TranslationDtoBuilder()
    {
        _translationDto = new TranslationDto
        {
            Id = Guid.NewGuid(),
            AssetId = Guid.NewGuid(),
            Language = "en",
            Content = "Test translation content",
            CreatedAt = DateTime.UtcNow - TimeSpan.FromDays(1),
            MajorVersion = 1,
            MinorVersion = 0,
            Status = TranslationStatus.UpToDate
        };
    }

    public TranslationDto Build()
    {
        return _translationDto;
    }

    public TranslationDtoBuilder WithId(Guid id)
    {
        _translationDto.Id = id;
        return this;
    }

    public TranslationDtoBuilder WithAssetId(Guid assetId)
    {
        _translationDto.AssetId = assetId;
        return this;
    }

    public TranslationDtoBuilder WithLanguageCode(string languageCode)
    {
        _translationDto.Language = languageCode;
        return this;
    }

    public TranslationDtoBuilder WithContent(string content)
    {
        _translationDto.Content = content;
        return this;
    }

    public TranslationDtoBuilder WithCreatedAt(DateTime createdAt)
    {
        _translationDto.CreatedAt = createdAt;
        return this;
    }

    public TranslationDtoBuilder WithVersion(int majorVersion, int minorVersion)
    {
        _translationDto.MajorVersion = majorVersion;
        _translationDto.MinorVersion = minorVersion;
        return this;
    }

    public TranslationDtoBuilder WithStatus(TranslationStatus status)
    {
        _translationDto.Status = status;
        return this;
    }
}