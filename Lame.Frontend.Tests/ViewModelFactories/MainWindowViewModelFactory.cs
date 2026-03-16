using Lame.Frontend.Services;
using Lame.Frontend.ViewModels;
using Moq;

namespace Lame.Frontend.Tests.ViewModelFactories;

public class MainWindowViewModelFactory
{
    public static MainWindowViewModel Create(
        INavigationService? navigationService = null,
        IServiceProvider? serviceProvider = null,
        INotificationService? notificationService = null,
        IDialogService? dialogService = null
    )
    {
        return new MainWindowViewModel(
            navigationService ?? new Mock<INavigationService>().Object,
            serviceProvider ?? new Mock<IServiceProvider>().Object,
            notificationService ?? new Mock<INotificationService>().Object,
            dialogService ?? new Mock<IDialogService>().Object);
    }
}