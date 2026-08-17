using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FrontlineCardWarfare.Commands;

/// <summary>
/// Асинхронная реализация ICommand для MVVM.
/// </summary>
public class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> _execute;
    private readonly Func<object?, bool>? _canExecute;
    private bool _isExecuting;

    /// <summary>
    /// Инициализирует новый экземпляр AsyncRelayCommand.
    /// </summary>
    /// <param name="execute">Асинхронный метод выполнения команды.</param>
    /// <param name="canExecute">Метод проверки возможности выполнения (опционально).</param>
    public AsyncRelayCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Инициализирует новый экземпляр AsyncRelayCommand без параметров.
    /// </summary>
    /// <param name="execute">Асинхронный метод выполнения команды.</param>
    /// <param name="canExecute">Метод проверки возможности выполнения (опционально).</param>
    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        : this(
            async _ => await execute(),
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
    public bool CanExecute(object? parameter)
    {
        if (_isExecuting)
            return false;

        return _canExecute?.Invoke(parameter) ?? true;
    }

    /// <summary>
    /// Выполняет команду асинхронно и возвращает задачу.
    /// </summary>
    public async Task ExecuteAsync(object? parameter)
    {
        if (!CanExecute(parameter))
            return;

        _isExecuting = true;
        RaiseCanExecuteChanged();

        try
        {
            await _execute(parameter);
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Выполняет команду асинхронно.
    /// </summary>
    public async void Execute(object? parameter)
    {
        await ExecuteAsync(parameter);
    }

    /// <summary>
    /// Вызывает событие CanExecuteChanged.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}
