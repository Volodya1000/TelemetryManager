using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace TelemetryManager.AvaloniaApp.Converters;

public class LessThanMultiConverter : IMultiValueConverter
{
    public static readonly LessThanMultiConverter Instance = new();

    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Count >= 2 && values[0] is int currentPage && values[1] is int totalPages)
        {
            return currentPage < totalPages;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
