using System.Windows.Input;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Frontend.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    INavigationService _navigationService;

    public PageViewModel CurrentView => _navigationService.CurrentViewModel;
    public AppPage CurrentPage => _navigationService.CurrentViewModel?.Page ?? AppPage.None;

    public ICommand NavigateToDashboardCommand { get; }
    public ICommand NavigateToLibraryCommand { get; }
    public ICommand NavigateToCreateAssetCommand { get; }
    
    public MainWindowViewModel(INavigationService navigationService, IServiceProvider serviceProvider)
    {
        _navigationService = navigationService;
        
        _navigationService.CurrentViewModelChanged += () =>
        {
            OnPropertyChanged(nameof(CurrentView));
            OnPropertyChanged(nameof(CurrentPage));
        };
        
        NavigateToLibraryCommand = new RelayCommand(() =>
            _navigationService.NavigateTo(serviceProvider.GetRequiredService<AssetLibraryViewModel>));
        
        NavigateToDashboardCommand = new RelayCommand(() =>
            _navigationService.NavigateTo(serviceProvider.GetRequiredService<DashboardViewModel>));
        
        NavigateToCreateAssetCommand = new RelayCommand(() =>
            _navigationService.NavigateTo(serviceProvider.GetRequiredService<CreateAssetViewModel>));
    }
}