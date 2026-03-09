using Lame.Backend.Statistics;
using Lame.DomainModel;
using Lame.Frontend.Enums;

namespace Lame.Frontend.ViewModels;

public class DashboardViewModel : PageViewModel
{
    private readonly IStatistics _statistics;

    public DashboardViewModel(IStatistics statistics)
    {
        _statistics = statistics;

        Page = AppPage.Dashboard;
        ProjectStatistics = new ProjectStatistics();
    }

    public ProjectStatistics ProjectStatistics
    {
        get;
        private set
        {
            SetField(ref field, value);
            OnPropertyChanged(nameof(CompletedTranslations));
            OnPropertyChanged(nameof(TotalTranslations));
        }
    }

    public int CompletedTranslations => TotalTranslations - ProjectStatistics.MissingTranslations;

    public int TotalTranslations => ProjectStatistics.TotalAssets * ProjectStatistics.TotalLanguages;

    public override void OnNavigatedTo()
    {
        base.OnNavigatedTo();
        _ = LoadStatistics();
    }

    private async Task LoadStatistics()
    {
        ProjectStatistics = await _statistics.GetProjectStatistics();
    }
}