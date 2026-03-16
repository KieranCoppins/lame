using System.Collections.ObjectModel;
using System.Windows.Input;
using Lame.Backend.Languages;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels.Dialogs;

namespace Lame.Frontend.ViewModels;

public class SettingsViewModel : PageViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILanguages _languagesService;

    public SettingsViewModel(IDialogService dialogService,
        ILanguages languagesService)
    {
        _dialogService = dialogService;
        _languagesService = languagesService;

        Page = AppPage.Settings;
        SupportedLanguages = [];

        OpenAddLanguageDialogCommand =
            new RelayCommand(() => _dialogService.ShowDialog<AddSupportedLanguageDialogViewModel>());
    }

    public ICommand OpenAddLanguageDialogCommand { get; }

    public ObservableCollection<Language> SupportedLanguages { get; }

    public override async Task OnNavigatedTo()
    {
        await base.OnNavigatedTo();
        _dialogService.ActiveDialogChanged += OnDialogChanged;
        await LoadLanguages();
    }

    public override async Task OnNavigatedFrom()
    {
        await base.OnNavigatedFrom();
        _dialogService.ActiveDialogChanged -= OnDialogChanged;
    }

    private async Task LoadLanguages()
    {
        var languages = await _languagesService.Get();
        SupportedLanguages.Clear();
        foreach (var language in languages) SupportedLanguages.Add(language);
    }

    private void OnDialogChanged()
    {
        if (_dialogService.ActiveDialog == null) _ = LoadLanguages();
    }
}