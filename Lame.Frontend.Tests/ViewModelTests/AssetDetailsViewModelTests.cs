using System.Collections.ObjectModel;
using Lame.Backend.Assets;
using Lame.Backend.FileStorage;
using Lame.Backend.Tags;
using Lame.Backend.Translations;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Services;
using Lame.Frontend.Tests.ViewModelFactories;
using Lame.Frontend.ViewModels;
using Lame.Frontend.ViewModels.Dialogs;
using Lame.TestingHelpers;
using Microsoft.Win32;
using Moq;

namespace Lame.Frontend.Tests.ViewModelTests;

public class AssetDetailsViewModelTests
{
    [Fact]
    public async Task OnNavigatedTo_WhenCalled_LoadsLinkedAssetsAndTranslationsAndTags()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();

        var linkedAssets = new List<AssetDto>
        {
            new AssetDtoBuilder().Build(),
            new AssetDtoBuilder().Build()
        };

        var translations = new List<TranslationDto>
        {
            new TranslationDtoBuilder().WithAssetId(asset.Id).WithLanguageCode("en").Build(),
            new TranslationDtoBuilder().WithAssetId(asset.Id).WithLanguageCode("fr").Build(),
            new TranslationDtoBuilder().WithAssetId(asset.Id).WithLanguageCode("es").Build()
        };

        var tags = new List<Tag>
        {
            new TagBuilder().Build(),
            new TagBuilder().Build()
        };


        var assetsService = new Mock<IAssets>();
        assetsService.Setup(x => x.GetLinkedAssets(asset.Id)).ReturnsAsync(linkedAssets);

        var translationsService = new Mock<ITranslations>();
        translationsService.Setup(x => x.GetTargetedForAsset(asset.Id)).ReturnsAsync(translations);

        var tagsService = new Mock<ITags>();
        tagsService.Setup(x => x.GetTagsForResource(asset.Id)).ReturnsAsync(tags);

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetsService: assetsService.Object,
            translationsService: translationsService.Object,
            tagsService: tagsService.Object);

        // Act
        await vm.OnNavigatedTo();

        // Assert
        Assert.Equal(3, vm.Translations.Count);
        Assert.Equal(2, vm.LinkedAssets.Count);
        Assert.Equal(2, vm.Tags.Count);
    }

    [Fact]
    public async Task OnNavigatedFrom_WhenCalled_UnsubscribesFromDialogServiceEvents()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();

        var dialogService = new Mock<IDialogService>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            dialogService: dialogService.Object);

        // Act
        await vm.OnNavigatedFrom();

        // Assert
        dialogService.VerifyAdd(d => d.ActiveDialogChanged += It.IsAny<Action>(), Times.Once);
        dialogService.VerifyRemove(d => d.ActiveDialogChanged -= It.IsAny<Action>(), Times.Once);
    }

    [Fact]
    public async Task LinkToAsset_WhenCalled_LinksAssetAndUpdatesLinkedAssets()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();
        var assetToLink = new AssetDtoBuilder().Build();

        var assetsService = new Mock<IAssets>();
        assetsService.Setup(x => x.LinkAssets(asset.Id, assetToLink.Id)).Returns(Task.CompletedTask);

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetsService: assetsService.Object);

        // Act
        await vm.LinkToAsset(assetToLink);

        // Assert
        Assert.Contains(assetToLink, vm.LinkedAssets);
    }

    [Fact]
    public async Task LinkToAsset_SuccessfullyLinks_EmitsSuccessNotification()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();
        var assetToLink = new AssetDtoBuilder().Build();

        var assetsService = new Mock<IAssets>();
        assetsService.Setup(x => x.LinkAssets(asset.Id, assetToLink.Id)).Returns(Task.CompletedTask);

        var notificationService = new Mock<INotificationService>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetsService: assetsService.Object,
            notificationService: notificationService.Object);

        // Act
        await vm.LinkToAsset(assetToLink);

        // Assert
        notificationService.Verify(
            x => x.EmitNotification(
                It.Is<Notification>(n => n.Type == NotificationType.Success)
            ),
            Times.Once);
    }

    [Fact]
    public async Task LinkToAsset_WhenLinkingFails_NotificationIsEmitted()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();
        var assetToLink = new AssetDtoBuilder().Build();

        var assetsService = new Mock<IAssets>();
        assetsService.Setup(x => x.LinkAssets(asset.Id, assetToLink.Id)).ThrowsAsync(new Exception("Test exception"));

        var notificationService = new Mock<INotificationService>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetsService: assetsService.Object,
            notificationService: notificationService.Object);

        // Act
        await vm.LinkToAsset(assetToLink);

        // Assert
        notificationService.Verify(
            n => n.EmitNotification(
                It.Is<Notification>(x => x.Type == NotificationType.Failure)
            ),
            Times.Once);
    }

    [Fact]
    public async Task UnLinkToAsset_WhenCalled_UnLinksAssetAndUpdatesLinkedAssets()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();
        var assetToUnLink = new AssetDtoBuilder().Build();

        var assetsService = new Mock<IAssets>();
        assetsService.Setup(x => x.UnLinkAssets(asset.Id, assetToUnLink.Id)).Returns(Task.CompletedTask);

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetsService: assetsService.Object);

        vm.LinkedAssets.Add(assetToUnLink);

        // Act
        await vm.UnLinkAsset(assetToUnLink);

        // Assert
        Assert.DoesNotContain(assetToUnLink, vm.LinkedAssets);
    }

    [Fact]
    public async Task UnLinkAsset_SuccessfullyUnLinks_EmitsSuccessNotification()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();
        var assetToUnLink = new AssetDtoBuilder().Build();

        var assetsService = new Mock<IAssets>();
        assetsService.Setup(x => x.UnLinkAssets(asset.Id, assetToUnLink.Id)).Returns(Task.CompletedTask);

        var notificationService = new Mock<INotificationService>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetsService: assetsService.Object,
            notificationService: notificationService.Object);

        // Act
        await vm.UnLinkAsset(assetToUnLink);

        // Assert
        notificationService.Verify(
            x => x.EmitNotification(
                It.Is<Notification>(n => n.Type == NotificationType.Success)
            ),
            Times.Once);
    }

    [Fact]
    public async Task UnLinkAsset_WhenUnLinkingFails_NotificationIsEmitted()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();
        var assetToUnLink = new AssetDtoBuilder().Build();

        var assetsService = new Mock<IAssets>();
        assetsService
            .Setup(x => x.UnLinkAssets(asset.Id, assetToUnLink.Id))
            .ThrowsAsync(new Exception("Test exception"));

        var notificationService = new Mock<INotificationService>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetsService: assetsService.Object,
            notificationService: notificationService.Object);

        // Act
        await vm.UnLinkAsset(assetToUnLink);

        // Assert
        notificationService.Verify(
            n => n.EmitNotification(
                It.Is<Notification>(x => x.Type == NotificationType.Failure)
            ),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsset_WhenCalled_UpdatesAsset()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();

        var assetsService = new Mock<IAssets>();
        assetsService.Setup(x => x.Update(It.IsAny<Asset>())).Returns(Task.CompletedTask);

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetsService: assetsService.Object);

        // Act
        await vm.UpdateAsset();

        // Assert
        assetsService.Verify(x => x.Update(
                It.Is<Asset>(a =>
                    a.Id == asset.Id &&
                    a.InternalName == asset.InternalName &&
                    a.Status == asset.Status &&
                    a.AssetType == asset.AssetType &&
                    a.LastUpdatedAt == asset.LastUpdatedAt &&
                    a.CreatedAt == asset.CreatedAt)
            ),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsset_WhenUpdateFails_NotificationIsEmitted()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();

        var assetsService = new Mock<IAssets>();
        assetsService.Setup(x => x.Update(It.IsAny<Asset>())).ThrowsAsync(new Exception("Test exception"));

        var notificationService = new Mock<INotificationService>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetsService: assetsService.Object,
            notificationService: notificationService.Object);

        // Act
        await vm.UpdateAsset();

        // Assert
        notificationService.Verify(
            n => n.EmitNotification(
                It.Is<Notification>(x => x.Type == NotificationType.Failure)
            ),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsset_WhenUpdateSucceeds_EmitsSuccessNotification()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();

        var assetsService = new Mock<IAssets>();
        assetsService.Setup(x => x.Update(It.IsAny<Asset>())).Returns(Task.CompletedTask);

        var notificationService = new Mock<INotificationService>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetsService: assetsService.Object,
            notificationService: notificationService.Object);

        // Act
        await vm.UpdateAsset();

        // Assert
        notificationService.Verify(
            n => n.EmitNotification(
                It.Is<Notification>(x => x.Type == NotificationType.Success)
            ),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAssetTags_WhenTagsToAddAndRemove_UpdatesTagsAndEmitsSuccessNotification()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();
        var tag1 = new TagBuilder().Build();
        var tag2 = new TagBuilder().Build();
        var tag3 = new TagBuilder().Build();

        var existingTags = new List<Tag> { tag1, tag2 };
        var newTags = new List<Tag> { tag2, tag3 };

        var tagsService = new Mock<ITags>();
        tagsService.Setup(x => x.GetTagsForResource(asset.Id)).ReturnsAsync(existingTags);
        tagsService.Setup(x => x.AddTagToResource(tag3, asset.Id, ResourceType.Asset)).Returns(Task.CompletedTask);
        tagsService.Setup(x => x.RemoveTagFromResource(tag1.Id, asset.Id)).Returns(Task.CompletedTask);

        var notificationService = new Mock<INotificationService>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            tagsService: tagsService.Object,
            notificationService: notificationService.Object);

        vm.Tags = new ObservableCollection<Tag>(newTags);

        // Act
        await vm.UpdateAssetTags();

        // Assert
        tagsService.Verify(x => x.AddTagToResource(tag3, asset.Id, ResourceType.Asset), Times.Once);
        tagsService.Verify(x => x.RemoveTagFromResource(tag1.Id, asset.Id), Times.Once);
        notificationService.Verify(
            n => n.EmitNotification(
                It.Is<Notification>(x => x.Type == NotificationType.Success)
            ),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAssetTags_WhenNoTagsToAddOrRemove_DoesNothingAndDoesNotEmitNotification()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();
        var tag1 = new TagBuilder().Build();
        var tag2 = new TagBuilder().Build();

        var existingTags = new List<Tag> { tag1, tag2 };

        var tagsService = new Mock<ITags>();
        tagsService.Setup(x => x.GetTagsForResource(asset.Id)).ReturnsAsync(existingTags);

        var notificationService = new Mock<INotificationService>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            tagsService: tagsService.Object,
            notificationService: notificationService.Object);

        vm.Tags = new ObservableCollection<Tag>(existingTags);

        // Act
        await vm.UpdateAssetTags();

        // Assert
        tagsService.Verify(x => x.AddTagToResource(It.IsAny<Tag>(), asset.Id, ResourceType.Asset), Times.Never);
        tagsService.Verify(x => x.RemoveTagFromResource(It.IsAny<Guid>(), asset.Id), Times.Never);
        notificationService.Verify(
            n => n.EmitNotification(It.IsAny<Notification>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAssetTags_WhenTagsServiceThrows_EmitsFailureNotification()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();
        var tag1 = new TagBuilder().Build();
        var tag2 = new TagBuilder().Build();
        var tag3 = new TagBuilder().Build();

        var existingTags = new List<Tag> { tag1, tag2 };
        var newTags = new List<Tag> { tag2, tag3 };

        var tagsService = new Mock<ITags>();
        tagsService.Setup(x => x.GetTagsForResource(asset.Id)).ReturnsAsync(existingTags);
        tagsService.Setup(x => x.AddTagToResource(tag3, asset.Id, ResourceType.Asset))
            .ThrowsAsync(new Exception("Test exception"));

        var notificationService = new Mock<INotificationService>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            tagsService: tagsService.Object,
            notificationService: notificationService.Object);

        vm.Tags = new ObservableCollection<Tag>(newTags);

        // Act
        await vm.UpdateAssetTags();

        // Assert
        notificationService.Verify(
            n => n.EmitNotification(
                It.Is<Notification>(x => x.Type == NotificationType.Failure)
            ),
            Times.Once);
    }

    [Fact]
    public async Task SettingInternalName_WithValidInternalName_UpdatesInternalNameAndCallsUpdateAsset()
    {
        // Arrange
        var asset = new AssetDtoBuilder().WithInternalName("Original name").Build();

        var assetsService = new Mock<IAssets>();
        assetsService.Setup(x => x.Update(It.IsAny<Asset>())).Returns(Task.CompletedTask);

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetsService: assetsService.Object);

        var newInternalName = "New Internal Name";

        // Act
        vm.InternalName = newInternalName;
        await vm.UpdateAssetTask!;

        // Assert
        Assert.Equal(newInternalName, vm.InternalName);
        assetsService.Verify(
            x => x.Update(
                It.Is<Asset>(a => a.InternalName == newInternalName)
            ),
            Times.Once);
    }

    [Fact]
    public void SettingInternalName_WithEmptyInternalName_DoesNotChange()
    {
        // Arrange
        var asset = new AssetDtoBuilder().WithInternalName("Original name").Build();

        var assetsService = new Mock<IAssets>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetsService: assetsService.Object);

        var newInternalName = "";

        // Act
        vm.InternalName = newInternalName;

        // Assert
        Assert.Equal(asset.InternalName, vm.InternalName);
        Assert.Null(vm.UpdateAssetTask);
    }

    [Fact]
    public void SettingInternalName_WithWhitespaceInternalName_DoesNotChange()
    {
        // Arrange
        var asset = new AssetDtoBuilder().WithInternalName("Original name").Build();

        var assetsService = new Mock<IAssets>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetsService: assetsService.Object);

        var newInternalName = "     ";

        // Act
        vm.InternalName = newInternalName;

        // Assert
        Assert.Equal(asset.InternalName, vm.InternalName);
        Assert.Null(vm.UpdateAssetTask);
    }

    [Fact]
    public async Task SettingContextNotes_WithContent_UpdatesContextNotesAndCallsUpdateAsset()
    {
        // Arrange
        var asset = new AssetDtoBuilder().WithContextNotes("Original Context Notes").Build();

        var assetsService = new Mock<IAssets>();
        assetsService.Setup(x => x.Update(It.IsAny<Asset>())).Returns(Task.CompletedTask);

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetsService: assetsService.Object);

        var newContextNotes = "New Context Notes";

        // Act
        vm.ContextNotes = newContextNotes;
        await vm.UpdateAssetTask!;

        // Assert
        Assert.Equal(newContextNotes, vm.ContextNotes);
        assetsService.Verify(
            x => x.Update(
                It.Is<Asset>(a => a.ContextNotes == newContextNotes)
            ),
            Times.Once);
    }

    [Fact]
    public async Task SettingContextNotes_WithNoContext_UpdatesContextNotesAndCallsUpdateAsset()
    {
        // Arrange
        var asset = new AssetDtoBuilder().WithContextNotes("Original notes").Build();

        var assetsService = new Mock<IAssets>();
        assetsService.Setup(x => x.Update(It.IsAny<Asset>())).Returns(Task.CompletedTask);

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetsService: assetsService.Object);

        var newContextNotes = "";

        // Act
        vm.ContextNotes = newContextNotes;
        await vm.UpdateAssetTask!;

        // Assert
        Assert.Equal(newContextNotes, vm.ContextNotes);
        assetsService.Verify(
            x => x.Update(
                It.Is<Asset>(a => a.ContextNotes == newContextNotes)
            ),
            Times.Once);
    }

    [Fact]
    public void ReturnToLibraryCommand_Execute_NavigatesToLibraryViewModel()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();

        var navigationService = new Mock<INavigationService>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            navigationService.Object);

        // Act
        vm.ReturnToLibraryCommand.Execute(null);

        // Assert
        navigationService.Verify(x => x.NavigateTo<AssetLibraryViewModel>(), Times.Once);
    }

    [Fact]
    public void ViewLinkedAssetDetails_Execute_NavigatesToAssetDetailsForAsset()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();
        var linkedAsset = new AssetDtoBuilder().Build();

        var navigationService = new Mock<INavigationService>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            navigationService.Object);

        // Act
        vm.ViewLinkedAssetDetails.Execute(linkedAsset);

        // Assert
        navigationService.Verify(
            x => x.NavigateTo<AssetDetailsViewModel>(
                It.Is<object[]>(args =>
                    args.OfType<AssetDto>().Any(a => a.Id == linkedAsset.Id)
                )
            ),
            Times.Once);
    }

    [Fact]
    public void OpenLinkAssetDialogCommand_Execute_OpensLinkAssetDialog()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();

        var dialogService = new Mock<IDialogService>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            dialogService: dialogService.Object);

        // Act
        vm.OpenLinkAssetDialogCommand.Execute(null);

        // Assert
        dialogService.Verify(
            x => x.ShowDialog<LinkAssetsDialogViewModel>(
                It.Is<object[]>(args =>
                    args.OfType<AssetDto>().Any(a => a.Id == asset.Id)
                )
            ),
            Times.Once);
    }

    [Fact]
    public async Task RemoveAssetLinkCommand_Execute_CallsUnlinkAsset()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();
        var linkedAssetToRemove = new AssetDtoBuilder().Build();

        var assetsService = new Mock<IAssets>();
        assetsService
            .Setup(x => x.UnLinkAssets(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetsService: assetsService.Object);

        // Act
        vm.RemoveAssetLinkCommand.Execute(linkedAssetToRemove);

        // Assert
        await ((AsyncRelayCommand<AssetDto>)vm.RemoveAssetLinkCommand).CommandTask!;
        assetsService.Verify(x => x.UnLinkAssets(asset.Id, linkedAssetToRemove.Id), Times.Once);
    }

    [Fact]
    public void EditTranslationCommand_Execute_OpensEditTranslationDialog()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();
        var translation = new TranslationDtoBuilder().WithAssetId(asset.Id).Build();

        var dialogService = new Mock<IDialogService>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            dialogService: dialogService.Object);

        // Act
        vm.EditTranslationCommand.Execute(translation);

        // Assert
        dialogService.Verify(
            x => x.ShowDialog<EditTranslationDialogViewModel>(
                It.Is<object[]>(args =>
                    args.OfType<TranslationDto>().Any(t => t.Id == translation.Id)
                )
            ),
            Times.Once);
    }

    [Fact]
    public void ArchiveAssetCommand_Execute_OpensArchiveAssetDialog()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();

        var dialogService = new Mock<IDialogService>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            dialogService: dialogService.Object);

        // Act
        vm.ArchiveAssetCommand.Execute(null);

        // Assert
        dialogService.Verify(
            x => x.ShowDialog<ArchiveAssetDialogViewModel>(
                It.Is<object[]>(args =>
                    args.OfType<AssetDto>().Any(a => a.Id == asset.Id)
                )
            ),
            Times.Once);
    }

    [Fact]
    public async Task DownloadTranslation_WhenSaveDialogCancelled_DoesNotWriteFileOrNotify()
    {
        // Arrange
        var fileStorage = new Mock<IFileStorage>();
        var systemIo = new Mock<ISystemIO>();
        var notificationService = new Mock<INotificationService>();
        var asset = new AssetDtoBuilder().WithInternalName("Asset1").Build();
        var translation = new TranslationDtoBuilder()
            .WithContent("file.txt")
            .WithLanguageCode("en")
            .WithVersion(1, 0)
            .Build();

        fileStorage.Setup(f => f.Get(translation.Content)).ReturnsAsync(new byte[] { 1, 2, 3 });
        systemIo.Setup(s => s.OpenSaveFileDialog(It.IsAny<SaveFileDialog>())).Returns(false);

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            fileStorageService: fileStorage.Object,
            systemIo: systemIo.Object,
            notificationService: notificationService.Object);

        // Act
        vm.DownloadTranslationCommand.Execute(translation);

        // Assert
        await ((AsyncRelayCommand<TranslationDto>)vm.DownloadTranslationCommand).CommandTask!;

        systemIo.Verify(s => s.WriteAllBytesAsync(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
        notificationService.Verify(n => n.EmitNotification(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task DownloadTranslation_WhenFileStorageThrows_EmitsFailureNotification()
    {
        // Arrange
        var fileStorage = new Mock<IFileStorage>();
        var systemIo = new Mock<ISystemIO>();
        var notificationService = new Mock<INotificationService>();
        var asset = new AssetDtoBuilder().WithInternalName("Asset1").Build();
        var translation = new TranslationDtoBuilder()
            .WithContent("file.txt")
            .WithLanguageCode("en")
            .WithVersion(1, 0)
            .Build();

        fileStorage.Setup(f => f.Get(translation.Content)).ThrowsAsync(new Exception("storage error"));

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            fileStorageService: fileStorage.Object,
            systemIo: systemIo.Object,
            notificationService: notificationService.Object);

        // Act
        vm.DownloadTranslationCommand.Execute(translation);

        // Assert
        await ((AsyncRelayCommand<TranslationDto>)vm.DownloadTranslationCommand).CommandTask!;

        notificationService.Verify(n => n.EmitNotification(
            It.Is<Notification>(notif =>
                notif.Type == NotificationType.Failure && notif.Title == "Download Failed" &&
                notif.Message == "storage error")), Times.Once);
    }

    [Fact]
    public async Task DownloadTranslation_WhenWriteAllBytesThrows_EmitsFailureNotification()
    {
        // Arrange
        var fileStorage = new Mock<IFileStorage>();
        var systemIo = new Mock<ISystemIO>();
        var notificationService = new Mock<INotificationService>();
        var asset = new AssetDtoBuilder().WithInternalName("Asset1").Build();
        var translation = new TranslationDtoBuilder()
            .WithContent("file.txt")
            .WithLanguageCode("en")
            .WithVersion(1, 0)
            .Build();

        fileStorage.Setup(f => f.Get(translation.Content)).ReturnsAsync(new byte[] { 1, 2, 3 });
        systemIo.Setup(s => s.OpenSaveFileDialog(It.IsAny<SaveFileDialog>()))
            .Callback<SaveFileDialog>(dlg => dlg.FileName = "test.txt")
            .Returns(true);
        systemIo.Setup(s => s.WriteAllBytesAsync("test.txt", It.IsAny<byte[]>()))
            .ThrowsAsync(new Exception("write error"));

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            fileStorageService: fileStorage.Object,
            systemIo: systemIo.Object,
            notificationService: notificationService.Object);

        // Act
        vm.DownloadTranslationCommand.Execute(translation);

        // Assert
        await ((AsyncRelayCommand<TranslationDto>)vm.DownloadTranslationCommand).CommandTask!;

        notificationService.Verify(n => n.EmitNotification(
            It.Is<Notification>(notif =>
                notif.Type == NotificationType.Failure && notif.Title == "Download Failed" &&
                notif.Message == "write error")), Times.Once);
    }

    [Fact]
    public async Task DownloadTranslation_WhenSuccessful_WritesFileAndEmitsSuccessNotification()
    {
        // Arrange
        var fileStorage = new Mock<IFileStorage>();
        var systemIo = new Mock<ISystemIO>();
        var notificationService = new Mock<INotificationService>();
        var asset = new AssetDtoBuilder().WithInternalName("Asset1").Build();
        var translation = new TranslationDtoBuilder()
            .WithContent("file.txt")
            .WithLanguageCode("en")
            .WithVersion(1, 0)
            .Build();

        var fileData = new byte[] { 1, 2, 3 };
        fileStorage.Setup(f => f.Get(translation.Content)).ReturnsAsync(fileData);
        systemIo.Setup(s => s.OpenSaveFileDialog(It.IsAny<SaveFileDialog>()))
            .Callback<SaveFileDialog>(dlg => dlg.FileName = "Asset1_en_1-0.txt")
            .Returns(true);
        systemIo.Setup(s => s.WriteAllBytesAsync("Asset1_en_1-0.txt", fileData)).Returns(Task.CompletedTask);

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            fileStorageService: fileStorage.Object,
            systemIo: systemIo.Object,
            notificationService: notificationService.Object);

        // Act
        vm.DownloadTranslationCommand.Execute(translation);

        // Assert
        await ((AsyncRelayCommand<TranslationDto>)vm.DownloadTranslationCommand).CommandTask!;

        systemIo.Verify(s => s.WriteAllBytesAsync("Asset1_en_1-0.txt", fileData), Times.Once);
        notificationService.Verify(n => n.EmitNotification(
                It.Is<Notification>(notif =>
                    notif.Type == NotificationType.Success && notif.Title == "Download Complete")),
            Times.Once);
    }
}