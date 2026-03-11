using Lame.DomainModel;

namespace Lame.Frontend.ViewModels.Exports;

public class ExportJsonViewModel : BaseViewModel, IExportOptionsViewModel
{
    public ExportJsonViewModel(ExportViewModel exportViewModel)
    {
        ExportViewModel = exportViewModel;
    }

    public Language? Language
    {
        get;
        set => SetField(ref field, value);
    }

    public ExportViewModel ExportViewModel { get; }

    public ExportOptions GetExportOptions()
    {
        if (Language == null) throw new NullReferenceException("Language not specified");

        var options = new ExportOptions
        {
            Format = ExportFormatType.JSON,
            LanguageCode = Language.LanguageCode,
            TranslationStatusFilter = ExportTranslationStatusFilter.Complete
        };

        return options;
    }
}