using System.Globalization;
using System.Windows.Data;

namespace Lame.Frontend.Helpers.Converters;

public class VersionConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2 || !(values[0] is int major) || !(values[1] is int minor))
            return "N/A";

        return $"{major}.{minor}";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}