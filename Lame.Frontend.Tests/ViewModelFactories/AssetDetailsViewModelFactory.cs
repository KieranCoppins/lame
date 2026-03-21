using Lame.Backend.AssetLinks;
using Lame.Backend.Assets;
using Lame.Backend.FileStorage;
using Lame.Backend.Tags;
using Lame.Backend.Translations;
using Lame.DomainModel;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels;
using Moq;

namespace Lame.Frontend.Tests.ViewModelFactories;

public class AssetDetailsViewModelFactory
{
    public static AssetDetailsViewModel Create(
        AssetDto asset,
        INavigationService? navigationService = null,
        ITranslations? translationsService = null,
        ITags? tagsService = null,
        IAssets? assetsService = null,
        IDialogService? dialogService = null,
        INotificationService? notificationService = null,
        IFileStorage? fileStorageService = null,
        ISystemIO? systemIo = null,
        IAssetLinks? assetLinksService = null,
        int supportedLanguagesCount = 0
    )
    {
        return new AssetDetailsViewModel(
            navigationService ?? new Mock<INavigationService>().Object,
            translationsService ?? new Mock<ITranslations>().Object,
            tagsService ?? new Mock<ITags>().Object,
            assetsService ?? new Mock<IAssets>().Object,
            dialogService ?? new Mock<IDialogService>().Object,
            notificationService ?? new Mock<INotificationService>().Object,
            fileStorageService ?? new Mock<IFileStorage>().Object,
            systemIo ?? new Mock<ISystemIO>().Object,
            assetLinksService ?? new Mock<IAssetLinks>().Object,
            asset,
            supportedLanguagesCount
        );
    }
}