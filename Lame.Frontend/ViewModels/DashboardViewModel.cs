using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Lame.Backend.ChangeLog;
using Lame.Backend.Statistics;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Services;

namespace Lame.Frontend.ViewModels;

public class DashboardViewModel : PageViewModel
{
    private readonly IChangeLog _changeLogService;
    private readonly IStatistics _statistics;

    public DashboardViewModel(IStatistics statistics,
        INavigationService navigationService,
        IChangeLog changeLogService)
    {
        _statistics = statistics;
        _changeLogService = changeLogService;

        Page = AppPage.Dashboard;
        ProjectStatistics = new ProjectStatistics();

        SearchTagCommand = new RelayCommand<Tag>(tag => navigationService.NavigateTo<AssetLibraryViewModel>(tag.Name));

        Entries = [];
    }

    public ObservableCollection<ChangeLogEntry> Entries { get; }

    public ProjectStatistics ProjectStatistics
    {
        get;
        set
        {
            SetField(ref field, value);
            OnPropertyChanged(nameof(CompletedTranslations));
            OnPropertyChanged(nameof(TotalTranslations));
        }
    }

    public int CompletedTranslations => TotalTranslations - ProjectStatistics.MissingTranslations;

    public int TotalTranslations => ProjectStatistics.TotalAssets * ProjectStatistics.TotalLanguages;

    public ICommand SearchTagCommand { get; }

    public override async Task OnNavigatedTo()
    {
        await base.OnNavigatedTo();
        await LoadStatistics();
    }

    private async Task LoadStatistics()
    {
        ProjectStatistics = await _statistics.GetProjectStatistics();

        var entries = await _changeLogService.Get(0, 10);
        Entries.Clear();
        foreach (var entry in entries.Items) Entries.Add(entry);
    }
}