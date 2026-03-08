using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Lame.Frontend.Helpers.Converters;

public class ProgressPercentageToBrushConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || !(values[0] is double value) || !(values[1] is double max) || max == 0)
            return Application.Current.Resources["Brush.LighterGrey"] as Brush;

        var percent = value / max;

        if (percent >= 1.0)
            return Application.Current.Resources["Brush.NotificationSuccess"] as Brush;
        if (percent < 0.5)
            return Application.Current.Resources["Brush.NotificationFailure"] as Brush;

        return Application.Current.Resources["Brush.NotificationWarning"] as Brush;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}