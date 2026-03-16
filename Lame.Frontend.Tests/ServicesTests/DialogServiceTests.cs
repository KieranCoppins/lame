using Lame.Frontend.Services;
using Moq;

namespace Lame.Frontend.Tests.ServicesTests;

public class DialogServiceTests
{
    [Fact]
    public void ShowDialog_Object_SetsActiveDialogAndIsDialogOpenAndRaisesEvent()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var dialogService = new DialogService(serviceProviderMock.Object);
        var viewModel = new object();
        var eventRaised = false;
        dialogService.ActiveDialogChanged += () => eventRaised = true;

        // Act
        dialogService.ShowDialog(viewModel);

        // Assert
        Assert.Equal(viewModel, dialogService.ActiveDialog);
        Assert.True(dialogService.IsDialogOpen);
        Assert.True(eventRaised);
    }

    [Fact]
    public void ShowDialog_Generic_CreatesInstanceSetsActiveDialogAndIsDialogOpenAndRaisesEvent()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var dialogService = new DialogService(serviceProviderMock.Object);
        var eventRaised = false;
        dialogService.ActiveDialogChanged += () => eventRaised = true;

        // Act
        dialogService.ShowDialog<TestDialogViewModel>("arg1", 42);

        // Assert
        Assert.IsType<TestDialogViewModel>(dialogService.ActiveDialog);
        Assert.True(dialogService.IsDialogOpen);
        Assert.True(eventRaised);
    }

    [Fact]
    public void CloseDialog_ResetsActiveDialogAndIsDialogOpenAndRaisesEvent()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var dialogService = new DialogService(serviceProviderMock.Object);
        dialogService.ShowDialog(new object());
        var eventRaised = false;
        dialogService.ActiveDialogChanged += () => eventRaised = true;

        // Act
        dialogService.CloseDialog();

        // Assert
        Assert.Null(dialogService.ActiveDialog);
        Assert.False(dialogService.IsDialogOpen);
        Assert.True(eventRaised);
    }

    [Fact]
    public void ShowDialog_Object_OverwritesPreviousDialog()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var dialogService = new DialogService(serviceProviderMock.Object);
        var firstViewModel = new object();
        var secondViewModel = new object();
        dialogService.ShowDialog(firstViewModel);

        // Act
        dialogService.ShowDialog(secondViewModel);

        // Assert
        Assert.Equal(secondViewModel, dialogService.ActiveDialog);
        Assert.True(dialogService.IsDialogOpen);
    }

    public class TestDialogViewModel
    {
        public TestDialogViewModel(string arg1, int arg2)
        {
        }
    }
}