using System.Windows.Input;
using Lame.Backend.Assets;
using Lame.Backend.Tags;
using Lame.Backend.Translations;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Services;

namespace Lame.Frontend.ViewModels;

public class CreateAssetViewModel : PageViewModel
{
    private readonly IAssets _assets;
    private readonly INotificationService _notificationService;
    private readonly ITags _tags;
    private readonly ITranslations _translations;

    public CreateAssetViewModel(IAssets assets, ITranslations translations, ITags tags,
        INotificationService notificationService)
    {
        _assets = assets;
        _translations = translations;
        _tags = tags;
        _notificationService = notificationService;

        Page = AppPage.CreateAsset;

        CreateAssetCommand = new RelayCommand(CreateAsset);
        ClearFormCommand = new RelayCommand(ClearForm);
    }

    public Array AssetTypes => Enum.GetValues<AssetType>();

    public string InternalName
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;


    public AssetType SelectedAssetType
    {
        get;
        set => SetField(ref field, value);
    } = AssetType.Text;

    public string Tags
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string EnglishContent
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string ContextNotes
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public ICommand CreateAssetCommand { get; }
    public ICommand ClearFormCommand { get; }

    private async void CreateAsset()
    {
        if (!ValidateForm()) return;

        try
        {
            var assetId = Guid.NewGuid();
            await _assets.Create(new Asset
            {
                Id = assetId,
                InternalName = InternalName,
                AssetType = SelectedAssetType,
                ContextNotes = ContextNotes
            });

            await _translations.Create(new Translation
            {
                Id = Guid.NewGuid(),
                AssetId = assetId,
                Language = "en",
                Content = EnglishContent,
                MajorVersion = 1,
                MinorVersion = 0
            });

            var tags = Tags.Split(',')
                .Select(tag => tag.Trim().ToLower())
                .Where(tag => !string.IsNullOrEmpty(tag))
                .Distinct()
                .ToList();

            Task.WaitAll(
                tags
                    .Select(async tag => _tags.AddTagToResource(await GetTagByName(tag), assetId, ResourceType.Asset))
                    .ToArray()
            );

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
    }

    private void ClearForm()
    {
        InternalName = string.Empty;
        SelectedAssetType = AssetType.Text;
        Tags = string.Empty;
        EnglishContent = string.Empty;
        ContextNotes = string.Empty;
    }

    private async Task<Tag> GetTagByName(string tagName)
    {
        var existingTags = await _tags.Get(tagName, 1);

        if (existingTags.Count == 0 || existingTags[0].Name != tagName)
            return new Tag { Id = Guid.NewGuid(), Name = tagName };

        return existingTags[0];
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(InternalName))
        {
            _notificationService.EmitNotification(
                new Notification
                {
                    Title = "Validation error",
                    Message = "Internal name is required.",
                    Type = NotificationType.Warning
                }
            );
            return false;
        }

        if (string.IsNullOrWhiteSpace(EnglishContent))
        {
            _notificationService.EmitNotification(
                new Notification
                {
                    Title = "Validation error",
                    Message = "English content is required.",
                    Type = NotificationType.Warning
                }
            );
            return false;
        }

        return true;
    }
}