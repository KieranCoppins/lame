using Lame.Backend.Assets;
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
        IServiceProvider? serviceProvider = null,
        ITranslations? translationsService = null,
        ITags? tagsService = null,
        IAssets? assetsService = null,
        IDialogService? dialogService = null,
        INotificationService? notificationService = null,
        int supportedLanguagesCount = 0
    )
    {
        return new AssetDetailsViewModel(
            navigationService ?? new Mock<INavigationService>().Object,
            serviceProvider ?? new Mock<IServiceProvider>().Object,
            translationsService ?? new Mock<ITranslations>().Object,
            tagsService ?? new Mock<ITags>().Object,
            assetsService ?? new Mock<IAssets>().Object,
            dialogService ?? new Mock<IDialogService>().Object,
            notificationService ?? new Mock<INotificationService>().Object,
            asset,
            supportedLanguagesCount
        );
    }
}