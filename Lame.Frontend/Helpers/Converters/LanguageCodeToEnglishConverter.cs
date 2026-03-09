using System.Globalization;
using System.Windows.Data;
using Panlingo.LanguageCode;
using Panlingo.LanguageCode.Models;

namespace Lame.Frontend.Helpers.Converters;

public class LanguageCodeToEnglishConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return null;

        var resolver = new LanguageCodeResolver().Select(LanguageCodeEntity.EnglishName);
        return LanguageCodeHelper.Resolve((string)value, resolver);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var resolver = new LanguageCodeResolver().Select(LanguageCodeEntity.Alpha2);
        return LanguageCodeHelper.Resolve((string)value, resolver);
    }
}