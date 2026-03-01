using System.ComponentModel;
using System.Runtime.CompilerServices;
using Lame.Frontend.Services;

namespace Lame.Frontend.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    INavigationService _navigationService;

    public BaseViewModel CurrentView => _navigationService.CurrentViewModel;
    
    public MainWindowViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        
        _navigationService.CurrentViewModelChanged += () =>
        {
            OnPropertyChanged(nameof(CurrentView));
        };
    }
}