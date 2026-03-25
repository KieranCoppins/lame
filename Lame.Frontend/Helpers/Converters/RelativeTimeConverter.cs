using System.Globalization;
using System.Windows.Data;

namespace Lame.Frontend.Helpers.Converters;

public class RelativeTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return string.Empty;

        DateTime dateTime;
        if (value is DateTime dt)
            dateTime = dt;
        else if (value is DateTimeOffset dto)
            dateTime = dto.DateTime;
        else
            return string.Empty;

        var ts = DateTime.UtcNow - dateTime;

        if (ts.TotalSeconds < 60)
            return $"{(int)ts.TotalSeconds} second{((int)ts.TotalSeconds > 1 ? "s" : "")} ago";
        if (ts.TotalMinutes < 60)
            return $"{(int)ts.TotalMinutes} minute{((int)ts.TotalMinutes > 1 ? "s" : "")} ago";
        if (ts.TotalHours < 24)
            return $"{(int)ts.TotalHours} hour{((int)ts.TotalHours > 1 ? "s" : "")} ago";
        if (ts.TotalDays < 7)
            return $"{(int)ts.TotalDays} day{((int)ts.TotalDays > 1 ? "s" : "")} ago";
        if (ts.TotalDays < 30)
            return $"{(int)(ts.TotalDays / 7)} week{((int)(ts.TotalDays / 7) > 1 ? "s" : "")} ago";
        if (ts.TotalDays < 365)
            return $"{(int)(ts.TotalDays / 30)} month{((int)(ts.TotalDays / 30) > 1 ? "s" : "")} ago";
        return $"{(int)(ts.TotalDays / 365)} year{((int)(ts.TotalDays / 365) > 1 ? "s" : "")} ago";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}