using Lame.Backend.AssetLinks;
using Lame.Backend.ChangeLog;
using Lame.Frontend.Models;
using Lame.Frontend.Services;
using Lame.Frontend.Tests.Helpers;
using Lame.Frontend.ViewModels;
using Moq;

namespace Lame.Frontend.Tests.ViewModelFactories;

public class AssetLinkReviewViewModelFactory
{
    public static AssetLinkReviewViewModel Create(
        PopulatedAssetLink assetLink,
        IAssetLinks? assetLinksService = null,
        INotificationService? notificationService = null,
        IChangeLog? changeLogService = null
    )
    {
        var serviceProvider = MockedServiceProvider.Get();
        return new AssetLinkReviewViewModel(
            assetLinksService ?? new Mock<IAssetLinks>().Object,
            serviceProvider,
            assetLink,
            notificationService ?? new Mock<INotificationService>().Object,
            changeLogService ?? new Mock<IChangeLog>().Object);
    }
}