using System.Windows;
using System.Windows.Controls;

namespace Lame.Frontend.Helpers.TemplateSelectors;

public class OptionalTemplateSelector : DataTemplateSelector
{
    public DataTemplate DefaultTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item == null) return base.SelectTemplate(item, container);

        // If item is a string and no template is explicitly provided
        if (item is string) return DefaultTemplate;

        // Otherwise, assume template is provided and handled by ContentPresenter
        return null;
    }
}