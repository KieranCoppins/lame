using Lame.Backend.ChangeLog;
using Lame.Backend.Statistics;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels;
using Moq;

namespace Lame.Frontend.Tests.ViewModelFactories;

public class DashboardViewModelFactory
{
    public static DashboardViewModel Create(
        IStatistics? statisticsService = null,
        INavigationService? navigationService = null,
        IChangeLog? changeLogService = null)
    {
        return new DashboardViewModel(
            statisticsService ?? new Mock<IStatistics>().Object,
            navigationService ?? new Mock<INavigationService>().Object,
            changeLogService ?? new Mock<IChangeLog>().Object
        );
    }
}