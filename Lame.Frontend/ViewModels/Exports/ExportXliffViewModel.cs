using System;
using System.Windows.Input;
using Lame.DomainModel;
using Lame.Frontend.Commands;

namespace Lame.Frontend.ViewModels.Exports;

public class ExportXliffViewModel : BaseViewModel, IExportOptionsViewModel
{
    public ExportXliffViewModel(ExportViewModel exportViewModel)
    {
        ExportViewModel = exportViewModel;

        SetTranslationStatusFilterCommand = new RelayCommand<ExportTranslationStatusFilter>(filter =>
        {
            TranslationStatusFilter = filter;
        });
    }

    public ICommand SetTranslationStatusFilterCommand { get; }

    public ExportViewModel ExportViewModel { get; }

    public Language? TargetLanguage
    {
        get;
        set => SetField(ref field, value);
    }

    public ExportTranslationStatusFilter TranslationStatusFilter
    {
        get;
        set => SetField(ref field, value);
    }

    public ExportOptions GetExportOptions()
    {
        if (TargetLanguage == null) throw new NullReferenceException("Target language not specified");

        var options = new ExportOptions
        {
            Format = ExportFormatType.XLIFF,
            LanguageCode = TargetLanguage.LanguageCode,
            TranslationStatusFilter = TranslationStatusFilter
        };

        return options;
    }
}