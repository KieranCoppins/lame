using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Lame.Frontend.Helpers.Converters;

public class GuidToColourConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return null;

        var guid = (Guid)value;
        var hash = guid.GetHashCode();
        var colourIndex = Math.Abs(hash) % 10;

        // Return a colour based on the index
        return colourIndex switch
        {
            0 => Application.Current.Resources["Brush.Tag.A"] as Brush,
            1 => Application.Current.Resources["Brush.Tag.B"] as Brush,
            2 => Application.Current.Resources["Brush.Tag.C"] as Brush,
            3 => Application.Current.Resources["Brush.Tag.D"] as Brush,
            4 => Application.Current.Resources["Brush.Tag.E"] as Brush,
            5 => Application.Current.Resources["Brush.Tag.F"] as Brush,
            6 => Application.Current.Resources["Brush.Tag.G"] as Brush,
            7 => Application.Current.Resources["Brush.Tag.H"] as Brush,
            8 => Application.Current.Resources["Brush.Tag.I"] as Brush,
            9 => Application.Current.Resources["Brush.Tag.J"] as Brush
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}