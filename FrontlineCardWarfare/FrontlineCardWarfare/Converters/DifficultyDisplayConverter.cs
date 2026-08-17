using System;
using System.Globalization;
using System.Windows.Data;

namespace FrontlineCardWarfare.Converters;

/// <summary>
/// Конвертирует внутренний код сложности в отображаемое описание.
/// </summary>
public class DifficultyDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var difficulty = value?.ToString()?.ToLowerInvariant();

        return difficulty switch
        {
            "easy" => "Противник совершает больше ошибок. Идеально для изучения механик игры.",
            "medium" => "Сбалансированный оппонент. Стандартный уровень сложности для большинства игроков.",
            "hard" => "Высокая агрессия и минимум ошибок. Требует глубокого понимания тактики.",
            _ => "Сбалансированный оппонент."
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
