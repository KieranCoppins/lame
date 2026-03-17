using System.Collections.ObjectModel;
using System.Windows.Input;
using Lame.Backend.Assets;
using Lame.Backend.Tags;
using Lame.Backend.Translations;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels.Dialogs;

namespace Lame.Frontend.ViewModels;

public class AssetDetailsViewModel : PageViewModel
{
    private readonly IAssets _assetsService;
    private readonly IDialogService _dialogService;

    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;
    private readonly ITags _tagsService;
    private readonly ITranslations _translationsService;

    public AssetDetailsViewModel(
        INavigationService navigationService,
        IServiceProvider serviceProvider,
        ITranslations translationsService,
        ITags tagsService,
        IAssets assetsService,
        IDialogService dialogService,
        INotificationService notificationService,
        AssetDto asset,
        int supportedLanguagesCount)
    {
        _navigationService = navigationService;
        _translationsService = translationsService;
        _tagsService = tagsService;
        _assetsService = assetsService;
        _dialogService = dialogService;
        _notificationService = notificationService;

        Translations = [];
        LinkedAssets = [];
        Tags = [];

        Asset = asset;
        SupportedLanguagesCount = supportedLanguagesCount;

        ReturnToLibraryCommand = new RelayCommand(() =>
            _navigationService.NavigateTo<AssetLibraryViewModel>());

        ViewLinkedAssetDetails = new RelayCommand<AssetDto>(linkedAsset =>
            _navigationService.NavigateTo<AssetDetailsViewModel>(linkedAsset, SupportedLanguagesCount));

        OpenLinkAssetDialogCommand = new RelayCommand(() =>
            _dialogService.ShowDialog<LinkAssetsDialogViewModel>(Asset, LinkToAsset)
        );

        RemoveAssetLinkCommand = new AsyncRelayCommand<AssetDto>(UnLinkAsset);

        EditTranslationCommand = new RelayCommand<TranslationDto>(translation =>
            _dialogService.ShowDialog<EditTranslationDialogViewModel>(Asset, translation));

        _dialogService.ActiveDialogChanged += DialogServiceOnActiveDialogChanged;

        ArchiveAssetCommand = new RelayCommand(() =>
        {
            _dialogService.ShowDialog<ArchiveAssetDialogViewModel>(Asset);
        });

        Page = AppPage.Library;
    }

    public int SupportedLanguagesCount { get; }

    public AssetDto Asset { get; }

    public ObservableCollection<TranslationDto> Translations { get; }
    public ObservableCollection<AssetDto> LinkedAssets { get; }

    public ObservableCollection<Tag> Tags
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand ReturnToLibraryCommand { get; }
    public ICommand ViewLinkedAssetDetails { get; }
    public ICommand OpenLinkAssetDialogCommand { get; }
    public ICommand RemoveAssetLinkCommand { get; }
    public ICommand EditTranslationCommand { get; }
    public ICommand ArchiveAssetCommand { get; }

    public Task? UpdateAssetTask { get; private set; }

    public string InternalName
    {
        get => Asset.InternalName;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            if (Asset.InternalName == value) return;

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

            Asset.ContextNotes = value;
            OnPropertyChanged();

            UpdateAssetTask = UpdateAsset();
        }
    }

    public Func<Task<List<Tag>>> GetTags => () => _tagsService.Get();

    private void DialogServiceOnActiveDialogChanged()
    {
        if (_dialogService.ActiveDialog == null)
            _ = OnNavigatedTo();
    }

    public override async Task OnNavigatedTo()
    {
        await base.OnNavigatedTo();
        await LoadAsset();
    }

    public override async Task OnNavigatedFrom()
    {
        await base.OnNavigatedFrom();
        _dialogService.ActiveDialogChanged -= DialogServiceOnActiveDialogChanged;
    }

    private async Task LoadAsset()
    {
        await Task.WhenAll(LoadTranslations(), LoadLinkedAssets(), LoadTags());
    }

    private async Task LoadTranslations()
    {
        var translations = await _translationsService.GetTargetedForAsset(Asset.Id);

        Translations.Clear();
        foreach (var translation in translations) Translations.Add(translation);
    }

    private async Task LoadLinkedAssets()
    {
        var linkedAssets = await _assetsService.GetLinkedAssets(Asset.Id);
        LinkedAssets.Clear();
        foreach (var linkedAsset in linkedAssets)
            LinkedAssets.Add(linkedAsset);
    }

    private async Task LoadTags()
    {
        var tags = await _tagsService.GetTagsForResource(Asset.Id);
        Tags.Clear();
        foreach (var tag in tags) Tags.Add(tag);
    }

    public async Task LinkToAsset(AssetDto asset)
    {
        try
        {
            await _assetsService.LinkAssets(Asset.Id, asset.Id);
            LinkedAssets.Add(asset);

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
            await _assetsService.UnLinkAssets(Asset.Id, asset.Id);
            LinkedAssets.Remove(asset);

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
}