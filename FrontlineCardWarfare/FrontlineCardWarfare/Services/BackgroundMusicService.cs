using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Интерфейс сервиса фоновой музыки.
/// </summary>
public interface IBackgroundMusicService
{
    /// <summary>
    /// Запускает фоновую музыку.
    /// </summary>
    void Play();

    /// <summary>
    /// Останавливает фоновую музыку.
    /// </summary>
    void Stop();

    /// <summary>
    /// Приостанавливает фоновую музыку.
    /// </summary>
    void Pause();

    /// <summary>
    /// Возобновляет фоновую музыку.
    /// </summary>
    void Resume();

    /// <summary>
    /// Устанавливает громкость музыки (0.0 - 1.0).
    /// </summary>
    void SetVolume(double volume);

    /// <summary>
    /// Возвращает текущее состояние воспроизведения.
    /// </summary>
    bool IsPlaying { get; }
}

/// <summary>
/// Сервис фоновой музыки — воспроизводит фоновую мелодию с зацикливанием с использованием MediaPlayer.
/// </summary>
public class BackgroundMusicService : IBackgroundMusicService, IDisposable
{
    private readonly MediaPlayer _mediaPlayer;
    private double _volume = 0.5;
    private bool _isPlaying;
    private bool _isPaused;

    public bool IsPlaying => _isPlaying && !_isPaused;

    public BackgroundMusicService()
    {
        _mediaPlayer = new MediaPlayer();
        _mediaPlayer.MediaEnded += OnMediaEnded;
        _mediaPlayer.MediaFailed += OnMediaFailed;
    }

    /// <summary>
    /// Запускает фоновую музыку.
    /// </summary>
    public void Play()
    {
        try
        {
            var audioPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Audio", "P.T. Adamczyk - The Rebel Path.mp3");

            if (!File.Exists(audioPath))
            {
                System.Diagnostics.Debug.WriteLine($"Фоновая музыка не найдена по пути: {audioPath}");
                return;
            }

            _mediaPlayer.Open(new Uri(audioPath));
            _mediaPlayer.Volume = _volume;
            _mediaPlayer.Play();
            _isPlaying = true;
            _isPaused = false;
            
            System.Diagnostics.Debug.WriteLine("Фоновая музыка запущена");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка воспроизведения музыки: {ex.Message}");
        }
    }

    /// <summary>
    /// Останавливает фоновую музыку.
    /// </summary>
    public void Stop()
    {
        try
        {
            _mediaPlayer.Stop();
            _isPlaying = false;
            _isPaused = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка остановки музыки: {ex.Message}");
        }
    }

    /// <summary>
    /// Приостанавливает фоновую музыку.
    /// </summary>
    public void Pause()
    {
        try
        {
            _mediaPlayer.Pause();
            _isPaused = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка паузы музыки: {ex.Message}");
        }
    }

    /// <summary>
    /// Возобновляет фоновую музыку из паузы.
    /// </summary>
    public void Resume()
    {
        try
        {
            if (_isPlaying && _isPaused)
            {
                _mediaPlayer.Play();
                _isPaused = false;
            }
            else if (!_isPlaying)
            {
                Play();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка возобновления музыки: {ex.Message}");
        }
    }

    /// <summary>
    /// Устанавливает громкость музыки (0.0 - 1.0).
    /// </summary>
    public void SetVolume(double volume)
    {
        _volume = Math.Clamp(volume, 0.0, 1.0);
        _mediaPlayer.Volume = _volume;
    }

    /// <summary>
    /// Обработчик окончания воспроизведения — перезапускает трек (зацикливание).
    /// </summary>
    private void OnMediaEnded(object? sender, EventArgs e)
    {
        try
        {
            _mediaPlayer.Position = TimeSpan.Zero;
            _mediaPlayer.Play();
            _isPlaying = true;
            _isPaused = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка зацикливания музыки: {ex.Message}");
        }
    }

    /// <summary>
    /// Обработчик ошибки воспроизведения.
    /// </summary>
    private void OnMediaFailed(object? sender, ExceptionEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Ошибка MediaPlayer: {e.ErrorException.Message}");
        _isPlaying = false;
    }

    /// <summary>
    /// Освобождает ресурсы.
    /// </summary>
    public void Dispose()
    {
        try
        {
            _mediaPlayer.Stop();
            _mediaPlayer.MediaEnded -= OnMediaEnded;
            _mediaPlayer.MediaFailed -= OnMediaFailed;
            _mediaPlayer.Close();
        }
        catch
        {
            // Игнорируем ошибки при очистке
        }
    }
}
