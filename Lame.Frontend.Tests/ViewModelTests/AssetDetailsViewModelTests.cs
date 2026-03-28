using System.Collections.ObjectModel;
using Lame.Backend.AssetLinks;
using Lame.Backend.Assets;
using Lame.Backend.ChangeLog;
using Lame.Backend.FileStorage;
using Lame.Backend.Languages;
using Lame.Backend.Tags;
using Lame.Backend.Translations;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Models;
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
    public async Task
        LoadAssets_WhenCalled_LoadsLinkedAssetsAndTranslationsAndTagsAndLanguagesAndChangeLogAndGetsAssetAgain()
    {
        // Arrange
        var linkedAssets = new List<AssetDto>
        {
            new AssetDtoBuilder().Build(),
            new AssetDtoBuilder().Build()
        };

        var asset = new AssetDtoBuilder()
            .AddLinkedAsset(linkedAssets[0])
            .AddLinkedAsset(linkedAssets[1])
            .Build();

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

        var languages = new List<Language>
        {
            new LanguageBuilder().WithLanguageCode("en").Build(),
            new LanguageBuilder().WithLanguageCode("fr").Build(),
            new LanguageBuilder().WithLanguageCode("es").Build()
        };

        var changes = new List<ChangeLogEntry>
        {
            new ChangeLogEntryBuilder().WithResourceId(asset.Id).Build(),
            new ChangeLogEntryBuilder().WithResourceId(asset.Id).Build(),
            new ChangeLogEntryBuilder().WithResourceId(asset.Id).Build(),
            new ChangeLogEntryBuilder().WithResourceId(asset.Id).Build()
        };


        var assetsService = new Mock<IAssets>();
        assetsService.Setup(x => x.Get(
                It.Is<List<Guid>>(param =>
                    param.All(id => linkedAssets
                        .Select(linked => linked.Id)
                        .Contains(id)
                    )
                )
            ))
            .ReturnsAsync(linkedAssets);

        assetsService.Setup(x => x.Get(asset.Id)).ReturnsAsync(asset);

        var translationsService = new Mock<ITranslations>();
        translationsService.Setup(x => x.GetTargetedForAsset(asset.Id)).ReturnsAsync(translations);
        translationsService.Setup(x => x.GetAllForAsset(asset.Id)).ReturnsAsync(translations);

        var tagsService = new Mock<ITags>();
        tagsService.Setup(x => x.GetTagsForResource(asset.Id)).ReturnsAsync(tags);

        var languagesService = new Mock<ILanguages>();
        languagesService.Setup(x => x.Get()).ReturnsAsync(languages);

        var changeLogService = new Mock<IChangeLog>();
        changeLogService.Setup(x => x.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<Guid>>()))
            .ReturnsAsync(new PaginatedResponseBuilder<ChangeLogEntry>(changes).WithTotalPages(3).Build());

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetsService: assetsService.Object,
            translationsService: translationsService.Object,
            tagsService: tagsService.Object,
            languagesService: languagesService.Object,
            changeLogService: changeLogService.Object);

        // Act
        await vm.LoadAsset();

        // Assert
        Assert.Equal(3, vm.Translations.Count);
        Assert.Equal(2, vm.LinkedAssets.Count);
        Assert.Equal(2, vm.Tags.Count);
        Assert.Equal(3, vm.SupportedLanguagesCount);
        Assert.Equal(4, vm.ChangeLogEntries.Count);
        Assert.Equal(3, vm.PageNumbers.Count);
    }

    // TODO how to unit test destructors?

    [Fact]
    public async Task LinkToAsset_WhenCalled_LinksAssetAndUpdatesLinkedAssetsAndCreatesChangeLog()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();
        var assetToLink = new AssetDtoBuilder().Build();

        var assetLinksService = new Mock<IAssetLinks>();
        assetLinksService.Setup(x => x.Create(asset.Id, assetToLink.Id))
            .ReturnsAsync(new AssetLink { AssetEntityId = asset.Id, LinkedContentId = assetToLink.Id, Synced = false });

        var changeLogService = new Mock<IChangeLog>();
        changeLogService.Setup(c => c.Create(It.IsAny<ChangeLogEntry>())).Returns(Task.CompletedTask);

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetLinksService: assetLinksService.Object,
            changeLogService: changeLogService.Object);

        // Act
        await vm.LinkToAsset(assetToLink);

        // Assert
        assetLinksService.Verify(x => x.Create(asset.Id, assetToLink.Id), Times.Once);
        changeLogService.Verify(x => x.Create(It.Is<ChangeLogEntry>(entry =>
            entry.ResourceAction == ResourceAction.Created &&
            entry.ResourceType == ResourceType.AssetLink &&
            entry.ResourceId == asset.Id &&
            entry.Message.Contains(asset.InternalName) &&
            entry.Message.Contains(assetToLink.InternalName)
        )), Times.Once);
        Assert.Contains(vm.LinkedAssets, x => x.LinkedContentId == assetToLink.Id);
    }

    [Fact]
    public async Task LinkToAsset_SuccessfullyLinks_EmitsSuccessNotification()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();
        var assetToLink = new AssetDtoBuilder().Build();

        var assetLinksService = new Mock<IAssetLinks>();
        assetLinksService.Setup(x => x.Create(asset.Id, assetToLink.Id))
            .ReturnsAsync(new AssetLink { AssetEntityId = asset.Id, LinkedContentId = assetToLink.Id, Synced = false });

        var notificationService = new Mock<INotificationService>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetLinksService: assetLinksService.Object,
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

        var assetLinksService = new Mock<IAssetLinks>();
        assetLinksService.Setup(x => x.Create(asset.Id, assetToLink.Id)).ThrowsAsync(new Exception("Test exception"));

        var notificationService = new Mock<INotificationService>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetLinksService: assetLinksService.Object,
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
    public async Task UnLinkToAsset_WhenCalled_UnLinksAssetAndUpdatesLinkedAssetsAndCreatesChangeLog()
    {
        // Arrange
        var assetToUnLink = new AssetDtoBuilder().Build();
        var asset = new AssetDtoBuilder().AddLinkedAsset(assetToUnLink).Build();

        var assetLinksService = new Mock<IAssetLinks>();
        assetLinksService.Setup(x => x.Delete(asset.Id, assetToUnLink.Id)).Returns(Task.CompletedTask);

        var changeLogService = new Mock<IChangeLog>();
        changeLogService.Setup(c => c.Create(It.IsAny<ChangeLogEntry>())).Returns(Task.CompletedTask);

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetLinksService: assetLinksService.Object,
            changeLogService: changeLogService.Object);

        var link = new PopulatedAssetLink
        {
            Asset = asset,
            AssetEntityId = asset.Id,
            LinkedAsset = assetToUnLink,
            LinkedContentId = assetToUnLink.Id
        };
        vm.LinkedAssets.Add(link);

        // Act
        await vm.UnLinkAsset(assetToUnLink);

        // Assert
        assetLinksService.Verify(x => x.Delete(asset.Id, assetToUnLink.Id), Times.Once);
        changeLogService.Verify(x => x.Create(It.Is<ChangeLogEntry>(entry =>
            entry.ResourceAction == ResourceAction.Deleted &&
            entry.ResourceType == ResourceType.AssetLink &&
            entry.ResourceId == asset.Id &&
            entry.Message.Contains(asset.InternalName) &&
            entry.Message.Contains(assetToUnLink.InternalName)
        )), Times.Once);
        Assert.DoesNotContain(vm.LinkedAssets, x => x.LinkedContentId == assetToUnLink.Id);
    }

    [Fact]
    public async Task UnLinkAsset_SuccessfullyUnLinks_EmitsSuccessNotification()
    {
        // Arrange
        var assetToUnLink = new AssetDtoBuilder().Build();

        var asset = new AssetDtoBuilder()
            .AddLinkedAsset(assetToUnLink)
            .Build();

        var assetLinksService = new Mock<IAssetLinks>();
        assetLinksService.Setup(x => x.Delete(asset.Id, assetToUnLink.Id)).Returns(Task.CompletedTask);

        var notificationService = new Mock<INotificationService>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetLinksService: assetLinksService.Object,
            notificationService: notificationService.Object);

        var link = new PopulatedAssetLink
        {
            Asset = asset,
            AssetEntityId = asset.Id,
            LinkedAsset = assetToUnLink,
            LinkedContentId = assetToUnLink.Id
        };
        vm.LinkedAssets.Add(link);

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

        var assetLinksService = new Mock<IAssetLinks>();
        assetLinksService
            .Setup(x => x.Delete(asset.Id, assetToUnLink.Id))
            .ThrowsAsync(new Exception("Test exception"));

        var notificationService = new Mock<INotificationService>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetLinksService: assetLinksService.Object,
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
    public async Task UpdateAsset_WhenCalled_CreatesChangeLog()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();

        var changeLogService = new Mock<IChangeLog>();
        changeLogService.Setup(x => x.Create(It.IsAny<ChangeLogEntry>())).Returns(Task.CompletedTask);

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            changeLogService: changeLogService.Object);

        // Act
        await vm.UpdateAsset();

        // Assert
        changeLogService.Verify(x => x.Create(It.Is<ChangeLogEntry>(entry =>
            entry.ResourceId == asset.Id &&
            entry.ResourceType == ResourceType.Asset &&
            entry.ResourceAction == ResourceAction.Updated &&
            entry.Message.Contains(asset.InternalName)
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateAssetTags_WhenTagsToAddAndRemove_UpdatesTagsAndCreatesChangeLogAndEmitsSuccessNotification()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();
        var tag1 = new TagBuilder().WithName("tagA").Build();
        var tag2 = new TagBuilder().WithName("tagB").Build();
        var tag3 = new TagBuilder().WithName("tagC").Build();

        var existingTags = new List<Tag> { tag1, tag2 };
        var newTags = new List<Tag> { tag2, tag3 };

        var tagsService = new Mock<ITags>();
        tagsService.Setup(x => x.GetTagsForResource(asset.Id)).ReturnsAsync(existingTags);
        tagsService.Setup(x => x.AddTagToResource(tag3, asset.Id, ResourceType.Asset)).Returns(Task.CompletedTask);
        tagsService.Setup(x => x.RemoveTagFromResource(tag1.Id, asset.Id)).Returns(Task.CompletedTask);

        var notificationService = new Mock<INotificationService>();

        var changeLogService = new Mock<IChangeLog>();
        changeLogService.Setup(x => x.Create(It.IsAny<ChangeLogEntry>())).Returns(Task.CompletedTask);

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            tagsService: tagsService.Object,
            notificationService: notificationService.Object,
            changeLogService: changeLogService.Object);

        vm.Tags = new ObservableCollection<Tag>(newTags);

        // Act
        await vm.UpdateAssetTags();

        // Assert
        var changedTags = new List<Tag> { tag1, tag3 };
        tagsService.Verify(x => x.AddTagToResource(tag3, asset.Id, ResourceType.Asset), Times.Once);
        tagsService.Verify(x => x.RemoveTagFromResource(tag1.Id, asset.Id), Times.Once);
        notificationService.Verify(
            n => n.EmitNotification(
                It.Is<Notification>(x => x.Type == NotificationType.Success)
            ),
            Times.Once);
        changeLogService.Verify(x => x.Create(It.Is<ChangeLogEntry>(entry =>
            entry.ResourceId == asset.Id &&
            entry.ResourceType == ResourceType.Asset &&
            entry.ResourceAction == ResourceAction.Updated &&
            entry.Message.Contains(asset.InternalName) &&
            changedTags.All(tag => entry.Message.Contains(tag.Name))
        )), Times.Once);
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
    public void ViewLinkedAssetDetails_Execute_NavigatesToAssetDetailsForAsset()
    {
        // Arrange
        var linkedAsset = new AssetDtoBuilder().Build();

        var asset = new AssetDtoBuilder()
            .AddLinkedAsset(linkedAsset)
            .Build();

        var navigationService = new Mock<INavigationService>();

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            navigationService.Object);

        var link = new PopulatedAssetLink
        {
            Asset = asset,
            AssetEntityId = asset.Id,
            LinkedAsset = linkedAsset,
            LinkedContentId = linkedAsset.Id
        };
        vm.LinkedAssets.Add(link);

        // Act
        vm.ViewLinkedAssetDetails.Execute(link);

        // Assert
        navigationService.Verify(
            x => x.NavigateTo<AssetLibraryDetailsViewModel>(
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

        var assetLinksService = new Mock<IAssetLinks>();
        assetLinksService
            .Setup(x => x.Delete(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var vm = AssetDetailsViewModelFactory.Create(
            asset,
            assetLinksService: assetLinksService.Object);

        var link = new PopulatedAssetLink
        {
            Asset = asset,
            AssetEntityId = asset.Id,
            LinkedAsset = linkedAssetToRemove,
            LinkedContentId = linkedAssetToRemove.Id
        };
        vm.LinkedAssets.Add(link);

        // Act
        vm.RemoveAssetLinkCommand.Execute(link);

        // Assert
        await ((AsyncRelayCommand<PopulatedAssetLink>)vm.RemoveAssetLinkCommand).CommandTask!;
        assetLinksService.Verify(x => x.Delete(asset.Id, linkedAssetToRemove.Id), Times.Once);
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

    [Fact]
    public async Task SetPageCommand_WithPageNumber_SetsCurrentPageAndLoadsAssetChanges()
    {
        // Arrange
        var asset = new AssetDtoBuilder().Build();

        var translations = new List<TranslationDto>
        {
            new TranslationDtoBuilder().WithAssetId(asset.Id).WithLanguageCode("en").Build(),
            new TranslationDtoBuilder().WithAssetId(asset.Id).WithLanguageCode("fr").Build(),
            new TranslationDtoBuilder().WithAssetId(asset.Id).WithLanguageCode("es").Build()
        };

        var changes = new List<ChangeLogEntry>
        {
            new ChangeLogEntryBuilder().WithResourceId(asset.Id).Build(),
            new ChangeLogEntryBuilder().WithResourceId(asset.Id).Build(),
            new ChangeLogEntryBuilder().WithResourceId(asset.Id).Build(),
            new ChangeLogEntryBuilder().WithResourceId(translations[0].Id).Build(),
            new ChangeLogEntryBuilder().WithResourceId(translations[1].Id).Build(),
            new ChangeLogEntryBuilder().WithResourceId(translations[2].Id).Build()
        };

        var translationsService = new Mock<ITranslations>();
        translationsService.Setup(x => x.GetAllForAsset(asset.Id)).ReturnsAsync(translations);

        var changeLogService = new Mock<IChangeLog>();
        changeLogService.Setup(x => x.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<Guid>>()))
            .ReturnsAsync(new PaginatedResponseBuilder<ChangeLogEntry>(changes).WithTotalPages(3).Build());


        var vm = AssetDetailsViewModelFactory.Create(asset,
            translationsService: translationsService.Object,
            changeLogService: changeLogService.Object);

        // Act
        vm.SetPageCommand.Execute(2);

        // Assert
        await ((AsyncRelayCommand<int>)vm.SetPageCommand).CommandTask!;

        Assert.Equal(2, vm.CurrentPage);
        Assert.Equal(6, vm.ChangeLogEntries.Count);
        changeLogService.Verify(x => x.Get(2, It.IsAny<int>(), It.IsAny<List<Guid>>()), Times.Once);
    }
}