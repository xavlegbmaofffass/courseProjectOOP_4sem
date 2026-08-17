namespace FrontlineCardWarfare.Helpers;

/// <summary>
/// Вспомогательный класс для хэширования паролей.
/// Использует BCrypt для безопасного хранения паролей.
/// </summary>
public static class PasswordHelper
{
    /// <summary>
    /// Хэширует пароль с использованием BCrypt.
    /// </summary>
    /// <param name="password">Пароль в открытом виде.</param>
    /// <returns>Хэш пароля.</returns>
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Проверяет соответствие пароля хэшу.
    /// </summary>
    /// <param name="password">Пароль в открытом виде.</param>
    /// <param name="hash">Хэш пароля для сравнения.</param>
    /// <returns>True, если пароль соответствует хэшу.</returns>
    public static bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
