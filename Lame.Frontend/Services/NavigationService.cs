using System;
using Lame.Frontend.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Frontend.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public PageViewModel CurrentViewModel { get; private set; }

    public event Action CurrentViewModelChanged;

    public void NavigateTo<TViewModel>(Func<TViewModel> factory) where TViewModel : PageViewModel
    {
        CurrentViewModel?.OnNavigatedFrom();
        CurrentViewModel = factory() ?? throw new InvalidOperationException("Navigation factory returned null.");
        CurrentViewModel.OnNavigatedTo();
        CurrentViewModelChanged?.Invoke();
    }

    public void NavigateTo<TViewModel>(params object[] args) where TViewModel : PageViewModel
    {
        CurrentViewModel?.OnNavigatedFrom();
        CurrentViewModel = ActivatorUtilities.CreateInstance<TViewModel>(_serviceProvider, args);
        CurrentViewModel.OnNavigatedTo();
        CurrentViewModelChanged?.Invoke();
    }
}