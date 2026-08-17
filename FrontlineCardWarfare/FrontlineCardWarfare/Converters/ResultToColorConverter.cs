using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FrontlineCardWarfare.Converters;

/// <summary>
/// Конвертирует результат игры в цвет текста.
/// </summary>
public class ResultToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var result = value?.ToString()?.ToLowerInvariant() ?? string.Empty;

        if (result.Contains("win") || result.Contains("поб"))
            return new SolidColorBrush(Color.FromRgb(76, 175, 80));

        if (result.Contains("lose") || result.Contains("defeat") || result.Contains("пораж"))
            return new SolidColorBrush(Color.FromRgb(244, 67, 54));

        return new SolidColorBrush(Color.FromRgb(205, 214, 244));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
