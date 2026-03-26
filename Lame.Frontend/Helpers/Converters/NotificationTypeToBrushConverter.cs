using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Lame.Frontend.Services;

namespace Lame.Frontend.Helpers.Converters;

public class NotificationTypeToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            NotificationType.Info => Application.Current.Resources["Brush.NotificationInfo"] as Brush,
            NotificationType.Success => Application.Current.Resources["Brush.NotificationSuccess"] as Brush,
            NotificationType.Warning => Application.Current.Resources["Brush.NotificationWarning"] as Brush,
            NotificationType.Failure => Application.Current.Resources["Brush.NotificationFailure"] as Brush,

            // An obvious colour to indicate an error in the converter itself
            _ => Brushes.Purple
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}