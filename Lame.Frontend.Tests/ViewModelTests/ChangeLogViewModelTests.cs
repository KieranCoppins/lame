using Lame.Backend.ChangeLog;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Services;
using Lame.Frontend.Tests.ViewModelFactories;
using Moq;

namespace Lame.Frontend.Tests.ViewModelTests;

public class ChangeLogViewModelTests
{
    [Fact]
    public async Task OnNavigatedTo_WhenCalled_CallsChangeLogService()
    {
        // Arrange
        var changeLogService = new Mock<IChangeLog>();
        changeLogService.Setup(x => x.Get(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new PaginatedResponse<ChangeLogEntry>());

        var vm = ChangeLogViewModelFactory.Create(changeLogService.Object);

        // Act
        await vm.OnNavigatedTo();

        // Assert
        changeLogService.Verify(x => x.Get(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task SetPageCommand_WhenCalled_PopulatesEntriesAndPageNumbers()
    {
        // Arrange
        var entries = new PaginatedResponse<ChangeLogEntry>
        {
            Items = new List<ChangeLogEntry>
            {
                new() { Message = "Entry 1" },
                new() { Message = "Entry 2" }
            },
            TotalPages = 3
        };

        var changeLogService = new Mock<IChangeLog>();
        changeLogService.Setup(x => x.Get(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(entries);

        var vm = ChangeLogViewModelFactory.Create(changeLogService.Object);

        // Act
        vm.SetPageCommand.Execute(0);

        // Assert
        await ((AsyncRelayCommand<int>)vm.SetPageCommand).CommandTask!;

        Assert.Equal(2, vm.Entries.Count);
        Assert.Equal("Entry 1", vm.Entries[0].Message);
        Assert.Equal("Entry 2", vm.Entries[1].Message);
        Assert.Equal(3, vm.PageNumbers.Count);
        Assert.Equal(0, vm.PageNumbers[0].Number);
        Assert.Equal(1, vm.PageNumbers[1].Number);
        Assert.Equal(2, vm.PageNumbers[2].Number);
    }

    [Fact]
    public async Task SetPageCommand_WhenChangeLogServiceThrows_EmitsFailureNotification()
    {
        // Arrange
        var changeLogService = new Mock<IChangeLog>();
        changeLogService.Setup(x => x.Get(It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new Exception("fail"));

        var notificationService = new Mock<INotificationService>();

        var vm = ChangeLogViewModelFactory.Create(changeLogService.Object, notificationService.Object);

        // Act
        vm.SetPageCommand.Execute(0);

        // Assert
        await ((AsyncRelayCommand<int>)vm.SetPageCommand).CommandTask!;

        notificationService.Verify(n => n.EmitNotification(
            It.Is<Notification>(notif =>
                notif.Type == NotificationType.Failure &&
                notif.Title == "Error getting change log" &&
                notif.Message.Contains("fail"))), Times.Once);
    }

    [Fact]
    public async Task SetPageCommand_WhenNoEntries_ReturnsEmptyEntriesAndPageNumbers()
    {
        // Arrange
        var entries = new PaginatedResponse<ChangeLogEntry>
        {
            Items = new List<ChangeLogEntry>(),
            TotalPages = 0
        };

        var changeLogService = new Mock<IChangeLog>();
        changeLogService.Setup(x => x.Get(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(entries);

        var vm = ChangeLogViewModelFactory.Create(changeLogService.Object);

        // Act
        vm.SetPageCommand.Execute(0);

        // Assert
        await ((AsyncRelayCommand<int>)vm.SetPageCommand).CommandTask!;

        Assert.Empty(vm.Entries);
        Assert.Empty(vm.PageNumbers);
    }

    [Fact]
    public async Task SetPageCommand_WithPageNumber_SetsCurrentPage()
    {
        // Arrange
        var vm = ChangeLogViewModelFactory.Create();

        // Act
        vm.SetPageCommand.Execute(2);

        // Assert
        await ((AsyncRelayCommand<int>)vm.SetPageCommand).CommandTask!;

        Assert.Equal(2, vm.CurrentPage);
    }
}