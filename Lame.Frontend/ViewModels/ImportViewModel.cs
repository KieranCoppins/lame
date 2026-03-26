using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Lame.Backend.Imports;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels.Imports;

namespace Lame.Frontend.ViewModels;

public class ImportViewModel : PageViewModel
{
    private readonly IImports _importsService;
    private readonly INotificationService _notificationService;
    private readonly ISystemIO _systemIo;

    public ImportViewModel(
        INotificationService notificationService,
        ISystemIO systemIo,
        IImports importsService
    )
    {
        _notificationService = notificationService;
        _systemIo = systemIo;
        _importsService = importsService;
        Page = AppPage.ImportXliff;

        ImportCommand = new AsyncRelayCommand(Import);
    }

    public ICommand ImportCommand { get; }

    public string FilePath
    {
        get;
        set
        {
            if (!SetField(ref field, value)) return;
            var extension = Path.GetExtension(value);

            if (extension == ".xliff" || extension == ".xlif")
                Importer = new ImportXliffViewModel(_systemIo);
            else
                Importer = null;
        }
    }

    public bool ContainsMajorChanges
    {
        get;
        set => SetField(ref field, value);
    }

    public bool CreateMissingAssets
    {
        get;
        set => SetField(ref field, value);
    }

    public IImportViewModel? Importer
    {
        get;
        set => SetField(ref field, value);
    }

    private async Task Import()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(FilePath)) throw new Exception("Please select a file to import.");

            if (Importer == null) throw new Exception("Unsupported file type.");

            // First get the importer to extract the data from the file
            var importData = await Importer.GetImportData(FilePath);

            var importOptions = new ImportOptions
            {
                ImportData = importData,
                ContainsMajorChanges = ContainsMajorChanges,
                CreateMissingAssets = CreateMissingAssets
            };

            var createdTranslations = await _importsService.Import(importOptions);

            _notificationService.EmitNotification(
                new Notification
                {
                    Title = "Import successful",
                    Message = $"Successfully imported {createdTranslations} translations.",
                    Type = NotificationType.Success
                });
        }
        catch (Exception ex)
        {
            _notificationService.EmitNotification(
                new Notification
                {
                    Title = "Failed to import",
                    Message = ex.Message,
                    Type = NotificationType.Failure
                });
        }
    }
}