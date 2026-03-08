using Lame.DomainModel;

namespace Lame.Backend.Languages;

public interface ILanguages
{
    Task<List<Language>> Get();
    Task RegisterLanguage(Language language);
}