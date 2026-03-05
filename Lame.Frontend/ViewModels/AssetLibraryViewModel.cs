using System.Collections.ObjectModel;
using System.Windows.Input;
using Lame.Backend.Assets;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Frontend.ViewModels;

public class AssetLibraryViewModel : PageViewModel
{
    private readonly IAssets _assets;
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;
    private readonly IServiceProvider _serviceProvider;

    public AssetLibraryViewModel(IAssets assets, INavigationService navigationService, IServiceProvider serviceProvider,
        INotificationService notificationService)
    {
        _assets = assets;
        _navigationService = navigationService;
        _serviceProvider = serviceProvider;
        _notificationService = notificationService;

        Assets = [];

        ViewAssetDetailsCommand = new RelayCommand<AssetViewModel>(OnViewAssetDetails);
        Page = AppPage.Library;
    }

    public bool IsLoading
    {
        get;
        private set => SetField(ref field, value);
    }

    public string SearchQuery
    {
        get;
        set
        {
            SetField(ref field, value);
            _ = LoadAssets();
        }
    }

    public ObservableCollection<AssetViewModel> Assets { get; }

    public ICommand ViewAssetDetailsCommand { get; }

    public override void OnNavigatedTo()
    {
        base.OnNavigatedTo();
        _ = LoadAssets();
    }

    private async Task LoadAssets()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            Assets.Clear();
            List<AssetDto> assets;

            if (string.IsNullOrEmpty(SearchQuery))
                assets = await _assets.Get();
            else
                assets = await _assets.Get(SearchQuery);

            foreach (var asset in assets) Assets.Add(new AssetViewModel(asset));
        }
        catch (Exception ex)
        {
            _notificationService.EmitNotification(
                new Notification
                {
                    Title = "Error",
                    Message = ex.Message,
                    Type = NotificationType.Failure
                }
            );
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnViewAssetDetails(AssetViewModel asset)
    {
        _navigationService.NavigateTo(() =>
            ActivatorUtilities.CreateInstance<AssetDetailsViewModel>(_serviceProvider, asset.Asset));
    }
}