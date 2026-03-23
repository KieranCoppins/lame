using Lame.DomainModel;
using Lame.Frontend.Services;
using Lame.Frontend.Tests.Helpers;
using Lame.Frontend.ViewModels;
using Moq;

namespace Lame.Frontend.Tests.ViewModelFactories;

public class AssetLibraryDetailsViewModelFactory
{
    public static AssetLibraryDetailsViewModel Create(
        AssetDto asset,
        IDialogService? dialogService = null,
        INavigationService? navigationService = null)
    {
        var serviceProvider = MockedServiceProvider.Get();

        return new AssetLibraryDetailsViewModel(
            dialogService ?? new Mock<IDialogService>().Object,
            navigationService ?? new Mock<INavigationService>().Object,
            serviceProvider,
            asset
        );
    }
}