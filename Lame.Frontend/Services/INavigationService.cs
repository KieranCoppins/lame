using Lame.Frontend.ViewModels;

namespace Lame.Frontend.Services;

public interface INavigationService
{
    BaseViewModel CurrentViewModel { get; }
    event Action CurrentViewModelChanged;
    
    void NavigateTo<TViewModel>(Func<TViewModel> factory) where TViewModel : BaseViewModel;
}