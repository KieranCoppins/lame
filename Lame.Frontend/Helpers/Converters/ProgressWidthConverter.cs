using System;
using System.Globalization;
using System.Windows.Data;

namespace Lame.Frontend.Helpers.Converters;

public class ProgressWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 3) return 0d;

        var totalWidth = (double)values[0];
        var value = (double)values[1];
        var max = (double)values[2];

        if (max == 0) return 0d;

        return totalWidth * (value / max);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}