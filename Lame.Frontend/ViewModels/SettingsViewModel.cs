using System.Collections.ObjectModel;
using System.Windows.Input;
using Lame.Backend.Languages;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels.Dialogs;

namespace Lame.Frontend.ViewModels;

public class SettingsViewModel : PageViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILanguages _languagesService;
    private readonly IServiceProvider _serviceProvider;

    public SettingsViewModel(IDialogService dialogService, IServiceProvider serviceProvider,
        ILanguages languagesService)
    {
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;
        _languagesService = languagesService;

        Page = AppPage.Settings;
        SupportedLanguages = [];

        OpenAddLanguageDialogCommand =
            new RelayCommand(() => _dialogService.ShowDialog<AddSupportedLanguageDialogViewModel>());
    }

    public ICommand OpenAddLanguageDialogCommand { get; }

    public ObservableCollection<LanguageViewModel> SupportedLanguages { get; }

    public override void OnNavigatedTo()
    {
        base.OnNavigatedTo();

        _ = LoadLanguages();
        _dialogService.ActiveDialogChanged += OnDialogChanged;
    }

    public override void OnNavigatedFrom()
    {
        base.OnNavigatedFrom();

        _dialogService.ActiveDialogChanged -= OnDialogChanged;
    }

    private async Task LoadLanguages()
    {
        var languages = await _languagesService.Get();
        SupportedLanguages.Clear();
        foreach (var language in languages) SupportedLanguages.Add(new LanguageViewModel(language));
    }

    private void OnDialogChanged()
    {
        if (_dialogService.ActiveDialog == null) _ = LoadLanguages();
    }
}