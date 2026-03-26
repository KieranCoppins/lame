using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
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
using Lame.Frontend.Enums;
using Lame.Frontend.Helpers;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels.Dialogs;

namespace Lame.Frontend.ViewModels;

public class CreateAssetViewModel : PageViewModel
{
    private readonly IAssetLinks _assetLinks;
    private readonly IAssets _assets;
    private readonly IChangeLog _changeLogService;
    private readonly IDialogService _dialogService;
    private readonly IFileStorage _fileStorageService;
    private readonly ILanguages _languagesService;
    private readonly INotificationService _notificationService;
    private readonly ISystemIO _systemIo;
    private readonly ITags _tags;
    private readonly ITranslations _translations;
    private LinkAssetsDialogViewModel _linkAssetsDialogViewModel;

    public CreateAssetViewModel(
        IAssets assets,
        ITranslations translations,
        ITags tags,
        IAssetLinks assetLinks,
        INotificationService notificationService,
        IDialogService dialogService,
        ILanguages languagesService,
        IFileStorage fileStorageService,
        ISystemIO systemIo,
        IChangeLog changeLogService)
    {
        _assets = assets;
        _translations = translations;
        _tags = tags;
        _assetLinks = assetLinks;
        _notificationService = notificationService;
        _dialogService = dialogService;
        _languagesService = languagesService;
        _fileStorageService = fileStorageService;
        _systemIo = systemIo;
        _changeLogService = changeLogService;

        Page = AppPage.CreateAsset;
        AssetsToLink = [];
        Tags = [];

        CreateAssetCommand = new AsyncRelayCommand(CreateAsset, () => !CreatingAsset);
        ClearFormCommand = new RelayCommand(ClearForm, () => !CreatingAsset);
        RemoveAssetLinkCommand = new RelayCommand<AssetDto>(linkedAsset => AssetsToLink.Remove(linkedAsset));

        OpenLinkAssetDialogCommand = new RelayCommand(() =>
            _dialogService.ShowDialog<LinkAssetsDialogViewModel>(LinkToAsset));
    }

    public Func<Task<List<Tag>>> TagSearch => () => _tags.Get();

    public ObservableCollection<AssetDto> AssetsToLink
    {
        get;
        set => SetField(ref field, value);
    }

    public Array AssetTypes => Enum.GetValues<AssetType>();

    public string InternalName
    {
        get;
        set
        {
            SetField(ref field, value);
            if (string.IsNullOrWhiteSpace(value))
                AddError("Asset Name is required.");
            else
                ClearError();
        }
    } = string.Empty;


    public AssetType SelectedAssetType
    {
        get;
        set
        {
            if (!SetField(ref field, value)) return;

            EnglishContent = string.Empty;
            ClearError(nameof(EnglishContent));
        }
    } = AssetType.Text;

    public ObservableCollection<Tag> Tags
    {
        get;
        set => SetField(ref field, value);
    }

    public string EnglishContent
    {
        get;
        set
        {
            SetField(ref field, value);

            if (string.IsNullOrWhiteSpace(value))
                AddError("Content is required.");
            else
                ClearError();

            if (SelectedAssetType == AssetType.Audio &&
                !string.IsNullOrWhiteSpace(value) &&
                string.IsNullOrWhiteSpace(InternalName))
                InternalName = Path.GetFileNameWithoutExtension(value);
        }
    } = string.Empty;

    public string ContextNotes
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public bool CreatingAsset
    {
        get;
        set
        {
            SetField(ref field, value);
            ((AsyncRelayCommand)CreateAssetCommand).RaiseCanExecuteChanged();
        }
    } = false;

    public ICommand CreateAssetCommand { get; }
    public ICommand ClearFormCommand { get; }
    public ICommand OpenLinkAssetDialogCommand { get; }
    public ICommand RemoveAssetLinkCommand { get; }

    public int SupportedLanguagesCount { get; set; }

    public override async Task OnNavigatedTo()
    {
        await base.OnNavigatedTo();
        await GetSupportedLanguagesCount();
    }

    private void ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(InternalName))
            throw new ValidationException("Asset Name is required.");

        if (string.IsNullOrWhiteSpace(EnglishContent))
            throw new ValidationException("Content is required.");
    }

    public async Task CreateAsset()
    {
        CreatingAsset = true;

        try
        {
            ValidateForm();

            // Create our asset
            var asset = new Asset
            {
                Id = Guid.NewGuid(),
                InternalName = InternalName,
                AssetType = SelectedAssetType,
                ContextNotes = ContextNotes
            };


            await _assets.Create(asset);

            // Create the english translation of the asset
            var translation = new Translation
            {
                Id = Guid.NewGuid(),
                AssetId = asset.Id,
                Language = "en",
                Content = EnglishContent,
                MajorVersion = 1,
                MinorVersion = 0
            };

            await TranslationHelpers.CreateTranslation(
                _translations,
                _fileStorageService,
                _systemIo,
                asset.AssetType,
                translation
            );

            // Tag our asset
            foreach (var tag in Tags) await _tags.AddTagToResource(tag, asset.Id, ResourceType.Asset);

            // Create asset links
            foreach (var linkedAsset in AssetsToLink) await _assetLinks.Create(linkedAsset.Id, asset.Id);

            // Create a log entry for asset creation
            await _changeLogService.Create(new ChangeLogEntry
            {
                ResourceId = asset.Id,
                ResourceAction = ResourceAction.Created,
                ResourceType = ResourceType.Asset,
                Message = $"New {asset.AssetType} asset '{asset.InternalName}' created."
            });

            _notificationService.EmitNotification(
                new Notification
                {
                    Title = "Asset created",
                    Message = $"Asset '{InternalName}' has been created successfully.",
                    Type = NotificationType.Success
                }
            );

            ClearForm();
        }
        catch (ValidationException ex)
        {
            _notificationService.EmitNotification(
                new Notification
                {
                    Title = "Validation error",
                    Message = ex.Message,
                    Type = NotificationType.Failure
                }
            );
        }
        catch (Exception ex)
        {
            _notificationService.EmitNotification(
                new Notification
                {
                    Title = "Error creating asset",
                    Message = $"An error occurred while creating the asset: {ex.Message}",
                    Type = NotificationType.Failure
                }
            );
        }
        finally
        {
            CreatingAsset = false;
        }
    }

    private void ClearForm()
    {
        InternalName = string.Empty;
        SelectedAssetType = AssetType.Text;
        EnglishContent = string.Empty;
        ContextNotes = string.Empty;
        AssetsToLink.Clear();
        Tags.Clear();

        ClearError(nameof(InternalName));
        ClearError(nameof(EnglishContent));
    }

    public Task LinkToAsset(AssetDto asset)
    {
        AssetsToLink.Add(asset);
        return Task.CompletedTask;
    }

    private async Task GetSupportedLanguagesCount()
    {
        var languages = await _languagesService.Get();
        SupportedLanguagesCount = languages.Count;
    }
}