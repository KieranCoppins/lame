using System.Windows.Input;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Frontend.ViewModels;

public class AssetDetailsViewModel : AssetViewModel
{
    public ICommand ReturnToLibraryCommand { get; }
    private readonly INavigationService _navigationService;
    
    public AssetDetailsViewModel(INavigationService navigationService, IServiceProvider serviceProvider, AssetDto asset) : base(asset)
    {
        _navigationService = navigationService;
        ReturnToLibraryCommand = new RelayCommand(() =>
            _navigationService.NavigateTo(serviceProvider.GetRequiredService<AssetLibraryViewModel>));
    }
}