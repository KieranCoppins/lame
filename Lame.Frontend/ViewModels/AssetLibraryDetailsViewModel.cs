using System.Windows.Input;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Frontend.ViewModels;

public class AssetLibraryDetailsViewModel : PageViewModel
{
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    public AssetLibraryDetailsViewModel(
        IDialogService dialogService,
        INavigationService navigationService,
        IServiceProvider serviceProvider,
        AssetDto asset)
    {
        _dialogService = dialogService;
        _navigationService = navigationService;
        Page = AppPage.Library;

        AssetDetailsViewModel = ActivatorUtilities.CreateInstance<AssetDetailsViewModel>(serviceProvider, asset);

        ReturnToLibraryCommand = new RelayCommand(() =>
            _navigationService.NavigateTo<AssetLibraryViewModel>());

        ArchiveAssetCommand = new RelayCommand(() =>
        {
            _dialogService.ShowDialog<ArchiveAssetDialogViewModel>(asset);
        });
    }

    public AssetDetailsViewModel AssetDetailsViewModel { get; }

    public ICommand ArchiveAssetCommand { get; }
    public ICommand ReturnToLibraryCommand { get; }

    public override async Task OnNavigatedTo()
    {
        await base.OnNavigatedTo();
        await AssetDetailsViewModel.LoadAsset();
    }
}