using Lame.Backend.Translations;
using Lame.DomainModel;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels.Dialogs;
using Lame.TestingHelpers;
using Moq;

namespace Lame.Frontend.Tests.ViewModelFactories.Dialog;

public class EditTranslationDialogViewModelFactory
{
    public static EditTranslationDialogViewModel Create(
        TranslationDto? translation = null,
        IDialogService? dialogService = null,
        ITranslations? translationsService = null,
        INotificationService? notificationService = null
    )
    {
        return new EditTranslationDialogViewModel(
            translation ?? new TranslationDtoBuilder().Build(),
            dialogService ?? new Mock<IDialogService>().Object,
            translationsService ?? new Mock<ITranslations>().Object,
            notificationService ?? new Mock<INotificationService>().Object
        );
    }
}