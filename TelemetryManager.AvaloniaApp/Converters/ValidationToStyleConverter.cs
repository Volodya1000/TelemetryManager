using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace TelemetryManager.AvaloniaApp.Converters;

public class ValidationToStyleConverter : IValueConverter
{
    public static readonly ValidationToStyleConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool isValid) return null;

        string param = parameter?.ToString() ?? "Background";

        return param switch
        {
            "Background" => isValid
                ? new SolidColorBrush(Colors.LightGreen)
                : new SolidColorBrush(Colors.LightPink),

            "Foreground" => isValid
                ? Brushes.Green
                : Brushes.Red,

            "Text" => isValid
                ? "✓ Valid"
                : "⨉ Invalid",

            _ => null
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
