using Lame.DomainModel;

namespace Lame.Frontend.Tests.TestingHelpers;

public class LanguageBuilder
{
    private readonly Language _language;

    public LanguageBuilder()
    {
        _language = new Language
        {
            LanguageCode = "en"
        };
    }

    public Language Build()
    {
        return _language;
    }

    public LanguageBuilder WithLanguageCode(string languageCode)
    {
        _language.LanguageCode = languageCode;
        return this;
    }
}