using Lame.Frontend.Models;

namespace Lame.Frontend.Services;

public interface IUserSettingsService
{
    UserSettings UserSettings { get; }
    void Save();
}