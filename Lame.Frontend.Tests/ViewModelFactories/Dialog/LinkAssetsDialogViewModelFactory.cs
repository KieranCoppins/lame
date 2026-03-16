using Lame.Backend.Assets;
using Lame.DomainModel;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels.Dialogs;
using Moq;

namespace Lame.Frontend.Tests.ViewModelFactories.Dialog;

public class LinkAssetsDialogViewModelFactory
{
    public static LinkAssetsDialogViewModel Create(
        IAssets? assetsService = null,
        INotificationService? notificationService = null,
        IDialogService? dialogService = null,
        AssetDto? asset = null,
        Func<AssetDto, Task>? handleLinkAsset = null
    )
    {
        return new LinkAssetsDialogViewModel(
            handleLinkAsset ?? (_ => Task.CompletedTask),
            assetsService ?? new Mock<IAssets>().Object,
            notificationService ?? new Mock<INotificationService>().Object,
            dialogService ?? new Mock<IDialogService>().Object,
            asset
        );
    }
}