using Lame.Frontend.ViewModels;

namespace Lame.Frontend.Services;

public class NavigationService : INavigationService
{
    public PageViewModel CurrentViewModel { get; private set; }
    
    public event Action CurrentViewModelChanged;
    
    public void NavigateTo<TViewModel>(Func<TViewModel> factory) where TViewModel : PageViewModel
    {
        CurrentViewModel = factory() ?? throw new InvalidOperationException("Navigation factory returned null.");
        CurrentViewModelChanged?.Invoke();
    }
}