using Lame.Backend.Assets;
using Lame.Backend.ChangeLog;
using Lame.DomainModel;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels.Dialogs;
using Moq;

namespace Lame.Frontend.Tests.ViewModelFactories.Dialog;

public class ArchiveAssetDialogViewModelFactory
{
    public static ArchiveAssetDialogViewModel Create(
        IDialogService? dialogService = null,
        IAssets? assetsService = null,
        INotificationService? notificationService = null,
        IChangeLog? changeLogService = null,
        AssetDto? asset = null
    )
    {
        return new ArchiveAssetDialogViewModel(
            dialogService ?? new Mock<IDialogService>().Object,
            assetsService ?? new Mock<IAssets>().Object,
            notificationService ?? new Mock<INotificationService>().Object,
            changeLogService ?? new Mock<IChangeLog>().Object,
            asset
        );
    }
}