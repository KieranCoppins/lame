using Lame.Backend.AssetLinks;
using Lame.Backend.Assets;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Services;
using Lame.Frontend.Tests.ViewModelFactories;
using Lame.TestingHelpers;
using Moq;

namespace Lame.Frontend.Tests.ViewModelTests;

public class AssetSyncViewModelTests
{
    [Fact]
    public async Task OnNavigatedTo_CallsLoadAssetLinksAndPopulatesAssetLinks()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();
        var linkedAsset = new AssetDtoBuilder().Build();

        var assetLink = new AssetLink
            { AssetEntityId = asset.Id, LinkedContentId = linkedAsset.Id, Synced = false };

        var paginatedResponse = new PaginatedResponseBuilder<AssetLink>([assetLink]).Build();

        var assetLinksService = new Mock<IAssetLinks>();
        assetLinksService.Setup(x => x.GetAssetLinks(It.IsAny<int>(), 25)).ReturnsAsync(paginatedResponse);

        var assetsService = new Mock<IAssets>();
        assetsService.Setup(x => x.Get(It.IsAny<List<Guid>>())).ReturnsAsync([asset, linkedAsset]);


        var vm = AssetSyncViewModelFactory.Create(
            assetLinksService.Object,
            assetsService.Object);

        // Act
        await vm.OnNavigatedTo();

        // Assert
        Assert.Single(vm.AssetLinks);
        Assert.Equal(assetLink.AssetEntityId, vm.AssetLinks[0].AssetEntityId);
        Assert.Equal(assetLink.LinkedContentId, vm.AssetLinks[0].LinkedContentId);
        Assert.Single(vm.PageNumbers);
    }

    [Fact]
    public async Task OnNavigatedTo_WhenAssetLinksServiceThrows_EmitsFailureNotification()
    {
        // Arrange
        var assetLinksService = new Mock<IAssetLinks>();
        assetLinksService.Setup(x => x.GetAssetLinks(It.IsAny<int>(), 25)).ThrowsAsync(new Exception("fail"));

        var notificationService = new Mock<INotificationService>();

        var vm = AssetSyncViewModelFactory.Create(
            assetLinksService.Object,
            notificationService: notificationService.Object);

        // Act
        await vm.OnNavigatedTo();

        // Assert
        notificationService.Verify(n => n.EmitNotification(
            It.Is<Notification>(notif =>
                notif.Type == NotificationType.Failure &&
                notif.Title == "Error loading asset links" &&
                notif.Message.Contains("fail"))), Times.Once);
    }

    [Fact]
    public async Task OnNavigatedTo_WhenNoAssetLinks_ReturnsEmptyAssetLinksAndPageNumbers()
    {
        // Arrange
        var assetLinksResponse = new PaginatedResponse<AssetLink>
        {
            Items = [],
            TotalPages = 0
        };


        var assetLinksService = new Mock<IAssetLinks>();
        assetLinksService.Setup(x => x.GetAssetLinks(It.IsAny<int>(), 25)).ReturnsAsync(assetLinksResponse);

        var vm = AssetSyncViewModelFactory.Create(assetLinksService.Object);

        // Act
        await vm.OnNavigatedTo();

        // Assert
        Assert.Empty(vm.AssetLinks);
        Assert.Empty(vm.PageNumbers);
    }

    [Fact]
    public async Task SetPageCommand_WithPageNumber_SetsCurrentPage()
    {
        // Arrange
        var vm = AssetSyncViewModelFactory.Create();

        // Act
        vm.SetPageCommand.Execute(2);

        // Assert
        await ((AsyncRelayCommand<int>)vm.SetPageCommand).CommandTask!;

        Assert.Equal(2, vm.CurrentPage);
    }

    [Fact]
    public async Task SetPageCommand_WhenCalled_LoadsLinksOfPage()
    {
        // Arrange
        var assetLink = new AssetLink
            { AssetEntityId = Guid.NewGuid(), LinkedContentId = Guid.NewGuid(), Synced = false };

        var paginatedResponse = new PaginatedResponseBuilder<AssetLink>([assetLink])
            .WithTotalPages(5)
            .Build();

        var assetLinksService = new Mock<IAssetLinks>();
        assetLinksService.Setup(x => x.GetAssetLinks(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(paginatedResponse);

        var vm = AssetSyncViewModelFactory.Create(assetLinksService.Object);

        // Act
        vm.SetPageCommand.Execute(2);

        // Assert
        await ((AsyncRelayCommand<int>)vm.SetPageCommand).CommandTask!;

        Assert.Equal(5, vm.PageNumbers.Count);
        assetLinksService.Verify(x => x.GetAssetLinks(2, It.IsAny<int>()), Times.Once);
    }
}