using System.Collections.ObjectModel;
using System.Windows.Input;
using Lame.Backend.Languages;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels.Exports;

namespace Lame.Frontend.ViewModels;

public class ExportViewModel : PageViewModel
{
    private readonly ILanguages _languagesService;
    private readonly INotificationService _notificationService;

    public ExportViewModel(INotificationService notificationService, ILanguages languagesService)
    {
        _notificationService = notificationService;
        _languagesService = languagesService;

        Page = AppPage.ExportXliff;

        SetSelectedExportFormatCommand = new RelayCommand<ExportFormatType>(exportType =>
        {
            SelectedExportFormat = exportType;
        });

        ExportCommand = new AsyncRelayCommand(Export);

        AvailableLanguages = [];

        SelectedExportFormat = ExportFormatType.XLIFF;
    }

    public IExportOptionsViewModel? ExportFormatView
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand SetSelectedExportFormatCommand { get; }
    public ICommand ExportCommand { get; }

    public ObservableCollection<Language> AvailableLanguages
    {
        get;
        private set => SetField(ref field, value);
    }

    public ExportFormatType SelectedExportFormat
    {
        get;
        set
        {
            SetField(ref field, value);

            ExportFormatView = SelectedExportFormat switch
            {
                ExportFormatType.XLIFF => new ExportXliffViewModel(this),
                ExportFormatType.JSON => new ExportJsonViewModel(this),
                _ => null
            };
        }
    }

    public override void OnNavigatedTo()
    {
        base.OnNavigatedTo();
        _ = LoadAvailableLanguages();
    }

    private Task Export()
    {
        try
        {
            if (ExportFormatView == null) throw new NullReferenceException("No export format selected");

            var options = ExportFormatView.GetExportOptions();
        }
        catch (Exception e)
        {
            _notificationService.EmitNotification(new Notification
            {
                Message = e.Message,
                Type = NotificationType.Failure,
                Title = "Export Failed"
            });
        }

        return Task.CompletedTask;
    }

    private async Task LoadAvailableLanguages()
    {
        var result = await _languagesService.Get();
        AvailableLanguages.Clear();
        foreach (var language in result) AvailableLanguages.Add(language);
    }
}