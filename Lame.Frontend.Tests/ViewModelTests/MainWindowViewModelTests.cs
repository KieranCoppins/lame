using Lame.Frontend.Services;
using Lame.Frontend.Tests.ViewModelFactories;
using Lame.Frontend.ViewModels;
using Moq;

namespace Lame.Frontend.Tests.ViewModelTests;

public class MainWindowViewModelTests
{
    [Fact]
    public void NavigateToLibraryCommand_Execute_NavigatesToLibrary()
    {
        // Arrange
        var navigationServiceMock = new Mock<INavigationService>();
        var vm = MainWindowViewModelFactory.Create(navigationServiceMock.Object);

        // Act
        vm.NavigateToLibraryCommand.Execute(null);

        // Assert
        navigationServiceMock.Verify(ns => ns.NavigateTo<AssetLibraryViewModel>(), Times.Once);
    }

    [Fact]
    public void NavigateToDashboardCommand_Execute_NavigatesToDashboard()
    {
        // Arrange
        var navigationServiceMock = new Mock<INavigationService>();
        var vm = MainWindowViewModelFactory.Create(navigationServiceMock.Object);

        // Act
        vm.NavigateToDashboardCommand.Execute(null);

        // Assert
        navigationServiceMock.Verify(ns => ns.NavigateTo<DashboardViewModel>(), Times.Once);
    }

    [Fact]
    public void NavigateToCreateAssetCommand_Execute_NavigatesToCreateAsset()
    {
        // Arrange
        var navigationServiceMock = new Mock<INavigationService>();
        var vm = MainWindowViewModelFactory.Create(navigationServiceMock.Object);

        // Act
        vm.NavigateToCreateAssetCommand.Execute(null);

        // Assert
        navigationServiceMock.Verify(ns => ns.NavigateTo<CreateAssetViewModel>(), Times.Once);
    }

    [Fact]
    public void NavigateToSettingsCommand_Execute_NavigatesToSettings()
    {
        // Arrange
        var navigationServiceMock = new Mock<INavigationService>();
        var vm = MainWindowViewModelFactory.Create(navigationServiceMock.Object);

        // Act
        vm.NavigateToSettingsCommand.Execute(null);

        // Assert
        navigationServiceMock.Verify(ns => ns.NavigateTo<SettingsViewModel>(), Times.Once);
    }

    [Fact]
    public void NavigateToExportXliffCommand_Execute_NavigatesToExports()
    {
        // Arrange
        var navigationServiceMock = new Mock<INavigationService>();
        var vm = MainWindowViewModelFactory.Create(navigationServiceMock.Object);

        // Act
        vm.NavigateToExportXliffCommand.Execute(null);

        // Assert
        navigationServiceMock.Verify(ns => ns.NavigateTo<ExportViewModel>(), Times.Once);
    }

    [Fact]
    public void CloseDialogCommand_Execute_CallsCloseDialogOnDialogService()
    {
        // Arrange
        var dialogService = new Mock<IDialogService>();
        var vm = MainWindowViewModelFactory.Create(dialogService: dialogService.Object);

        // Act
        vm.CloseDialogCommand.Execute(null);

        // Assert
        dialogService.Verify(x => x.CloseDialog(), Times.Once);
    }
}