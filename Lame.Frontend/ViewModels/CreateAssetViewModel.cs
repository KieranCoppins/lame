using System.ComponentModel.DataAnnotations;
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

        CreateAssetCommand = new AsyncRelayCommand(CreateAsset, () => !CreatingAsset);
        ClearFormCommand = new RelayCommand(ClearForm, () => !CreatingAsset);
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
        set
        {
            SetField(ref field, value);
            if (string.IsNullOrWhiteSpace(value))
                AddError("Content is required.");
            else
                ClearError();
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

    private void ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(InternalName))
            throw new ValidationException("Asset Name is required.");

        if (string.IsNullOrWhiteSpace(EnglishContent))
            throw new ValidationException("Content is required.");
    }

    private async Task CreateAsset()
    {
        CreatingAsset = true;

        try
        {
            ValidateForm();

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

            foreach (var tagName in tags)
            {
                var tagEntity = await GetTagByName(tagName);
                await _tags.AddTagToResource(tagEntity, assetId, ResourceType.Asset);
            }

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
        Tags = string.Empty;
        EnglishContent = string.Empty;
        ContextNotes = string.Empty;

        ClearError(nameof(InternalName));
        ClearError(nameof(EnglishContent));
    }

    private async Task<Tag> GetTagByName(string tagName)
    {
        var existingTags = await _tags.Get(tagName, 1);

        if (existingTags.Count == 0 || existingTags[0].Name != tagName)
            return new Tag { Id = Guid.NewGuid(), Name = tagName };

        return existingTags[0];
    }
}