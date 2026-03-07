using System.Collections.ObjectModel;
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
        AssetDto? asset,
        Func<AssetDto, Task> handleLinkAsset,
        IAssets assetService,
        INotificationService notificationService,
        IDialogService dialogService)
    {
        _handleLinkAsset = handleLinkAsset;
        _assetService = assetService;
        _notificationService = notificationService;
        _dialogService = dialogService;

        if (asset != null) Asset = new AssetViewModel(asset);

        SearchResults = [];

        LinkAssetCommand = new AsyncRelayCommand(LinkAsset);
        CancelCommand = new RelayCommand(() => _dialogService.CloseDialog());
    }

    public AssetViewModel? Asset { get; }
    public ObservableCollection<AssetViewModel> SearchResults { get; }

    public AssetViewModel? SelectedAssetToLink
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand LinkAssetCommand { get; set; }
    public ICommand CancelCommand { get; }

    public bool IsLinkingAsset { get; private set; }

    public string TitleText => Asset == null
        ? "Link an asset"
        : $"Link to '{Asset.Asset.InternalName}'";

    public string SearchQuery
    {
        get;
        set
        {
            if (!SetField(ref field, value))
                return;

            if (SearchQuery != SelectedAssetToLink?.Asset.InternalName)
            {
                SelectedAssetToLink = null;
                _ = Search(value);
            }
        }
    }

    public bool IsSearchDropDownOpen
    {
        get;
        set => SetField(ref field, value);
    }

    public event Action OnAssetLinked;

    private async Task Search(string searchTerm)
    {
        if (!IsSearchDropDownOpen) return;

        var results = await _assetService.Get(searchTerm, 5);

        // Remove any search results that are no longer in the new results
        for (var i = SearchResults.Count - 1; i >= 0; i--)
        {
            var searchResultId = SearchResults[i].Asset.Id;

            if (searchResultId == SelectedAssetToLink?.Asset.Id || results.Any(x => x.Id == searchResultId))
                continue;

            SearchResults.RemoveAt(i);
        }

        // Add new results that are not in the search results
        foreach (var item in results)
        {
            if (Asset != null && item.Id == Asset.Asset.Id)
                continue;

            if (SearchResults.All(x => x.Asset.Id != item.Id))
                SearchResults.Add(new AssetViewModel(item));
        }

        // Reorder the collection to match the order of the new results
        for (var i = 0; i < SearchResults.Count; i++)
        {
            var resultIndex = results.FindIndex(x => x.Id == SearchResults[i].Asset.Id);
            SearchResults.Move(i, resultIndex);
        }
    }

    private async Task LinkAsset()
    {
        IsLinkingAsset = true;
        try
        {
            if (SelectedAssetToLink == null) throw new NullReferenceException("No asset selected to link.");

            await _handleLinkAsset(SelectedAssetToLink.Asset);

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