using System.Globalization;
using System.Windows.Data;

namespace Lame.Frontend.Helpers.Converters;

public class PercentageConverter : IMultiValueConverter
{
    public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return "0%";

        if (!double.TryParse(values[0]?.ToString(), out var value) ||
            !double.TryParse(values[1]?.ToString(), out var total))
            return "0%";


        // Default to 2 decimal places if not specified
        var dp = values.Length < 3 ||
                 values[2] == null ||
                 !double.TryParse(values[2]?.ToString(), out var parsedDp)
            ? 2
            : parsedDp;

        if (total == 0)
            return "0%";

        var percent = value / total * 100;
        return percent.ToString($"F{dp}") + "%";
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}