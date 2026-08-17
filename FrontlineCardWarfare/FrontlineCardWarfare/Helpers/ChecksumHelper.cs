using System;
using System.Security.Cryptography;
using System.Text;

namespace FrontlineCardWarfare.Helpers;

/// <summary>
/// Вспомогательный класс для вычисления контрольных сумм.
/// Используется для проверки целостности сохранённых игр.
/// </summary>
public static class ChecksumHelper
{
    /// <summary>
    /// Вычисляет SHA256 хэш для строковых данных.
    /// </summary>
    /// <param name="data">Данные для вычисления хэша.</param>
    /// <returns>Hex-представление хэша.</returns>
    public static string ComputeChecksum(string data)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(data);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes);
    }

    /// <summary>
    /// Проверяет соответствие данных контрольной сумме.
    /// </summary>
    /// <param name="data">Данные для проверки.</param>
    /// <param name="checksum">Ожидаемая контрольная сумма.</param>
    /// <returns>True, если данные соответствуют хэшу.</returns>
    public static bool VerifyChecksum(string data, string checksum)
    {
        var computedChecksum = ComputeChecksum(data);
        return string.Equals(computedChecksum, checksum, StringComparison.OrdinalIgnoreCase);
    }
}
