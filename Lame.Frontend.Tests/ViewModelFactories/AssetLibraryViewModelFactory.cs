using Lame.Backend.Assets;
using Lame.Backend.Languages;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels;
using Moq;

namespace Lame.Frontend.Tests.ViewModelFactories;

public class AssetLibraryViewModelFactory
{
    public static AssetLibraryViewModel Create(
        IAssets? assets = null,
        INavigationService? navigationService = null,
        IServiceProvider? serviceProvider = null,
        ILanguages? languagesService = null,
        INotificationService? notificationService = null)
    {
        return new AssetLibraryViewModel(
            assets ?? new Mock<IAssets>().Object,
            navigationService ?? new Mock<INavigationService>().Object,
            serviceProvider ?? new Mock<IServiceProvider>().Object,
            languagesService ?? new Mock<ILanguages>().Object,
            notificationService ?? new Mock<INotificationService>().Object);
    }
}