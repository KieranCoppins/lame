using Lame.Backend.Languages;
using Lame.Frontend.Models;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels;
using Moq;

namespace Lame.Frontend.Tests.ViewModelFactories;

public class SettingsViewModelFactory
{
    public static SettingsViewModel Create(
        IDialogService? dialogService = null,
        ILanguages? languagesService = null,
        IUserSettingsService? userSettingsService = null
    )
    {
        // Constructor requires user settings have an object, lets mock it
        if (userSettingsService == null)
        {
            var userSettingsServiceMock = new Mock<IUserSettingsService>();
            userSettingsServiceMock.Setup(x => x.UserSettings)
                .Returns(new UserSettings
                {
                    BaseDirectory = "Initial Directory"
                });
            userSettingsService = userSettingsServiceMock.Object;
        }

        return new SettingsViewModel(
            dialogService ?? new Mock<IDialogService>().Object,
            languagesService ?? new Mock<ILanguages>().Object,
            userSettingsService
        );
    }
}