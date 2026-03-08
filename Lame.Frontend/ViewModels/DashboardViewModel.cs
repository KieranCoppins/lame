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
            OnPropertyChanged(nameof(CompletionPercentage));
            OnPropertyChanged(nameof(CompletionText));
        }
    }

    public float CompletionPercentage
    {
        get
        {
            if (ProjectStatistics.TotalAssets == 0 || ProjectStatistics.TotalLanguages == 0)
                return 100;

            float totalLocalizations = ProjectStatistics.TotalAssets * ProjectStatistics.TotalLanguages;
            var completedLocalizations = totalLocalizations - ProjectStatistics.MissingTranslations;
            return completedLocalizations / totalLocalizations * 100;
        }
    }

    public string CompletionText => $"{CompletionPercentage:F2}%";

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