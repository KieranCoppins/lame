using Lame.Backend.AssetLinks;
using Lame.Frontend.Commands;
using Lame.Frontend.Services;
using Lame.Frontend.Tests.Builders;
using Lame.Frontend.Tests.ViewModelFactories;
using Lame.TestingHelpers;
using Moq;

namespace Lame.Frontend.Tests.ViewModelTests;

public class AssetLinkReviewViewModelTests
{
    [Fact]
    public async Task SyncLinkCommand_WhenSyncSucceeds_EmitsSuccessNotification()
    {
        // Arrange
        var assetA = new AssetDtoBuilder().Build();
        var assetB = new AssetDtoBuilder().Build();
        var assetLink = new PopulatedAssetLinkBuilder(assetA, assetB).Build();

        var assetLinksService = new Mock<IAssetLinks>();

        assetLinksService.Setup(x => x.SyncAssetLink(assetA.Id, assetB.Id))
            .Returns(Task.CompletedTask);

        var notificationService = new Mock<INotificationService>();

        var vm = AssetLinkReviewViewModelFactory.Create(
            assetLink,
            assetLinksService.Object,
            notificationService.Object);

        // Act
        vm.SyncLinkCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.SyncLinkCommand).CommandTask!;

        assetLinksService.Verify(n => n.SyncAssetLink(assetA.Id, assetB.Id), Times.Once);

        notificationService.Verify(n => n.EmitNotification(
            It.Is<Notification>(notif =>
                notif.Type == NotificationType.Success &&
                notif.Title == "Link Synced" &&
                notif.Message == "Successfully synced the asset link.")), Times.Once);
    }

    [Fact]
    public async Task SyncLinkCommand_WhenSyncFails_EmitsFailureNotification()
    {
        var assetA = new AssetDtoBuilder().Build();
        var assetB = new AssetDtoBuilder().Build();
        var assetLink = new PopulatedAssetLinkBuilder(assetA, assetB).Build();

        var assetLinksService = new Mock<IAssetLinks>();
        assetLinksService.Setup(x => x.SyncAssetLink(assetA.Id, assetB.Id))
            .ThrowsAsync(new Exception("sync error"));

        var notificationService = new Mock<INotificationService>();

        var vm = AssetLinkReviewViewModelFactory.Create(
            assetLink,
            assetLinksService.Object,
            notificationService.Object);

        // Act
        vm.SyncLinkCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.SyncLinkCommand).CommandTask!;

        notificationService.Verify(n => n.EmitNotification(
            It.Is<Notification>(notif =>
                notif.Type == NotificationType.Failure &&
                notif.Title == "Link Sync Failure" &&
                notif.Message.Contains("sync error"))), Times.Once);
    }
}