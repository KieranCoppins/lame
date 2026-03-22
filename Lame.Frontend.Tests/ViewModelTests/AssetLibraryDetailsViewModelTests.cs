using Lame.DomainModel;
using Lame.Frontend.Services;
using Lame.Frontend.Tests.ViewModelFactories;
using Lame.Frontend.ViewModels;
using Lame.Frontend.ViewModels.Dialogs;
using Lame.TestingHelpers;
using Moq;

namespace Lame.Frontend.Tests.ViewModelTests;

public class AssetLibraryDetailsViewModelTests
{
    [Fact]
    public void ReturnToLibraryCommand_Execute_NavigatesToLibraryViewModel()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();

        var navigationService = new Mock<INavigationService>();

        var vm = AssetLibraryDetailsViewModelFactory.Create(
            asset,
            navigationService: navigationService.Object);

        // Act
        vm.ReturnToLibraryCommand.Execute(null);

        // Assert
        navigationService.Verify(x => x.NavigateTo<AssetLibraryViewModel>(), Times.Once);
    }

    [Fact]
    public void ArchiveAssetCommand_Execute_OpensArchiveAssetDialog()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();

        var dialogService = new Mock<IDialogService>();

        var vm = AssetLibraryDetailsViewModelFactory.Create(
            asset,
            dialogService: dialogService.Object);

        // Act
        vm.ArchiveAssetCommand.Execute(null);

        // Assert
        dialogService.Verify(
            x => x.ShowDialog<ArchiveAssetDialogViewModel>(
                It.Is<object[]>(args =>
                    args.OfType<AssetDto>().Any(a => a.Id == asset.Id)
                )
            ),
            Times.Once);
    }
}