using System;
using System.Globalization;
using System.Windows.Data;
using FrontlineCardWarfare.Data;

namespace FrontlineCardWarfare.Converters;

/// <summary>
/// Возвращает описание действия карты во время хода.
/// Если у карты есть способность (Ability) — показывает её.
/// Иначе — базовое действие в зависимости от типа карты.
/// </summary>
public class CardActionDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Card card)
            return "Неизвестная карта";

        // Показываем способность только для Support и Special
        bool canHaveAbilities = card.CardType == CardType.Support || card.CardType == CardType.Special;
        if (canHaveAbilities && !string.IsNullOrWhiteSpace(card.Ability))
            return card.Ability;

        return card.CardType switch
        {
            CardType.Melee    => "Атакует врага в ближнем бою",
            CardType.Ranged   => "Стреляет по врагу на дистанции",
            CardType.Siege    => "Наносит урон осадным орудием",
            CardType.Support  => "Поддерживает союзников",
            CardType.Special  => "Особое тактическое действие",
            _                 => "Атака по врагу"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
