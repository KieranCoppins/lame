using System.Globalization;
using System.Windows.Data;

namespace Lame.Frontend.Helpers.Converters;

public class PercentageConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 2 ||
            values[0] == null ||
            values[1] == null)
            return "0%";

        var value = System.Convert.ToDouble(values[0]);
        var total = System.Convert.ToDouble(values[1]);

        if (total == 0)
            return "0%";

        var percent = value / total * 100;
        return $"{percent:F2}%";
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}