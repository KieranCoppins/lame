using System.Collections.ObjectModel;
using System.Windows.Input;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Frontend.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;

    public MainWindowViewModel(
        INavigationService navigationService,
        IServiceProvider serviceProvider,
        INotificationService notificationService)
    {
        _navigationService = navigationService;
        _notificationService = notificationService;

        _navigationService.CurrentViewModelChanged += () =>
        {
            OnPropertyChanged(nameof(CurrentView));
            OnPropertyChanged(nameof(CurrentPage));
        };

        _notificationService.NotificationsChanged += () => { OnPropertyChanged(nameof(Notifications)); };

        NavigateToLibraryCommand = new RelayCommand(() =>
            _navigationService.NavigateTo(serviceProvider.GetRequiredService<AssetLibraryViewModel>));

        NavigateToDashboardCommand = new RelayCommand(() =>
            _navigationService.NavigateTo(serviceProvider.GetRequiredService<DashboardViewModel>));

        NavigateToCreateAssetCommand = new RelayCommand(() =>
            _navigationService.NavigateTo(serviceProvider.GetRequiredService<CreateAssetViewModel>));
    }

    public PageViewModel CurrentView => _navigationService.CurrentViewModel;
    public AppPage CurrentPage => _navigationService.CurrentViewModel?.Page ?? AppPage.None;

    public ObservableCollection<Notification> Notifications => _notificationService.Notifications;

    public ICommand NavigateToDashboardCommand { get; }
    public ICommand NavigateToLibraryCommand { get; }
    public ICommand NavigateToCreateAssetCommand { get; }
}