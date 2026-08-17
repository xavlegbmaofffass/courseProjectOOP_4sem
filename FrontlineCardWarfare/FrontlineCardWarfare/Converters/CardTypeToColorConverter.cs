using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using FrontlineCardWarfare.Data;

namespace FrontlineCardWarfare.Converters;

/// <summary>
/// Конвертирует тип карты в цвет рамки для визуального отображения.
/// Поддерживает как enum CardType, так и строковые значения.
/// </summary>
public class CardTypeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Обработка enum CardType
        if (value is CardType cardType)
        {
            return cardType switch
            {
                CardType.Melee => new SolidColorBrush(Color.FromRgb(255, 100, 100)),
                CardType.Ranged => new SolidColorBrush(Color.FromRgb(100, 255, 100)),
                CardType.Siege => new SolidColorBrush(Color.FromRgb(100, 100, 255)),
                CardType.Support => new SolidColorBrush(Color.FromRgb(255, 255, 100)),
                CardType.Special => new SolidColorBrush(Color.FromRgb(255, 100, 255)),
                _ => new SolidColorBrush(Color.FromRgb(88, 91, 112))
            };
        }

        // Обработка строкового значения
        if (value is string cardTypeStr)
        {
            return cardTypeStr.ToLower() switch
            {
                "melee" => new SolidColorBrush(Color.FromRgb(255, 100, 100)),
                "ranged" => new SolidColorBrush(Color.FromRgb(100, 255, 100)),
                "siege" => new SolidColorBrush(Color.FromRgb(100, 100, 255)),
                "support" => new SolidColorBrush(Color.FromRgb(255, 255, 100)),
                "special" => new SolidColorBrush(Color.FromRgb(255, 100, 255)),
                _ => new SolidColorBrush(Color.FromRgb(88, 91, 112))
            };
        }

        return new SolidColorBrush(Color.FromRgb(88, 91, 112));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
