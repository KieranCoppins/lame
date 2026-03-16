using Lame.Frontend.Services;
using Lame.Frontend.ViewModels;
using Moq;

namespace Lame.Frontend.Tests.ServicesTests;

public class NavigationServiceTests
{
    [Fact]
    public void NavigateTo_Factory_SetsCurrentViewModelAndInvokesEvents()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(serviceProviderMock.Object);
        var viewModelMock = new Mock<PageViewModel>();
        var eventRaised = false;
        navigationService.CurrentViewModelChanged += () => eventRaised = true;

        // Act
        navigationService.NavigateTo(() => viewModelMock.Object);

        // Assert
        Assert.Equal(viewModelMock.Object, navigationService.CurrentViewModel);
        Assert.True(eventRaised);
        viewModelMock.Verify(vm => vm.OnNavigatedTo(), Times.Once);
    }

    [Fact]
    public void NavigateTo_Factory_CallsOnNavigatedFromOnPreviousViewModel()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(serviceProviderMock.Object);
        var previousViewModelMock = new Mock<PageViewModel>();
        var newViewModelMock = new Mock<PageViewModel>();
        navigationService.NavigateTo(() => previousViewModelMock.Object);

        // Act
        navigationService.NavigateTo(() => newViewModelMock.Object);

        // Assert
        previousViewModelMock.Verify(vm => vm.OnNavigatedFrom(), Times.Once);
        newViewModelMock.Verify(vm => vm.OnNavigatedTo(), Times.Once);
    }

    [Fact]
    public void NavigateTo_Factory_ThrowsWhenFactoryReturnsNull()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(serviceProviderMock.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => navigationService.NavigateTo<PageViewModel>(() => null));
    }

    [Fact]
    public void NavigateTo_Args_SetsCurrentViewModelAndInvokesEvents()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(serviceProviderMock.Object);
        var eventRaised = false;
        navigationService.CurrentViewModelChanged += () => eventRaised = true;

        // Act
        navigationService.NavigateTo<TestViewModel>("arg1", 42);

        // Assert
        Assert.IsType<TestViewModel>(navigationService.CurrentViewModel);
        Assert.NotNull(navigationService.CurrentViewModel);
        Assert.True(eventRaised);
    }

    [Fact]
    public void NavigateTo_Args_CallsOnNavigatedFromOnPreviousViewModel()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(serviceProviderMock.Object);
        var previousViewModelMock = new Mock<PageViewModel>();
        navigationService.NavigateTo(() => previousViewModelMock.Object);

        // Act
        navigationService.NavigateTo<TestViewModel>("arg1", 42);

        // Assert
        previousViewModelMock.Verify(vm => vm.OnNavigatedFrom(), Times.Once);
    }

    private class TestViewModel : PageViewModel
    {
        public TestViewModel(string arg1, int arg2)
        {
        }
    }
}