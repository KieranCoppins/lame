using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Lame.Frontend.Helpers.Converters;

public class BoolToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
            // An obvious colour to indicate an error in the converter itself
            return Brushes.Purple;

        if (boolValue) return Application.Current.Resources["Brush.NotificationSuccess"] as Brush;

        return Application.Current.Resources["Brush.NotificationFailure"] as Brush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}