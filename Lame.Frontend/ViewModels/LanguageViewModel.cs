using Lame.DomainModel;
using Panlingo.LanguageCode;
using Panlingo.LanguageCode.Models;

namespace Lame.Frontend.ViewModels;

public class LanguageViewModel : BaseViewModel
{
    private readonly Language _language;
    private readonly LanguageCodeResolver _languageCodeResolver;

    public LanguageViewModel(Language language)
    {
        _languageCodeResolver = new LanguageCodeResolver().Select(LanguageCodeEntity.EnglishName);
        _language = language;
    }

    public string LanguageCode => _language.LanguageCode;

    public string Name => LanguageCodeHelper.Resolve(LanguageCode, _languageCodeResolver);

    public override string ToString()
    {
        return Name;
    }
}