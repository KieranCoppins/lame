using Lame.Backend.Assets;
using Lame.Backend.Languages;
using Lame.DomainModel;
using Lame.Frontend.Services;
using Lame.Frontend.Tests.TestingHelpers;
using Lame.Frontend.Tests.ViewModelFactories;
using Lame.Frontend.ViewModels;
using Moq;

namespace Lame.Frontend.Tests.ViewModelTests;

public class AssetLibraryViewModelTests
{
    [Fact]
    public async Task OnNavigatedTo_WhenCalled_LoadsAssetsAndLanguages()
    {
        // Arrange
        var assets = new List<AssetDto>
        {
            new AssetDtoBuilder().Build(),
            new AssetDtoBuilder().Build()
        };

        var languages = new List<Language>
        {
            new LanguageBuilder().WithLanguageCode("en").Build(),
            new LanguageBuilder().WithLanguageCode("fr").Build(),
            new LanguageBuilder().WithLanguageCode("es").Build()
        };

        var assetsService = new Mock<IAssets>();
        assetsService.Setup(x => x.Get()).ReturnsAsync(assets);

        var languagesService = new Mock<ILanguages>();
        languagesService.Setup(x => x.Get()).ReturnsAsync(languages);

        var vm = AssetLibraryViewModelFactory.Create(
            assetsService.Object, languagesService: languagesService.Object);

        // Act
        await vm.OnNavigatedTo();

        // Assert
        Assert.Equal(2, vm.Assets.Count);
        Assert.Equal(3, vm.SupportedLanguagesCount);
    }

    [Fact]
    public async Task OnNavigatedTo_WhenAssetsThrow_NotificationIsEmitted()
    {
        // Arrange

        var assetsService = new Mock<IAssets>();
        assetsService.Setup(x => x.Get()).ThrowsAsync(new Exception("Test exception"));

        var languagesService = new Mock<ILanguages>();
        languagesService.Setup(x => x.Get()).ReturnsAsync([]);

        var notificationService = new Mock<INotificationService>();

        var vm = AssetLibraryViewModelFactory.Create(
            assetsService.Object,
            languagesService: languagesService.Object,
            notificationService: notificationService.Object);

        // Act
        await vm.OnNavigatedTo();

        // Assert
        notificationService.Verify(
            x => x.EmitNotification(
                It.Is<Notification>(n => n.Type == NotificationType.Failure)),
            Times.Once);
    }

    [Fact]
    public async Task OnNavigatedTo_WhenLanguagesThrow_NotificationIsEmitted()
    {
        // Arrange
        var assetsService = new Mock<IAssets>();
        assetsService.Setup(x => x.Get()).ReturnsAsync([]);

        var languagesService = new Mock<ILanguages>();
        languagesService.Setup(x => x.Get()).ThrowsAsync(new Exception("Test exception"));

        var notificationService = new Mock<INotificationService>();

        var vm = AssetLibraryViewModelFactory.Create(
            assetsService.Object,
            languagesService: languagesService.Object,
            notificationService: notificationService.Object);

        // Act
        await vm.OnNavigatedTo();

        // Assert
        notificationService.Verify(
            x => x.EmitNotification(
                It.Is<Notification>(n => n.Type == NotificationType.Failure)),
            Times.Once);
    }

    [Fact]
    public async Task SettingSearchQuery_WithValidSearch_SearchesAssets()
    {
        // Arrange
        var assets = new List<AssetDto>
        {
            new AssetDtoBuilder().Build()
        };

        var assetsService = new Mock<IAssets>();
        assetsService.Setup(x => x.Get("test")).ReturnsAsync(assets);

        var vm = AssetLibraryViewModelFactory.Create(
            assetsService.Object);

        // Act
        vm.SearchQuery = "test";

        // Assert
        await vm.SearchQueryTask;
        assetsService.Verify(x => x.Get("test"), Times.Once);
        Assert.Single(vm.Assets);
    }
}