using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
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
using Lame.Frontend.ViewModels.Dialogs;
using Microsoft.Win32;

namespace Lame.Frontend.ViewModels;

public class AssetDetailsViewModel : BaseViewModel
{
    private readonly IAssetLinks _assetLinksService;
    private readonly IAssets _assetsService;
    private readonly IChangeLog _changeLogService;
    private readonly IDialogService _dialogService;
    private readonly IFileStorage _fileStorageService;
    private readonly ILanguages _languagesService;

    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;
    private readonly ISystemIO _systemIo;
    private readonly ITags _tagsService;
    private readonly ITranslations _translationsService;

    private readonly List<string> Changes;

    public AssetDetailsViewModel(
        INavigationService navigationService,
        ITranslations translationsService,
        ITags tagsService,
        IAssets assetsService,
        IDialogService dialogService,
        INotificationService notificationService,
        IFileStorage fileStorageService,
        ISystemIO systemIo,
        IAssetLinks assetLinksService,
        ILanguages languagesService,
        IChangeLog changeLogService,
        AssetDto asset)
    {
        _navigationService = navigationService;
        _translationsService = translationsService;
        _tagsService = tagsService;
        _assetsService = assetsService;
        _dialogService = dialogService;
        _notificationService = notificationService;
        _fileStorageService = fileStorageService;
        _systemIo = systemIo;
        _assetLinksService = assetLinksService;
        _languagesService = languagesService;
        _changeLogService = changeLogService;

        Translations = [];
        LinkedAssets = [];
        Tags = [];
        Changes = [];

        Asset = asset;

        ViewLinkedAssetDetails = new RelayCommand<PopulatedAssetLink>(link =>
            _navigationService.NavigateTo<AssetLibraryDetailsViewModel>(link.LinkedAsset));

        OpenLinkAssetDialogCommand = new RelayCommand(() =>
            _dialogService.ShowDialog<LinkAssetsDialogViewModel>(Asset, LinkToAsset)
        );

        RemoveAssetLinkCommand = new AsyncRelayCommand<PopulatedAssetLink>(x => UnLinkAsset(x.LinkedAsset));
        ReviewAssetLinkCommand = new RelayCommand<PopulatedAssetLink>(link =>
            _navigationService.NavigateTo<AssetLinkReviewViewModel>(link));

        EditTranslationCommand = new RelayCommand<TranslationDto>(translation =>
            _dialogService.ShowDialog<EditTranslationDialogViewModel>(Asset, translation));

        _dialogService.ActiveDialogChanged += DialogServiceOnActiveDialogChanged;

        DownloadTranslationCommand = new AsyncRelayCommand<TranslationDto>(DownloadTranslation);
    }

    public int SupportedLanguagesCount
    {
        get;
        set => SetField(ref field, value);
    }

    public AssetDto Asset
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableCollection<TranslationDto> Translations { get; }
    public ObservableCollection<PopulatedAssetLink> LinkedAssets { get; }

    public ObservableCollection<Tag> Tags
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand ViewLinkedAssetDetails { get; }
    public ICommand OpenLinkAssetDialogCommand { get; }
    public ICommand RemoveAssetLinkCommand { get; }
    public ICommand ReviewAssetLinkCommand { get; }
    public ICommand EditTranslationCommand { get; }
    public ICommand DownloadTranslationCommand { get; }

    public Task? UpdateAssetTask { get; private set; }

    public string InternalName
    {
        get => Asset.InternalName;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            if (Asset.InternalName == value) return;

            Changes.Add($"- Name updated '{Asset.InternalName}' -> '{value}'");

            Asset.InternalName = value;
            OnPropertyChanged();

            UpdateAssetTask = UpdateAsset();
        }
    }

    public string? ContextNotes
    {
        get => Asset.ContextNotes;
        set
        {
            if (Asset.ContextNotes == value) return;


            Changes.Add("- Context notes updated");

            Asset.ContextNotes = value;
            OnPropertyChanged();

            UpdateAssetTask = UpdateAsset();
        }
    }

    public Func<Task<List<Tag>>> GetTags => () => _tagsService.Get();

    ~AssetDetailsViewModel()
    {
        _dialogService.ActiveDialogChanged -= DialogServiceOnActiveDialogChanged;
    }

    private void DialogServiceOnActiveDialogChanged()
    {
        if (_dialogService.ActiveDialog == null)
            _ = LoadAsset();
    }

    public async Task LoadAsset()
    {
        // Refresh the state of the asset also
        Asset = await _assetsService.Get(Asset.Id);

        await Task.WhenAll(LoadTranslations(), LoadLinkedAssets(), LoadTags(), LoadSupportedLanguages());
    }

    private async Task LoadTranslations()
    {
        var translations = await _translationsService.GetTargetedForAsset(Asset.Id);

        Translations.Clear();
        foreach (var translation in translations) Translations.Add(translation);
    }

    private async Task LoadLinkedAssets()
    {
        var linkedAssetIds = Asset.AssetLinks
            .Select(x => x.LinkedContentId)
            .ToList();

        var linkedAssets = await _assetsService.Get(linkedAssetIds);

        LinkedAssets.Clear();
        foreach (var linkedAsset in linkedAssets)
        {
            // This should always be here as it's how we got the asset in the first place.
            var linkedAssetLink = Asset.AssetLinks.First(x => x.LinkedContentId == linkedAsset.Id);

            LinkedAssets.Add(new PopulatedAssetLink
            {
                Asset = Asset,
                LinkedAsset = linkedAsset,
                AssetEntityId = Asset.Id,
                LinkedContentId = linkedAsset.Id,
                Synced = linkedAssetLink.Synced
            });
        }
    }

    private async Task LoadTags()
    {
        var tags = await _tagsService.GetTagsForResource(Asset.Id);
        Tags.Clear();
        foreach (var tag in tags) Tags.Add(tag);
        OnPropertyChanged(nameof(Tags));
    }

    public async Task LinkToAsset(AssetDto asset)
    {
        try
        {
            var link = await _assetLinksService.Create(Asset.Id, asset.Id);

            Asset.AssetLinks.Add(link);
            LinkedAssets.Add(new PopulatedAssetLink
            {
                Asset = Asset,
                LinkedAsset = asset,
                AssetEntityId = Asset.Id,
                LinkedContentId = asset.Id,
                Synced = link.Synced
            });

            await _changeLogService.Create(new ChangeLogEntry
            {
                ResourceId = Asset.Id,
                ResourceAction = ResourceAction.Created,
                ResourceType = ResourceType.AssetLink,
                Message =
                    $"Asset '{Asset.InternalName}' linked to '{asset.InternalName}'"
            });

            _notificationService.EmitNotification(new Notification
            {
                Message =
                    $"Asset '{asset.InternalName}' successfully linked to '{Asset.InternalName}'",
                Type = NotificationType.Success,
                Title = "Assets linked"
            });
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
    }

    public async Task UnLinkAsset(AssetDto asset)
    {
        try
        {
            await _assetLinksService.Delete(Asset.Id, asset.Id);

            var populatedLink = LinkedAssets.First(x => x.LinkedContentId == asset.Id);
            LinkedAssets.Remove(populatedLink);

            var assetLink = Asset.AssetLinks.First(x => x.LinkedContentId == asset.Id);
            Asset.AssetLinks.Remove(assetLink);

            await _changeLogService.Create(new ChangeLogEntry
            {
                ResourceId = Asset.Id,
                ResourceAction = ResourceAction.Deleted,
                ResourceType = ResourceType.AssetLink,
                Message =
                    $"Asset '{Asset.InternalName}' unlinked to '{asset.InternalName}'"
            });

            _notificationService.EmitNotification(new Notification
            {
                Message =
                    $"Asset '{asset.InternalName}' successfully unlinked from '{Asset.InternalName}'",
                Type = NotificationType.Success,
                Title = "Assets unlinked"
            });
        }
        catch (Exception ex)
        {
            _notificationService.EmitNotification(new Notification
            {
                Message = ex.Message,
                Type = NotificationType.Failure,
                Title = "Error unlinking assets"
            });
        }
    }

    public async Task UpdateAsset()
    {
        try
        {
            // TODO: Consider using some auto mapper to map assets and asset dto and asset entities
            await _assetsService.Update(new Asset
            {
                Id = Asset.Id,
                AssetType = Asset.AssetType,
                ContextNotes = Asset.ContextNotes,
                InternalName = Asset.InternalName,
                LastUpdatedAt = Asset.LastUpdatedAt,
                CreatedAt = Asset.CreatedAt,
                Status = Asset.Status
            });

            var message = $"Asset '{Asset.InternalName}' updated";
            if (Changes.Count > 0)
                message += ":\n\t" + Changes.Aggregate((a, b) => a + "\n\t" + b);

            await _changeLogService.Create(new ChangeLogEntry
            {
                ResourceId = Asset.Id,
                ResourceAction = ResourceAction.Updated,
                ResourceType = ResourceType.Asset,
                Message = message
            });

            Changes.Clear();

            _notificationService.EmitNotification(
                new Notification
                {
                    Title = "Asset Updated",
                    Message = "The asset was successfully updated.",
                    Type = NotificationType.Success
                });
        }
        catch (Exception ex)
        {
            _notificationService.EmitNotification(
                new Notification
                {
                    Title = "Error updating asset",
                    Message = $"An error occurred while updating the asset: {ex.Message}",
                    Type = NotificationType.Failure
                });
        }
    }

    public async Task UpdateAssetTags()
    {
        try
        {
            var currentTags = await _tagsService.GetTagsForResource(Asset.Id);

            var tagsToAdd = Tags.Where(t => currentTags.All(ct => ct.Id != t.Id)).ToList();
            var tagsToRemove = currentTags.Where(ct => Tags.All(t => t.Id != ct.Id)).ToList();

            if (tagsToAdd.Count == 0 && tagsToRemove.Count == 0) return;

            var addTasks = tagsToAdd.Select(tag =>
                _tagsService.AddTagToResource(tag, Asset.Id, ResourceType.Asset));
            var removeTasks = tagsToRemove.Select(tag =>
                _tagsService.RemoveTagFromResource(tag.Id, Asset.Id));

            await Task.WhenAll(addTasks.Concat(removeTasks));

            var message = $"Asset '{Asset.InternalName}' tags changed:";
            if (tagsToAdd.Count == 1)
                message += $"\n\t+ {tagsToAdd.First().Name}";
            if (tagsToAdd.Count > 1)
                message += tagsToAdd.Select(t => t.Name).Aggregate((a, b) => "\n\t+ " + a + "\n\t+ " + b);

            if (tagsToRemove.Count == 1)
                message += $"\n\t+ {tagsToRemove.First().Name}";
            if (tagsToRemove.Count > 1)
                message += tagsToRemove.Select(t => t.Name).Aggregate((a, b) => "\n\t- " + a + "\n\t- " + b);


            await _changeLogService.Create(new ChangeLogEntry
            {
                ResourceId = Asset.Id,
                ResourceAction = ResourceAction.Updated,
                ResourceType = ResourceType.Asset,
                Message = message
            });

            _notificationService.EmitNotification(
                new Notification
                {
                    Title = "Asset Tags Updated",
                    Message = "The asset's tags were successfully updated.",
                    Type = NotificationType.Success
                });
        }
        catch (Exception ex)
        {
            _notificationService.EmitNotification(
                new Notification
                {
                    Title = "Error updating asset tags",
                    Message = $"An error occurred while updating the asset's tags: {ex.Message}",
                    Type = NotificationType.Failure
                });
        }
    }

    private async Task DownloadTranslation(TranslationDto translation)
    {
        try
        {
            var data = await _fileStorageService.Get(translation.Content);

            var extension = Path.GetExtension(translation.Content).ToLower();

            var dialog = new SaveFileDialog
            {
                Filter = "All files (*.*)|*.*",
                FileName =
                    $"{Asset.InternalName}_{translation.Language}_{translation.MajorVersion}-{translation.MinorVersion}{extension}",
                Title = "Select Download Destination"
            };

            var result = _systemIo.OpenSaveFileDialog(dialog);
            if (result == false) return;

            var filePath = dialog.FileName;

            await _systemIo.WriteAllBytesAsync(filePath, data);

            _notificationService.EmitNotification(new Notification
            {
                Title = "Download Complete",
                Message = "Translation file downloaded successfully",
                Type = NotificationType.Success
            });
        }
        catch (Exception ex)
        {
            _notificationService.EmitNotification(new Notification
            {
                Message = ex.Message,
                Type = NotificationType.Failure,
                Title = "Download Failed"
            });
        }
    }

    private async Task LoadSupportedLanguages()
    {
        var languages = await _languagesService.Get();
        SupportedLanguagesCount = languages.Count;
    }
}