namespace FrontlineCardWarfare.Services;

/// <summary>
/// Уровень логирования.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Отладочная информация.
    /// </summary>
    Debug,

    /// <summary>
    /// Информационное сообщение.
    /// </summary>
    Info,

    /// <summary>
    /// Предупреждение.
    /// </summary>
    Warning,

    /// <summary>
    /// Ошибка.
    /// </summary>
    Error
}

/// <summary>
/// Интерфейс сервиса логирования.
/// </summary>
public interface ILoggingService
{
    /// <summary>
    /// Записывает сообщение в лог.
    /// </summary>
    void Log(LogLevel level, string message);

    /// <summary>
    /// Записывает сообщение об ошибке с исключением.
    /// </summary>
    void LogError(string message, Exception ex);

    /// <summary>
    /// Записывает информационное сообщение.
    /// </summary>
    void LogInfo(string message);

    /// <summary>
    /// Записывает предупреждение.
    /// </summary>
    void LogWarning(string message);

    /// <summary>
    /// Записывает отладочное сообщение.
    /// </summary>
    void LogDebug(string message);
}
