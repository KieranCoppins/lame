using Lame.Backend.Assets;
using Lame.DomainModel;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels.Dialogs;

namespace Lame.Frontend.Factories;

public class LinkAssetsDialogViewModelFactory
{
    private readonly IAssets _assetService;
    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;
    private readonly IServiceProvider _serviceProvider;

    public LinkAssetsDialogViewModelFactory(
        IServiceProvider serviceProvider,
        IAssets assetService,
        INotificationService notificationService,
        IDialogService dialogService)
    {
        _serviceProvider = serviceProvider;
        _assetService = assetService;
        _notificationService = notificationService;
        _dialogService = dialogService;
    }

    public LinkAssetsDialogViewModel Create(AssetDto? asset, Func<AssetDto, Task> handleLinkAsset)
    {
        return new LinkAssetsDialogViewModel(
            asset,
            handleLinkAsset,
            _assetService,
            _notificationService,
            _dialogService
        );
    }
}