using System.Windows.Controls;
using System.Windows.Data;
using Lame.DomainModel;

namespace Lame.Frontend.Views.Exports;

public partial class ExportXliffView : UserControl
{
    public ExportXliffView()
    {
        InitializeComponent();

        var cvs = (CollectionViewSource)Resources["FilteredLanguages"];
        cvs.Filter += (s, e) =>
        {
            if (e.Item is Language lang)
                e.Accepted = lang.LanguageCode != "en";
            else
                e.Accepted = false;
        };
    }
}