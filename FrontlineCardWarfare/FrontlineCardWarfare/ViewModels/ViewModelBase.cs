using CommunityToolkit.Mvvm.ComponentModel;

namespace FrontlineCardWarfare.ViewModels;

/// <summary>
/// Базовый класс для всех ViewModel в приложении.
/// Наследуется от ObservableObject из CommunityToolkit.Mvvm.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
    /// <summary>
    /// Заголовок окна (опционально используется для навигации).
    /// </summary>
    public virtual string Title => string.Empty;

    /// <summary>
    /// Вызывается при активации ViewModel.
    /// </summary>
    public virtual void OnActivated()
    {
    }

    /// <summary>
    /// Вызывается при деактивации ViewModel.
    /// </summary>
    public virtual void OnDeactivated()
    {
    }
}
