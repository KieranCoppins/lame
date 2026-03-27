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
        ILanguages? languagesService = null,
        INotificationService? notificationService = null,
        string? searchQuery = null)
    {
        return new AssetLibraryViewModel(
            assets ?? new Mock<IAssets>().Object,
            navigationService ?? new Mock<INavigationService>().Object,
            languagesService ?? new Mock<ILanguages>().Object,
            notificationService ?? new Mock<INotificationService>().Object,
            searchQuery);
    }
}