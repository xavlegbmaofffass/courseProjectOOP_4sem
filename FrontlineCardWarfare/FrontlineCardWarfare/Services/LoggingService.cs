using System.IO;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Сервис логирования — записывает логи в файл.
/// </summary>
public class LoggingService : ILoggingService
{
    private readonly string _logDirectory;
    private readonly string _logFile;
    private readonly LogLevel _minLevel;
    private readonly object _lock = new();

    /// <summary>
    /// Инициализирует новый экземпляр LoggingService.
    /// </summary>
    public LoggingService(LogLevel minLevel = LogLevel.Debug)
    {
        _logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FrontlineCardWarfare",
            "Logs");

        _logFile = Path.Combine(_logDirectory, $"log_{DateTime.Now:yyyyMMdd}.txt");
        _minLevel = minLevel;

        Directory.CreateDirectory(_logDirectory);
    }

    /// <summary>
    /// Записывает сообщение в лог.
    /// </summary>
    public void Log(LogLevel level, string message)
    {
        if (level < _minLevel)
            return;

        lock (_lock)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var levelStr = level.ToString().ToUpper();
                var logEntry = $"[{timestamp}] [{levelStr}] {message}";

                File.AppendAllText(_logFile, logEntry + Environment.NewLine);
                System.Diagnostics.Debug.WriteLine(logEntry);
            }
            catch
            {
                // Не бросаем исключения из логирования
            }
        }
    }

    /// <summary>
    /// Записывает сообщение об ошибке с исключением.
    /// </summary>
    public void LogError(string message, Exception ex)
    {
        Log(LogLevel.Error, $"{message}{Environment.NewLine}Исключение: {ex.GetType().Name}: {ex.Message}{Environment.NewLine}Стек: {ex.StackTrace}");
    }

    /// <summary>
    /// Записывает информационное сообщение.
    /// </summary>
    public void LogInfo(string message)
    {
        Log(LogLevel.Info, message);
    }

    /// <summary>
    /// Записывает предупреждение.
    /// </summary>
    public void LogWarning(string message)
    {
        Log(LogLevel.Warning, message);
    }

    /// <summary>
    /// Записывает отладочное сообщение.
    /// </summary>
    public void LogDebug(string message)
    {
        Log(LogLevel.Debug, message);
    }
}
