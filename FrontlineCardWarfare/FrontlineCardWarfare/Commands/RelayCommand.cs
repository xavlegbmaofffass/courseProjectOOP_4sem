using System;
using System.Windows.Input;

namespace FrontlineCardWarfare.Commands;

/// <summary>
/// Базовая реализация ICommand для MVVM.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    /// <summary>
    /// Инициализирует новый экземпляр RelayCommand.
    /// </summary>
    /// <param name="execute">Метод выполнения команды.</param>
    /// <param name="canExecute">Метод проверки возможности выполнения (опционально).</param>
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Инициализирует новый экземпляр RelayCommand без параметров.
    /// </summary>
    /// <param name="execute">Метод выполнения команды.</param>
    /// <param name="canExecute">Метод проверки возможности выполнения (опционально).</param>
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
        : this(
            _ => execute(),
            canExecute != null ? _ => canExecute() : null)
    {
    }

    /// <summary>
    /// Событие изменения возможности выполнения команды.
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>
    /// Определяет, может ли команда быть выполнена.
    /// </summary>
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    /// <summary>
    /// Выполняет команду.
    /// </summary>
    public void Execute(object? parameter) => _execute(parameter);

    /// <summary>
    /// Принудительно вызывает пересмотр возможности выполнения команды.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}
