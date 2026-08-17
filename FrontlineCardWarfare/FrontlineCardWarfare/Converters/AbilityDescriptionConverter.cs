using System;
using System.Globalization;
using System.Windows.Data;

namespace FrontlineCardWarfare.Converters;

/// <summary>
/// Конвертирует способность карты в описание боевого действия.
/// Если способность отсутствует, возвращает стандартное описание атаки.
/// </summary>
public class AbilityDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string ability && !string.IsNullOrWhiteSpace(ability))
            return ability;

        return "Обычная атака по врагу в радиусе действия";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
