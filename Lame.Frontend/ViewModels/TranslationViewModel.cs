using Lame.DomainModel;
using Panlingo.LanguageCode;
using Panlingo.LanguageCode.Models;

namespace Lame.Frontend.ViewModels;

public class TranslationViewModel
{
    private readonly Translation _translation;
    private readonly LanguageCodeResolver _languageCodeResolver;
    
    public string Content => _translation.Content;
    
    /// <summary>
    /// Converts the translations language code to plain text. For example, "en" would be converted to "English".
    /// </summary>
    public string Language => LanguageCodeHelper.Resolve(_translation.Language, _languageCodeResolver);
    
    public string Version => $"Version: {MajorVersion}.{MinorVersion}";
    
    public int MajorVersion => _translation.MajorVersion;
    public int MinorVersion => _translation.MinorVersion;
    
    public string CreatedAt => _translation.CreatedAt.ToString("yyyy-MM-dd");

    public TranslationViewModel(Translation translation)
    {
        _translation = translation;
        _languageCodeResolver = new LanguageCodeResolver().Select(LanguageCodeEntity.EnglishName);
    }
}