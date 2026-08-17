using System;
using System.Collections.Generic;
using FrontlineCardWarfare.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Реализация сервиса навигации между ViewModel.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<ViewModelBase> _navigationStack = new();
    private ViewModelBase? _currentViewModel;

    /// <summary>
    /// Инициализирует новый экземпляр NavigationService.
    /// </summary>
    /// <param name="serviceProvider">Поставщик сервисов.</param>
    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Текущая активная ViewModel.
    /// </summary>
    public ViewModelBase? CurrentViewModel => _currentViewModel;

    /// <summary>
    /// Навигация к указанной ViewModel.
    /// </summary>
    public void NavigateTo<T>() where T : ViewModelBase
    {
        var viewModel = _serviceProvider.GetRequiredService<T>();
        NavigateTo(viewModel);
    }

    /// <summary>
    /// Навигация к указанной ViewModel с параметром.
    /// </summary>
    public void NavigateTo(ViewModelBase viewModel)
    {
        if (_currentViewModel != null)
        {
            _navigationStack.Push(_currentViewModel);
            _currentViewModel.OnDeactivated();
        }

        _currentViewModel = viewModel;
        _currentViewModel.OnActivated();

        // Сообщаем подписчикам об изменении
        CurrentViewModelChanged?.Invoke(this, _currentViewModel);
    }

    /// <summary>
    /// Возврат к предыдущей ViewModel.
    /// </summary>
    public void GoBack()
    {
        if (_navigationStack.Count == 0)
            return;

        if (_currentViewModel != null)
        {
            _currentViewModel.OnDeactivated();
        }

        _currentViewModel = _navigationStack.Pop();
        _currentViewModel.OnActivated();

        CurrentViewModelChanged?.Invoke(this, _currentViewModel);
    }

    /// <summary>
    /// Очистка истории навигации.
    /// </summary>
    public void ClearHistory()
    {
        _navigationStack.Clear();
    }

    /// <summary>
    /// Событие изменения текущей ViewModel.
    /// </summary>
    public event EventHandler<ViewModelBase>? CurrentViewModelChanged;
}
