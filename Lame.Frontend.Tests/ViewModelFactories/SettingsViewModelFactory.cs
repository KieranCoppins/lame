using Lame.Backend.Languages;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels;
using Moq;

namespace Lame.Frontend.Tests.ViewModelFactories;

public class SettingsViewModelFactory
{
    public static SettingsViewModel Create(
        IDialogService? dialogService = null,
        ILanguages? languagesService = null
    )
    {
        return new SettingsViewModel(
            dialogService ?? new Mock<IDialogService>().Object,
            languagesService ?? new Mock<ILanguages>().Object
        );
    }
}