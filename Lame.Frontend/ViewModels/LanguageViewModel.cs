using Lame.DomainModel;
using Panlingo.LanguageCode;
using Panlingo.LanguageCode.Models;

namespace Lame.Frontend.ViewModels;

// TODO remove this as its a wrapper. We can access langauge directly. We only need this as we are using the combo box which needs the to string method
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