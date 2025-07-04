using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace TelemetryManager.AvaloniaApp.Converters;

public class IsNotFirstPageConverter : IValueConverter
{
    public static readonly IsNotFirstPageConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is int currentPage && currentPage > 1;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
