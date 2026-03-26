using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Lame.Backend.AssetLinks;
using Lame.Backend.ChangeLog;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Models;
using Lame.Frontend.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Frontend.ViewModels;

public class AssetLinkReviewViewModel : PageViewModel
{
    private readonly PopulatedAssetLink _assetLink;
    private readonly IAssetLinks _assetLinksService;
    private readonly IChangeLog _changeLogService;
    private readonly INotificationService _notificationService;
    private readonly IServiceProvider _serviceProvider;

    public AssetLinkReviewViewModel(
        IAssetLinks assetLinksService,
        IServiceProvider serviceProvider,
        PopulatedAssetLink assetLink,
        INotificationService notificationService,
        IChangeLog changeLogService)
    {
        _assetLinksService = assetLinksService;
        _serviceProvider = serviceProvider;
        _assetLink = assetLink;
        _notificationService = notificationService;
        _changeLogService = changeLogService;

        Page = AppPage.AssetSync;

        AssetViewModel =
            ActivatorUtilities.CreateInstance<AssetDetailsViewModel>(_serviceProvider, assetLink.Asset);

        _ = AssetViewModel.LoadAsset();

        LinkedAssetViewModel =
            ActivatorUtilities.CreateInstance<AssetDetailsViewModel>(_serviceProvider, assetLink.LinkedAsset);

        AssetViewModel.PropertyChanged += (_, _) => OnPropertyChanged(nameof(AssetViewModel));
        LinkedAssetViewModel.PropertyChanged += (_, _) => OnPropertyChanged(nameof(LinkedAssetViewModel));

        _ = LinkedAssetViewModel.LoadAsset();

        SyncLinkCommand = new AsyncRelayCommand(SyncLink);
    }

    public AssetDetailsViewModel AssetViewModel { get; }
    public AssetDetailsViewModel LinkedAssetViewModel { get; }

    public ICommand SyncLinkCommand { get; }

    private async Task SyncLink()
    {
        try
        {
            await _assetLinksService.SyncAssetLink(_assetLink.AssetEntityId, _assetLink.LinkedContentId);

            await _changeLogService.Create(new ChangeLogEntry
            {
                ResourceId = _assetLink.AssetEntityId,
                ResourceType = ResourceType.AssetLink,
                ResourceAction = ResourceAction.Updated,
                Message =
                    $"Synced asset link between '{_assetLink.Asset.InternalName}' and '{_assetLink.LinkedAsset.InternalName}'."
            });

            _notificationService.EmitNotification(new Notification
            {
                Title = "Link Synced",
                Message = "Successfully synced the asset link.",
                Type = NotificationType.Success
            });

            // Reload the assets to reflect the change
            await Task.WhenAll(AssetViewModel.LoadAsset(), LinkedAssetViewModel.LoadAsset());
        }
        catch (Exception ex)
        {
            _notificationService.EmitNotification(new Notification
            {
                Title = "Link Sync Failure",
                Message = $"Something went wrong with syncing this link: {ex.Message}.",
                Type = NotificationType.Failure
            });
        }
    }
}