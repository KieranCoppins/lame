using Lame.Backend.AssetLinks;
using Lame.Backend.Assets;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels;
using Moq;

namespace Lame.Frontend.Tests.ViewModelFactories;

public class AssetSyncViewModelFactory
{
    public static AssetSyncViewModel Create(
        IAssetLinks? assetLinksService = null,
        IAssets? assetsService = null,
        INotificationService? notificationService = null,
        INavigationService? navigationService = null)
    {
        return new AssetSyncViewModel(
            assetLinksService ?? new Mock<IAssetLinks>().Object,
            assetsService ?? new Mock<IAssets>().Object,
            notificationService ?? new Mock<INotificationService>().Object,
            navigationService ?? new Mock<INavigationService>().Object);
    }
}