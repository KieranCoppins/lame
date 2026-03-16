using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Lame.Backend.Exports;
using Lame.Backend.Languages;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels.Exports;
using Microsoft.Win32;

namespace Lame.Frontend.ViewModels;

public class ExportViewModel : PageViewModel
{
    private readonly IExports _exportsService;
    private readonly ILanguages _languagesService;
    private readonly INotificationService _notificationService;

    public ExportViewModel(INotificationService notificationService,
        ILanguages languagesService,
        IExports exportsService)
    {
        _notificationService = notificationService;
        _languagesService = languagesService;
        _exportsService = exportsService;

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

    public override async Task OnNavigatedTo()
    {
        await base.OnNavigatedTo();
        await LoadAvailableLanguages();
    }

    // TODO Abstract file saving logic to a service and add unit tests for export logic
    private async Task Export()
    {
        try
        {
            if (ExportFormatView == null) throw new NullReferenceException("No export format selected");

            var options = ExportFormatView.GetExportOptions();

            // Get a save destination
            var dialog = new SaveFileDialog
            {
                Filter = options.Format switch
                {
                    ExportFormatType.XLIFF => "XLIFF files (*.xliff)|*.xliff",
                    ExportFormatType.JSON => "JSON files (*.json)|*.json",
                    _ => "All files (*.*)|*.*"
                },

                FileName = "export",
                Title = "Select Export Destination"
            };

            var result = dialog.ShowDialog();
            if (result == false) return;

            var filePath = dialog.FileName;
            var fileData = await _exportsService.Export(options);

            await File.WriteAllBytesAsync(filePath, fileData);

            _notificationService.EmitNotification(new Notification
            {
                Title = "Export Complete",
                Message = "File exported successfully",
                Type = NotificationType.Success
            });
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
    }

    private async Task LoadAvailableLanguages()
    {
        var result = await _languagesService.Get();
        AvailableLanguages.Clear();
        foreach (var language in result) AvailableLanguages.Add(language);
    }
}