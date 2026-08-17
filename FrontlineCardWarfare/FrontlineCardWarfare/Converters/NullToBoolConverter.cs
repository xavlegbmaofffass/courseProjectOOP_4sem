using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FrontlineCardWarfare.Converters;

/// <summary>
/// Конвертер, преобразующий null в false, а не-null в true.
/// Поддерживает параметр "Invert" для инверсии результата.
/// </summary>
public class NullToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isNull = value == null;
        
        if (parameter is string strParam && strParam.Equals("Invert", StringComparison.OrdinalIgnoreCase))
        {
            return isNull; // Инверсия: если null — true, иначе false
        }
        
        return !isNull; // По умолчанию: если есть значение (не null) — true, иначе false
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
