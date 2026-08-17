using System;
using System.Globalization;
using System.Windows.Data;
using FrontlineCardWarfare.Data;

namespace FrontlineCardWarfare.Converters;

/// <summary>
/// Конвертирует тип карты в человекочитаемое название на русском языке.
/// </summary>
public class CardTypeToDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is CardType cardType)
        {
            return cardType switch
            {
                CardType.Melee => "Ближний бой",
                CardType.Ranged => "Дальний бой",
                CardType.Siege => "Осадная",
                CardType.Support => "Поддержка",
                CardType.Special => "Особенная",
                _ => "Неизвестно"
            };
        }

        if (value is string cardTypeStr)
        {
            return cardTypeStr.ToLower() switch
            {
                "melee" => "Ближний бой",
                "ranged" => "Дальний бой",
                "siege" => "Осадная",
                "support" => "Поддержка",
                "special" => "Особенная",
                _ => "Неизвестно"
            };
        }

        return "Неизвестно";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
