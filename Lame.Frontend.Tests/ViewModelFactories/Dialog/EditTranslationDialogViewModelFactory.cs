using Lame.Backend.AssetLinks;
using Lame.Backend.FileStorage;
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
        INotificationService? notificationService = null,
        AssetDto? owningAsset = null,
        IFileStorage? fileStorageService = null,
        ISystemIO? systemIo = null,
        IAssetLinks? assetLinksService = null
    )
    {
        return new EditTranslationDialogViewModel(
            owningAsset ?? new AssetDtoBuilder().Build(),
            translation ?? new TranslationDtoBuilder().Build(),
            dialogService ?? new Mock<IDialogService>().Object,
            translationsService ?? new Mock<ITranslations>().Object,
            notificationService ?? new Mock<INotificationService>().Object,
            fileStorageService ?? new Mock<IFileStorage>().Object,
            systemIo ?? new Mock<ISystemIO>().Object,
            assetLinksService ?? new Mock<IAssetLinks>().Object
        );
    }
}