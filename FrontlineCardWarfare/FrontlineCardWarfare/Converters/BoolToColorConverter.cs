using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FrontlineCardWarfare.Converters;

/// <summary>
/// Конвертер bool в цвет для подсветки клеток.
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            var param = parameter?.ToString();
            
            if (boolValue)
            {
                // Доступная для перемещения - зелёный
                if (param == "Available" || param == "Success")
                    return new SolidColorBrush(Color.FromRgb(76, 175, 80));
                // Цель для атаки - красный
                if (param == "Attack")
                    return new SolidColorBrush(Color.FromRgb(244, 67, 54));
            }
            else
            {
                // Обычный цвет / блокировка
                if (param == "Success" || param == "Available")
                    return new SolidColorBrush(Color.FromRgb(128, 128, 128)); // Серый для заблокированного
                return new SolidColorBrush(Color.FromRgb(87, 107, 149));
            }
        }
        return new SolidColorBrush(Color.FromRgb(87, 107, 149));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
