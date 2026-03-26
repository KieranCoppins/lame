using Lame.Backend.ChangeLog;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels;
using Moq;

namespace Lame.Frontend.Tests.ViewModelFactories;

public class ChangeLogViewModelFactory
{
    public static ChangeLogViewModel Create(
        IChangeLog? changeLogService = null,
        INotificationService? notificationService = null)
    {
        return new ChangeLogViewModel(
            changeLogService ?? new Mock<IChangeLog>().Object,
            notificationService ?? new Mock<INotificationService>().Object);
    }
}