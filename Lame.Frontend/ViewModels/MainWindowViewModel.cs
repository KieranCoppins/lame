using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Services;

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

        NavigateToAssetSyncCommand = new RelayCommand(() =>
            _navigationService.NavigateTo<AssetSyncViewModel>());

        NavigateToLibraryCommand = new RelayCommand(() =>
            _navigationService.NavigateTo<AssetLibraryViewModel>());

        NavigateToDashboardCommand = new RelayCommand(() =>
            _navigationService.NavigateTo<DashboardViewModel>());

        NavigateToCreateAssetCommand = new RelayCommand(() =>
            _navigationService.NavigateTo<CreateAssetViewModel>());

        NavigateToSettingsCommand = new RelayCommand(() =>
            _navigationService.NavigateTo<SettingsViewModel>());

        NavigateToImportXliffCommand = new RelayCommand(() =>
            _navigationService.NavigateTo<ImportViewModel>());

        NavigateToExportXliffCommand = new RelayCommand(() =>
            _navigationService.NavigateTo<ExportViewModel>());

        NavigateToChangeLogCommand = new RelayCommand(() =>
            _navigationService.NavigateTo<ChangeLogViewModel>());

        CloseDialogCommand = new RelayCommand(dialogService.CloseDialog);
    }

    public bool IsDialogOpen => _dialogService.IsDialogOpen;

    public object ActiveDialog => _dialogService.ActiveDialog;

    public PageViewModel CurrentView => _navigationService.CurrentViewModel;
    public AppPage CurrentPage => _navigationService.CurrentViewModel?.Page ?? AppPage.None;

    public string AppVersion => Assembly
        .GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion.Split('+')[0] ?? "Unknown";

    public ObservableCollection<Notification> Notifications => _notificationService.Notifications;

    public ICommand NavigateToDashboardCommand { get; }
    public ICommand NavigateToLibraryCommand { get; }
    public ICommand NavigateToAssetSyncCommand { get; }
    public ICommand NavigateToCreateAssetCommand { get; }
    public ICommand NavigateToSettingsCommand { get; }
    public ICommand NavigateToImportXliffCommand { get; }
    public ICommand NavigateToExportXliffCommand { get; }
    public ICommand NavigateToChangeLogCommand { get; }
    public ICommand CloseDialogCommand { get; }
}