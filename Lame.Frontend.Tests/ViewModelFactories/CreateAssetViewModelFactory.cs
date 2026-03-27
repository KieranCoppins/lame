using Lame.Backend.AssetLinks;
using Lame.Backend.Assets;
using Lame.Backend.ChangeLog;
using Lame.Backend.FileStorage;
using Lame.Backend.Languages;
using Lame.Backend.Tags;
using Lame.Backend.Translations;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels;
using Moq;

namespace Lame.Frontend.Tests.ViewModelFactories;

public class CreateAssetViewModelFactory
{
    public static CreateAssetViewModel Create(
        ITranslations? translationsService = null,
        ITags? tagsService = null,
        IAssets? assetsService = null,
        IDialogService? dialogService = null,
        ILanguages? languagesService = null,
        INotificationService? notificationService = null,
        IFileStorage? fileStorageService = null,
        ISystemIO? systemIo = null,
        IAssetLinks? assetLinksService = null,
        IChangeLog? changeLogService = null
    )
    {
        return new CreateAssetViewModel(
            assetsService ?? new Mock<IAssets>().Object,
            translationsService ?? new Mock<ITranslations>().Object,
            tagsService ?? new Mock<ITags>().Object,
            assetLinksService ?? new Mock<IAssetLinks>().Object,
            notificationService ?? new Mock<INotificationService>().Object,
            dialogService ?? new Mock<IDialogService>().Object,
            languagesService ?? new Mock<ILanguages>().Object,
            fileStorageService ?? new Mock<IFileStorage>().Object,
            systemIo ?? new Mock<ISystemIO>().Object,
            changeLogService ?? new Mock<IChangeLog>().Object
        );
    }
}