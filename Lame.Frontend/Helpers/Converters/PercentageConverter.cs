using System;
using System.Globalization;
using System.Windows.Data;

namespace Lame.Frontend.Helpers.Converters;

public class PercentageConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 ||
            !double.TryParse(values[0]?.ToString(), out var parsedValue) ||
            !double.TryParse(values[1]?.ToString(), out var total))
            return 0;

        if (total <= 0) return 0;

        return parsedValue / total * 100;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}