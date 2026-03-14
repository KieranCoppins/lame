using System.Windows.Input;
using Lame.Backend.Translations;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Services;

namespace Lame.Frontend.ViewModels.Dialogs;

public class EditTranslationDialogViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;
    private readonly ITranslations _translationsService;

    public EditTranslationDialogViewModel(
        TranslationDto translation,
        IDialogService dialogService,
        ITranslations translationsService,
        INotificationService notificationService)
    {
        _dialogService = dialogService;
        _translationsService = translationsService;
        _notificationService = notificationService;
        Translation = translation;
        Content = translation.Content;

        CancelCommand = new RelayCommand(_dialogService.CloseDialog);
        SaveChangesCommand = new AsyncRelayCommand(SaveChanges);
    }

    public TranslationDto Translation { get; }

    public string Content
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsSaving
    {
        get;
        set => SetField(ref field, value);
    }

    public bool HasMajorChanges
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand SaveChangesCommand { get; set; }
    public ICommand CancelCommand { get; set; }

    private async Task SaveChanges()
    {
        IsSaving = true;

        try
        {
            Translation.Content = Content;
            if (Translation.Status == TranslationStatus.Missing ||
                (Translation.Status == TranslationStatus.Outdated && HasMajorChanges))
            {
                // Set translation major version to current english version
                var englishTranslation =
                    (await _translationsService.GetForAsset(Translation.AssetId))
                    .FirstOrDefault(t => t.Language == "en");

                if (englishTranslation == null)
                    throw new NullReferenceException("English translation not found for asset.");

                Translation.MajorVersion = englishTranslation.MajorVersion;
                Translation.MinorVersion = 0;
            }
            else if (Translation.Status == TranslationStatus.UpToDate && HasMajorChanges)
            {
                Translation.MajorVersion++;
                Translation.MinorVersion = 0;
            }
            else if (!HasMajorChanges)
            {
                Translation.MinorVersion++;
            }

            // Always create new translations, never update existing ones, to allow version control
            Translation.Id = Guid.NewGuid();
            Translation.CreatedAt = DateTime.UtcNow;
            await _translationsService.Create(Translation);

            _notificationService.EmitNotification(
                new Notification
                {
                    Title = "Created new translation",
                    Message =
                        $"Successfully translation version {Translation.MajorVersion}.{Translation.MinorVersion}.",
                    Type = NotificationType.Success
                });

            _dialogService.CloseDialog();
        }
        catch (Exception ex)
        {
            _notificationService.EmitNotification(
                new Notification
                {
                    Title = "Failed to create new translation",
                    Message = "Failed to create new translation: " + ex.Message,
                    Type = NotificationType.Failure
                });
        }
        finally
        {
            IsSaving = false;
        }
    }
}