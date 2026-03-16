using Lame.Backend.Languages;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels.Dialogs;
using Moq;

namespace Lame.Frontend.Tests.ViewModelFactories.Dialog;

public class AddSupportedLanguageDialogViewModelFactory
{
    public static AddSupportedLanguageDialogViewModel Create(
        IDialogService? dialogService = null,
        ILanguages? languageService = null,
        INotificationService? notificationService = null
    )
    {
        return new AddSupportedLanguageDialogViewModel(
            dialogService ?? new Mock<IDialogService>().Object,
            languageService ?? new Mock<ILanguages>().Object,
            notificationService ?? new Mock<INotificationService>().Object
        );
    }
}