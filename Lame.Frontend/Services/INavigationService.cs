using Lame.Frontend.ViewModels;

namespace Lame.Frontend.Services;

public interface INavigationService
{
    PageViewModel CurrentViewModel { get; }
    event Action CurrentViewModelChanged;
    
    void NavigateTo<TViewModel>(Func<TViewModel> factory) where TViewModel : PageViewModel;
}