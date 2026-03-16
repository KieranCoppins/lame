using System.Collections;
using System.Windows.Input;
using Lame.Backend.Languages;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Services;
using Panlingo.LanguageCode;

namespace Lame.Frontend.ViewModels.Dialogs;

public class AddSupportedLanguageDialogViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILanguages _languagesService;
    private readonly INotificationService _notificationService;

    public AddSupportedLanguageDialogViewModel(
        IDialogService dialogService,
        ILanguages languagesService,
        INotificationService notificationService)
    {
        _dialogService = dialogService;
        _languagesService = languagesService;
        _notificationService = notificationService;

        SearchLanguages = searchTerm => Task.FromResult<IEnumerable>(ISOGeneratorResourceProvider.ISOGeneratorResources
            .LanguageDescriptorList
            .Where(ld =>
                !string.IsNullOrEmpty(ld.Part1) &&
                ld.RefName.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(ld =>
                ld.RefName.Equals(searchTerm, StringComparison.CurrentCultureIgnoreCase) ? 0 :
                ld.RefName.StartsWith(searchTerm, StringComparison.CurrentCultureIgnoreCase) ? 1 : 2)
            .ThenBy(ld => ld.RefName)
            .Take(10)
            .Select(ld => new LanguageViewModel(new Language { LanguageCode = ld.Part1 })));

        CancelCommand = new RelayCommand(_dialogService.CloseDialog);
        AddLanguageCommand = new AsyncRelayCommand(AddLanguage);
    }

    public LanguageViewModel? SelectedLanguage
    {
        get;
        set => SetField(ref field, value);
    }

    public Func<string, Task<IEnumerable>> SearchLanguages { get; set; }

    public ICommand AddLanguageCommand { get; }
    public ICommand CancelCommand { get; }

    public bool IsAddingLanguage
    {
        get;
        private set => SetField(ref field, value);
    }

    private async Task AddLanguage()
    {
        IsAddingLanguage = true;

        try
        {
            if (SelectedLanguage == null) throw new NullReferenceException("No language selected.");

            await _languagesService.RegisterLanguage(new Language { LanguageCode = SelectedLanguage.LanguageCode });

            _notificationService.EmitNotification(new Notification
            {
                Title = "Added language",
                Message = $"Successfully added {SelectedLanguage.Name} as a supported language.",
                Type = NotificationType.Success
            });

            _dialogService.CloseDialog();
        }
        catch (Exception ex)
        {
            _notificationService.EmitNotification(new Notification
            {
                Title = "Failed to add language",
                Message = ex.Message,
                Type = NotificationType.Failure
            });
        }
        finally
        {
            IsAddingLanguage = false;
        }
    }
}