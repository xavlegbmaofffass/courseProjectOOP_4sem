using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FrontlineCardWarfare.Converters;

/// <summary>
/// Конвертирует непустую строку в Visibility.
/// Пустая строка -> Collapsed, непустая -> Visible.
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            bool isEmpty = string.IsNullOrWhiteSpace(str);

            // Если есть параметр "Inverse", инвертируем результат
            if (parameter is string param && param == "Inverse")
            {
                // Пустая строка -> Visible (показать подсказку)
                // Непустая строка -> Collapsed (скрыть подсказку)
                return isEmpty ? Visibility.Visible : Visibility.Collapsed;
            }

            return isEmpty ? Visibility.Collapsed : Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
