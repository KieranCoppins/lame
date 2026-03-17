using System.Collections.ObjectModel;
using System.Windows.Input;
using Lame.Backend.FileStorage;
using Lame.Backend.Translations;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Helpers;
using Lame.Frontend.Services;

namespace Lame.Frontend.ViewModels.Dialogs;

public class EditTranslationDialogViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IFileStorage _fileStorageService;
    private readonly INotificationService _notificationService;
    private readonly ITranslations _translationsService;

    public EditTranslationDialogViewModel(
        AssetDto owningAsset,
        TranslationDto translation,
        IDialogService dialogService,
        ITranslations translationsService,
        INotificationService notificationService,
        IFileStorage fileStorageService)
    {
        OwningAsset = owningAsset;
        _dialogService = dialogService;
        _translationsService = translationsService;
        _notificationService = notificationService;
        _fileStorageService = fileStorageService;
        Translation = translation;
        SelectedTranslation = translation;
        Content = translation.Content;

        Translations = [];

        CancelCommand = new RelayCommand(_dialogService.CloseDialog);
        SaveChangesCommand = new AsyncRelayCommand(SaveChanges);

        TranslationVersionTask = LoadTranslationVersions();
    }

    public AssetDto OwningAsset { get; }

    public Task TranslationVersionTask { get; }

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

    public TranslationDto SelectedTranslation
    {
        get;
        set
        {
            if (!SetField(ref field, value)) return;

            Content = value.Content;
        }
    }

    public ObservableCollection<TranslationDto> Translations
    {
        get;
        set => SetField(ref field, value);
    }

    private async Task LoadTranslationVersions()
    {
        var versions = await _translationsService.GetAllForLanguageForAsset(Translation.AssetId, Translation.Language);

        Translations.Clear();
        foreach (var version in versions)
            if (version.Id == SelectedTranslation.Id)
                Translations.Add(SelectedTranslation);
            else
                Translations.Add(version);
    }

    private async Task SaveChanges()
    {
        IsSaving = true;

        try
        {
            if (Translation.Content == Content)
            {
                // Nothing has changed, just close the dialog
                _dialogService.CloseDialog();
                return;
            }

            if (SelectedTranslation.Content == Content)
            {
                // We have changed our version but no content change, just change our target version
                await _translationsService.SetTargetTranslation(SelectedTranslation.Id);

                _notificationService.EmitNotification(
                    new Notification
                    {
                        Title = "Changed translation version",
                        Message =
                            $"Successfully changed translation version to {Translation.MajorVersion}.{Translation.MinorVersion}.",
                        Type = NotificationType.Success
                    });

                _dialogService.CloseDialog();
                return;
            }

            // Otherwise we have new content and need to create a new translation version
            Translation.Content = Content;
            if (Translation.Status == TranslationStatus.Missing ||
                (Translation.Status == TranslationStatus.Outdated && HasMajorChanges))
            {
                // Set translation major version to current english version
                var englishTranslation =
                    (await _translationsService.GetTargetedForAsset(Translation.AssetId))
                    .FirstOrDefault(t => t.Language == "en");

                if (englishTranslation == null)
                    throw new NullReferenceException("English translation not found for asset.");

                Translation.MajorVersion = englishTranslation.MajorVersion;

                // Check if we have other major versions for this asset of the above major version, we need to get the latest minor and increment it
                Translation.MinorVersion = Translations
                    .Where(t => t.MajorVersion == Translation.MajorVersion)
                    .OrderByDescending(t => t.MinorVersion)
                    .FirstOrDefault()?.MinorVersion ?? -1;

                Translation.MinorVersion++;
            }
            else if (Translation.Status == TranslationStatus.UpToDate && HasMajorChanges)
            {
                // Because up to date does not always mean the latest major version, we need to get the latest major version and increment it
                Translation.MajorVersion = Translations
                    .OrderByDescending(t => t.MajorVersion)
                    .FirstOrDefault()?.MajorVersion ?? 0;

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

            await TranslationHelpers.CreateTranslation(
                _translationsService,
                _fileStorageService,
                OwningAsset.AssetType,
                Translation
            );

            _notificationService.EmitNotification(
                new Notification
                {
                    Title = "Created new translation",
                    Message =
                        $"Successfully created translation version {Translation.MajorVersion}.{Translation.MinorVersion}.",
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