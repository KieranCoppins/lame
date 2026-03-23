using Lame.Backend.Statistics;
using Lame.Frontend.Services;
using Lame.Frontend.Tests.ViewModelFactories;
using Lame.Frontend.ViewModels;
using Lame.TestingHelpers;
using Moq;

namespace Lame.Frontend.Tests.ViewModelTests;

public class DashboardViewModelTests
{
    [Fact]
    public async Task OnNavigatedTo_WhenCalled_ShouldLoadStatistics()
    {
        // Arrange
        var statisticsServiceMock = new Mock<IStatistics>();
        var vm = DashboardViewModelFactory.Create(statisticsServiceMock.Object);

        // Act
        await vm.OnNavigatedTo();

        // Assert
        statisticsServiceMock.Verify(s => s.GetProjectStatistics(), Times.Once);
    }

    [Fact]
    public void SettingProjectStatistics_WithValue_SetsValueAndNotifiesPropertiesChanged()
    {
        // Arrange
        var vm = DashboardViewModelFactory.Create();

        var projectStatistics = new ProjectStatisticsBuilder().Build();

        var propertiesChanged = new List<string>();

        vm.PropertyChanged += (_, args) => { propertiesChanged.Add(args.PropertyName!); };

        // Act
        vm.ProjectStatistics = projectStatistics;

        // Assert
        Assert.Equal(projectStatistics, vm.ProjectStatistics);
        Assert.Contains(nameof(vm.ProjectStatistics), propertiesChanged);
        Assert.Contains(nameof(vm.TotalTranslations), propertiesChanged);
        Assert.Contains(nameof(vm.CompletedTranslations), propertiesChanged);
    }

    [Fact]
    public void CompletedTranslations_Get_CalculatesExpectedValue()
    {
        // Arrange
        var vm = DashboardViewModelFactory.Create();

        var projectStatistics = new ProjectStatisticsBuilder()
            .WithTotalAssets(10)
            .WithTotalLanguages(5)
            .WithMissingTranslations(20)
            .Build();

        // Act
        vm.ProjectStatistics = projectStatistics;

        // Assert
        Assert.Equal(30, vm.CompletedTranslations);
    }

    [Fact]
    public void TotalTranslations_Get_CalculatesExpectedValue()
    {
        // Arrange
        var vm = DashboardViewModelFactory.Create();

        var projectStatistics = new ProjectStatisticsBuilder()
            .WithTotalAssets(10)
            .WithTotalLanguages(5)
            .Build();

        // Act
        vm.ProjectStatistics = projectStatistics;

        // Assert
        Assert.Equal(50, vm.TotalTranslations);
    }

    [Fact]
    public void SearchTagCommand_WhenExecuted_NavigatesToAssetLibraryViewModelWithTagName()
    {
        // Arrange
        var navigationServiceMock = new Mock<INavigationService>();
        var statisticsServiceMock = new Mock<IStatistics>();
        var vm = DashboardViewModelFactory.Create(statisticsServiceMock.Object, navigationServiceMock.Object);

        var tag = new TagBuilder().WithName("TestTag").Build();

        // Act
        vm.SearchTagCommand.Execute(tag);

        // Assert
        navigationServiceMock.Verify(ns => ns.NavigateTo<AssetLibraryViewModel>("TestTag"), Times.Once);
    }
}