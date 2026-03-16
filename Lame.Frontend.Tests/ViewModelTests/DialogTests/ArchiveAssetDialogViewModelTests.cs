using Lame.Backend.Assets;
using Lame.Frontend.Commands;
using Lame.Frontend.Services;
using Lame.Frontend.Tests.TestingHelpers;
using Lame.Frontend.Tests.ViewModelFactories.Dialog;
using Moq;

namespace Lame.Frontend.Tests.ViewModelTests.DialogTests;

public class ArchiveAssetDialogViewModelTests
{
    [Fact]
    public async Task ArchiveCommand_WithAsset_ArchivesAsset()
    {
        // Arrange
        var assetsService = new Mock<IAssets>();
        var asset = new AssetDtoBuilder().Build();

        var vm = ArchiveAssetDialogViewModelFactory.Create(
            assetsService: assetsService.Object,
            asset: asset
        );

        // Act
        vm.ArchiveCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.ArchiveCommand).CommandTask!;

        assetsService.Verify(s => s.Delete(asset.Id), Times.Once);
    }

    [Fact]
    public async Task ArchiveCommand_WithAsset_EmitSuccessNotification()
    {
        // Arrange
        var notificationService = new Mock<INotificationService>();
        var asset = new AssetDtoBuilder().Build();

        var vm = ArchiveAssetDialogViewModelFactory.Create(
            notificationService: notificationService.Object,
            asset: asset
        );

        // Act
        vm.ArchiveCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.ArchiveCommand).CommandTask!;

        notificationService.Verify(s => s.EmitNotification(
                It.Is<Notification>(n => n.Type == NotificationType.Success)
            ),
            Times.Once);
    }

    [Fact]
    public async Task ArchiveCommand_WithAsset_ClosesDialog()
    {
        // Arrange
        var dialogService = new Mock<IDialogService>();
        var asset = new AssetDtoBuilder().Build();

        var vm = ArchiveAssetDialogViewModelFactory.Create(
            dialogService.Object,
            asset: asset
        );

        // Act
        vm.ArchiveCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.ArchiveCommand).CommandTask!;

        dialogService.Verify(x => x.CloseDialog(), Times.Once);
    }

    [Fact]
    public async Task ArchiveCommand_AssetServiceThrows_EmitFailureNotification()
    {
        // Arrange
        var notificationService = new Mock<INotificationService>();
        var assetsService = new Mock<IAssets>();
        assetsService.Setup(s => s.Delete(It.IsAny<Guid>())).ThrowsAsync(new Exception("Test exception"));

        var asset = new AssetDtoBuilder().Build();

        var vm = ArchiveAssetDialogViewModelFactory.Create(
            assetsService: assetsService.Object,
            notificationService: notificationService.Object,
            asset: asset
        );

        // Act
        vm.ArchiveCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.ArchiveCommand).CommandTask!;

        notificationService.Verify(s => s.EmitNotification(
                It.Is<Notification>(n => n.Type == NotificationType.Failure)
            ),
            Times.Once);
    }

    [Fact]
    public async Task ArchiveCommand_NullAsset_EmitFailureNotification()
    {
        // Arrange
        var notificationService = new Mock<INotificationService>();
        var assetsService = new Mock<IAssets>();
        assetsService.Setup(s => s.Delete(It.IsAny<Guid>())).ThrowsAsync(new Exception("Test exception"));

        var vm = ArchiveAssetDialogViewModelFactory.Create(
            assetsService: assetsService.Object,
            notificationService: notificationService.Object
        );

        // Act
        vm.ArchiveCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.ArchiveCommand).CommandTask!;

        notificationService.Verify(s => s.EmitNotification(
                It.Is<Notification>(n => n.Type == NotificationType.Failure)
            ),
            Times.Once);
    }

    [Fact]
    public void CloseCommand_Execute_ClosesDialog()
    {
        // Arrange
        var dialogService = new Mock<IDialogService>();
        var asset = new AssetDtoBuilder().Build();

        var vm = ArchiveAssetDialogViewModelFactory.Create(
            dialogService.Object,
            asset: asset
        );

        // Act
        vm.CloseCommand.Execute(null);

        // Assert
        dialogService.Verify(x => x.CloseDialog(), Times.Once);
    }
}