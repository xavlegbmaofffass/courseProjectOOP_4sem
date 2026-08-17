using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FrontlineCardWarfare.Converters;

/// <summary>
/// Конвертер флага IsPlayerTurn в цвет индикатора хода.
/// </summary>
public class TurnIndicatorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isPlayerTurn)
        {
            if (isPlayerTurn)
            {
                // Синий/циановый для игрока
                return new SolidColorBrush(Color.FromRgb(26, 35, 68));
            }
            else
            {
                // Красный для противника
                return new SolidColorBrush(Color.FromRgb(26, 15, 24));
            }
        }
        return new SolidColorBrush(Color.FromRgb(50, 50, 60));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Конвертер флага IsPlayerTurn в цвет точки индикатора.
/// </summary>
public class TurnDotColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isPlayerTurn)
        {
            if (isPlayerTurn)
            {
                return new SolidColorBrush(Color.FromRgb(0, 212, 255)); // Циан
            }
            else
            {
                return new SolidColorBrush(Color.FromRgb(255, 56, 96)); // Красный
            }
        }
        return new SolidColorBrush(Color.FromRgb(100, 100, 100));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Конвертер флага IsPlayerTurn в текст индикатора.
/// </summary>
public class TurnTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isPlayerTurn)
        {
            return isPlayerTurn ? "Ваш ход" : "Ход противника";
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
