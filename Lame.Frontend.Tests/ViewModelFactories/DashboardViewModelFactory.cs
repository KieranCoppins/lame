using Lame.Backend.Statistics;
using Lame.Frontend.ViewModels;
using Moq;

namespace Lame.Frontend.Tests.ViewModelFactories;

public class DashboardViewModelFactory
{
    public static DashboardViewModel Create(
        IStatistics? statisticsService = null)
    {
        return new DashboardViewModel(
            statisticsService ?? new Mock<IStatistics>().Object
        );
    }
}