using System;
using System.Globalization;
using System.Windows.Data;

namespace FrontlineCardWarfare.Converters;

/// <summary>
/// Конвертирует процент здоровья (0.0–1.0) в ширину полоски (0–90 px).
/// </summary>
public class HealthPercentToWidthConverter : IValueConverter
{
    /// <summary>
    /// Максимальная ширина полоски.
    /// </summary>
    public double MaxWidth { get; set; } = 90;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double percent)
        {
            return Math.Max(2, percent * MaxWidth);
        }
        return 2d;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
