using System.Collections;
using System.Windows.Input;
using Lame.Backend.Assets;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Services;

namespace Lame.Frontend.ViewModels.Dialogs;

public class LinkAssetsDialogViewModel : BaseViewModel
{
    private readonly IAssets _assetService;
    private readonly IDialogService _dialogService;
    private readonly Func<AssetDto, Task> _handleLinkAsset;
    private readonly INotificationService _notificationService;

    public LinkAssetsDialogViewModel(
        Func<AssetDto, Task> handleLinkAsset,
        IAssets assetService,
        INotificationService notificationService,
        IDialogService dialogService,
        AssetDto? asset = null)
    {
        _handleLinkAsset = handleLinkAsset;
        _assetService = assetService;
        _notificationService = notificationService;
        _dialogService = dialogService;

        Asset = asset;

        SearchAssets = InternalSearchAssets;

        LinkAssetCommand = new AsyncRelayCommand(LinkAsset);
        CancelCommand = new RelayCommand(() => _dialogService.CloseDialog());
    }

    public Func<string, Task<IEnumerable>> SearchAssets { get; }

    public AssetDto? Asset { get; }

    public AssetDto? SelectedAssetToLink
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand LinkAssetCommand { get; set; }
    public ICommand CancelCommand { get; }

    public bool IsLinkingAsset { get; private set; }

    public string TitleText => Asset == null
        ? "Link an asset"
        : $"Link to '{Asset.InternalName}'";


    public event Action OnAssetLinked;

    private async Task<IEnumerable> InternalSearchAssets(string searchQuery)
    {
        var assets = await _assetService.Get(searchQuery, 5);

        // Filter out own asset
        return assets.Where(a => Asset == null || a.Id != Asset.Id);
    }


    private async Task LinkAsset()
    {
        IsLinkingAsset = true;
        try
        {
            if (SelectedAssetToLink == null) throw new NullReferenceException("No asset selected to link.");

            await _handleLinkAsset(SelectedAssetToLink);

            OnAssetLinked?.Invoke();

            _dialogService.CloseDialog();
        }
        catch (Exception ex)
        {
            _notificationService.EmitNotification(new Notification
            {
                Message = ex.Message,
                Type = NotificationType.Failure,
                Title = "Error linking assets"
            });
        }
        finally
        {
            IsLinkingAsset = false;
        }
    }
}