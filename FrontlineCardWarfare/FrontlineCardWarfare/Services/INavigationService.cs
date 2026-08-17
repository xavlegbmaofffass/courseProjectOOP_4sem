using System;
using FrontlineCardWarfare.ViewModels;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Сервис навигации между ViewModel.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Текущая активная ViewModel.
    /// </summary>
    ViewModelBase? CurrentViewModel { get; }

    /// <summary>
    /// Событие изменения текущей ViewModel.
    /// </summary>
    event EventHandler<ViewModelBase>? CurrentViewModelChanged;

    /// <summary>
    /// Навигация к указанной ViewModel.
    /// </summary>
    /// <typeparam name="T">Тип ViewModel.</typeparam>
    void NavigateTo<T>() where T : ViewModelBase;

    /// <summary>
    /// Навигация к указанной ViewModel с параметром.
    /// </summary>
    /// <param name="viewModel">Экземпляр ViewModel.</param>
    void NavigateTo(ViewModelBase viewModel);

    /// <summary>
    /// Возврат к предыдущей ViewModel.
    /// </summary>
    void GoBack();

    /// <summary>
    /// Очистка истории навигации.
    /// </summary>
    void ClearHistory();
}
