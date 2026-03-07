using System.Collections.ObjectModel;
using System.Windows.Input;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Frontend.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;

    public MainWindowViewModel(
        INavigationService navigationService,
        IServiceProvider serviceProvider,
        INotificationService notificationService,
        IDialogService dialogService)
    {
        _navigationService = navigationService;
        _notificationService = notificationService;
        _dialogService = dialogService;

        _navigationService.CurrentViewModelChanged += () =>
        {
            OnPropertyChanged(nameof(CurrentView));
            OnPropertyChanged(nameof(CurrentPage));
        };

        _notificationService.NotificationsChanged += () => { OnPropertyChanged(nameof(Notifications)); };

        _dialogService.ActiveDialogChanged += () =>
        {
            OnPropertyChanged(nameof(ActiveDialog));
            OnPropertyChanged(nameof(IsDialogOpen));
        };

        NavigateToLibraryCommand = new RelayCommand(() =>
            _navigationService.NavigateTo(serviceProvider.GetRequiredService<AssetLibraryViewModel>));

        NavigateToDashboardCommand = new RelayCommand(() =>
            _navigationService.NavigateTo(serviceProvider.GetRequiredService<DashboardViewModel>));

        NavigateToCreateAssetCommand = new RelayCommand(() =>
            _navigationService.NavigateTo(serviceProvider.GetRequiredService<CreateAssetViewModel>));

        CloseDialogCommand = new RelayCommand(dialogService.CloseDialog);
    }

    public bool IsDialogOpen => _dialogService.IsDialogOpen;

    public object ActiveDialog => _dialogService.ActiveDialog;

    public PageViewModel CurrentView => _navigationService.CurrentViewModel;
    public AppPage CurrentPage => _navigationService.CurrentViewModel?.Page ?? AppPage.None;

    public ObservableCollection<Notification> Notifications => _notificationService.Notifications;

    public ICommand NavigateToDashboardCommand { get; }
    public ICommand NavigateToLibraryCommand { get; }
    public ICommand NavigateToCreateAssetCommand { get; }
    public ICommand CloseDialogCommand { get; }
}