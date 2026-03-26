using System.Globalization;
using System.Windows.Data;

namespace Lame.Frontend.Helpers.Converters;

public class TabToSpaceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string s) return null;

        return s.Replace("\t", "    ");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}