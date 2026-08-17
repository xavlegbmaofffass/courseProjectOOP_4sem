using System;
using System.Text.RegularExpressions;

namespace FrontlineCardWarfare.Helpers;

/// <summary>
/// Вспомогательный класс для валидации данных.
/// </summary>
public static partial class Validator
{
    /// <summary>
    /// Проверяет корректность имени пользователя (3-20 символов, буквы/цифры/_).
    /// </summary>
    public static bool ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        return UsernameRegex().IsMatch(username) && username.Length >= 3 && username.Length <= 20;
    }

    /// <summary>
    /// Проверяет корректность пароля (мин. 6 символов).
    /// </summary>
    public static bool ValidatePassword(string password)
    {
        return !string.IsNullOrWhiteSpace(password) && password.Length >= 6;
    }

    /// <summary>
    /// Проверяет корректность email.
    /// </summary>
    public static bool ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return EmailRegex().IsMatch(email);
    }

    /// <summary>
    /// Проверяет корректность названия карты (1-50 символов).
    /// </summary>
    public static bool ValidateCardName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && name.Length >= 1 && name.Length <= 50;
    }

    /// <summary>
    /// Проверяет, что числовое значение находится в указанном диапазоне.
    /// </summary>
    public static bool ValidateNumericRange(int value, int min, int max)
    {
        return value >= min && value <= max;
    }

    /// <summary>
    /// Проверяет, что строковое значение не пустое и не превышает максимальную длину.
    /// </summary>
    public static bool ValidateStringLength(string value, int maxLength)
    {
        return !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;
    }

    /// <summary>
    /// Проверяет название колоды (1-30 символов).
    /// </summary>
    public static bool ValidateDeckName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && name.Length >= 1 && name.Length <= 30;
    }

    [GeneratedRegex(@"^[a-zA-Z0-9_]+$")]
    private static partial Regex UsernameRegex();

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
