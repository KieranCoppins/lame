using System.Collections.ObjectModel;
using System.Windows.Input;
using Lame.Backend.AssetLinks;
using Lame.Backend.Assets;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Models;
using Lame.Frontend.Services;

namespace Lame.Frontend.ViewModels;

public class AssetSyncViewModel : PageViewModel
{
    private readonly IAssetLinks _assetLinksService;
    private readonly IAssets _assetsService;
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;

    public AssetSyncViewModel(
        IAssetLinks assetLinksService,
        IAssets assetsService,
        INotificationService notificationService,
        INavigationService navigationService)
    {
        _assetLinksService = assetLinksService;
        _assetsService = assetsService;
        _notificationService = notificationService;
        _navigationService = navigationService;
        Page = AppPage.AssetSync;

        AssetLinks = [];

        ReviewAssetLinkCommand = new RelayCommand<PopulatedAssetLink>(populatedAssetLink =>
        {
            _navigationService.NavigateTo<AssetLinkReviewViewModel>(populatedAssetLink);
        });
    }

    public ObservableCollection<PopulatedAssetLink> AssetLinks { get; }

    public ICommand ReviewAssetLinkCommand { get; set; }

    public override async Task OnNavigatedTo()
    {
        await base.OnNavigatedTo();
        await LoadAssetLinks();
    }

    private async Task LoadAssetLinks()
    {
        try
        {
            var assetLinks = (await _assetLinksService.GetAssetLinks())
                .OrderBy(link => link.Synced)
                .ToList();

            var assetIds = assetLinks
                .SelectMany(link => new[] { link.AssetEntityId, link.LinkedContentId })
                .Distinct()
                .ToList();

            var assets = (await _assetsService.Get(assetIds)).ToDictionary(asset => asset.Id);

            AssetLinks.Clear();
            foreach (var link in assetLinks)
            {
                var asset = assets[link.AssetEntityId];
                var linkedAsset = assets[link.LinkedContentId];
                AssetLinks.Add(new PopulatedAssetLink
                {
                    Asset = asset,
                    AssetEntityId = link.AssetEntityId,
                    LinkedAsset = linkedAsset,
                    LinkedContentId = link.LinkedContentId,
                    Synced = link.Synced
                });
            }
        }
        catch (Exception ex)
        {
            _notificationService.EmitNotification(new Notification
            {
                Title = "Error loading asset links",
                Message = $"An error occured loading asset links : {ex.Message}",
                Type = NotificationType.Failure
            });
        }
    }
}