using System.Windows.Input;
using Lame.Backend.Assets;
using Lame.Backend.ChangeLog;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Services;

namespace Lame.Frontend.ViewModels.Dialogs;

public class ArchiveAssetDialogViewModel : BaseViewModel
{
    private readonly IAssets _assetsService;
    private readonly IChangeLog _changeLogService;
    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;

    public ArchiveAssetDialogViewModel(
        IDialogService dialogService,
        IAssets assetsService,
        INotificationService notificationService,
        IChangeLog changeLogService,
        AssetDto asset)
    {
        _dialogService = dialogService;
        _assetsService = assetsService;
        _notificationService = notificationService;
        _changeLogService = changeLogService;
        Asset = asset;

        ArchiveCommand = new AsyncRelayCommand(ArchiveAsset);
        CloseCommand = new RelayCommand(() => _dialogService.CloseDialog());
    }

    public AssetDto Asset
    {
        get;
        private set => SetField(ref field, value);
    }

    public ICommand ArchiveCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand CloseCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsLoading
    {
        get;
        set => SetField(ref field, value);
    }

    private async Task ArchiveAsset()
    {
        try
        {
            IsLoading = true;
            await _assetsService.Delete(Asset.Id);

            await _changeLogService.Create(new ChangeLogEntry
            {
                ResourceAction = ResourceAction.Deleted,
                ResourceId = Asset.Id,
                ResourceType = ResourceType.Asset,
                Message = $"Archived asset '{Asset.InternalName}'"
            });

            _notificationService.EmitNotification(new Notification
            {
                Title = "Asset deleted",
                Message = $"The asset '{Asset.InternalName}' has been deleted.",
                Type = NotificationType.Success
            });

            _dialogService.CloseDialog();
        }
        catch (Exception ex)
        {
            _notificationService.EmitNotification(new Notification
            {
                Title = "Asset deletion failed",
                Message = $"Failed to delete asset: {ex.Message}",
                Type = NotificationType.Failure
            });
        }
        finally
        {
            IsLoading = false;
        }
    }
}