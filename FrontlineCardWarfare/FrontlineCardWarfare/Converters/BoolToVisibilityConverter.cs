using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FrontlineCardWarfare.Converters;

/// <summary>
/// Конвертирует boolean в Visibility.
/// True -> Visible, False -> Collapsed.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // Поддержка параметра инверсии
            if (parameter is string strParam && strParam.Equals("Invert", StringComparison.OrdinalIgnoreCase))
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }

        return false;
    }
}
