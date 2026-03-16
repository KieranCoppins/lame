using Lame.Backend.Assets;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Services;
using Lame.Frontend.Tests.TestingHelpers;
using Lame.Frontend.Tests.ViewModelFactories.Dialog;
using Moq;

namespace Lame.Frontend.Tests.ViewModelTests.DialogTests;

public class LinkAssetsDialogViewModelTests
{
    [Fact]
    public async Task SearchAssets_WhenCalled_ShouldCallAssetsService()
    {
        // Arrange
        var assetsServiceMock = new Mock<IAssets>();
        var vm = LinkAssetsDialogViewModelFactory.Create(assetsServiceMock.Object);

        var testQuery = "test query";

        // Act
        await vm.SearchAssets.Invoke(testQuery);

        // Assert
        assetsServiceMock.Verify(s => s.Get(testQuery, It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task LinkAssetCommand_ExecuteWithValidSelectedAsset_ShouldCallHandleCallback()
    {
        // Arrange
        var callbackCalled = false;
        AssetDto? assetToLinkFromCallback = null;

        var handleCallback = (AssetDto assetToLink) =>
        {
            assetToLinkFromCallback = assetToLink;
            callbackCalled = true;
            return Task.CompletedTask;
        };

        var vm = LinkAssetsDialogViewModelFactory.Create(handleLinkAsset: handleCallback);

        var eventInvoked = false;
        vm.OnAssetLinked += () => eventInvoked = true;

        vm.SelectedAssetToLink = new AssetDtoBuilder().Build();

        // Act
        vm.LinkAssetCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.LinkAssetCommand).CommandTask!;

        Assert.True(callbackCalled);
        Assert.NotNull(assetToLinkFromCallback);
        Assert.True(eventInvoked);
    }

    [Fact]
    public async Task LinkAssetCommand_ExecuteWithValidSelectedAsset_ShouldInvokeEvent()
    {
        // Arrange
        var vm = LinkAssetsDialogViewModelFactory.Create();

        var eventInvoked = false;
        vm.OnAssetLinked += () => eventInvoked = true;

        vm.SelectedAssetToLink = new AssetDtoBuilder().Build();

        // Act
        vm.LinkAssetCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.LinkAssetCommand).CommandTask!;

        Assert.True(eventInvoked);
    }

    [Fact]
    public async Task LinkAssetCommand_ExecuteWithValidSelectedAsset_ShouldCloseDialog()
    {
        // Arrange
        var dialogService = new Mock<IDialogService>();

        var vm = LinkAssetsDialogViewModelFactory.Create(
            dialogService: dialogService.Object
        );

        vm.SelectedAssetToLink = new AssetDtoBuilder().Build();

        // Act
        vm.LinkAssetCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.LinkAssetCommand).CommandTask!;

        dialogService.Verify(x => x.CloseDialog(), Times.Once);
    }

    [Fact]
    public async Task LinkAssetCommand_ExecuteWithInvalidSelectedAsset_ShouldEmitNotification()
    {
        // Arrange
        var notificationService = new Mock<INotificationService>();

        var vm = LinkAssetsDialogViewModelFactory.Create(
            notificationService: notificationService.Object
        );

        vm.SelectedAssetToLink = null;

        // Act
        vm.LinkAssetCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.LinkAssetCommand).CommandTask!;

        notificationService.Verify(x => x.EmitNotification(
                It.Is<Notification>(n => n.Type == NotificationType.Failure)
            ),
            Times.Once);
    }

    [Fact]
    public void CancelCommand_Execute_ClosesDialog()
    {
        // Arrange
        var dialogService = new Mock<IDialogService>();

        var vm = LinkAssetsDialogViewModelFactory.Create(
            dialogService: dialogService.Object
        );

        // Act
        vm.CancelCommand.Execute(null);

        // Assert
        dialogService.Verify(x => x.CloseDialog(), Times.Once);
    }

    [Fact]
    public void TitleText_WithAsset_ReturnsExpectedTitle()
    {
        // Arrange
        var asset = new AssetDtoBuilder().WithInternalName("Test Asset").Build();
        var vm = LinkAssetsDialogViewModelFactory.Create(asset: asset);

        // Act
        var titleText = vm.TitleText;

        // Assert
        Assert.Equal("Link to 'Test Asset'", titleText);
    }

    [Fact]
    public void TitleText_WithoutAsset_ReturnsExpectedTitle()
    {
        // Arrange
        var vm = LinkAssetsDialogViewModelFactory.Create();

        // Act
        var titleText = vm.TitleText;

        // Assert
        Assert.Equal("Link an asset", titleText);
    }
}