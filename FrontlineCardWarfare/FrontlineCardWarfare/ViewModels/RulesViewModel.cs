using System.Windows.Input;
using FrontlineCardWarfare.Commands;
using FrontlineCardWarfare.Services;

namespace FrontlineCardWarfare.ViewModels;

/// <summary>
/// ViewModel для экрана правил игры.
/// </summary>
public class RulesViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private int _selectedTab = 0;

    /// <summary>
    /// Инициализирует новый экземпляр RulesViewModel.
    /// </summary>
    public RulesViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        BackToMenuCommand = new RelayCommand(BackToMenu);
    }

    /// <summary>
    /// Заголовок окна.
    /// </summary>
    public override string Title => "Правила игры";

    /// <summary>
    /// Выбранная вкладка (0 — основные правила, 1 — способности, 2 — тактика).
    /// </summary>
    public int SelectedTab
    {
        get => _selectedTab;
        set => SetProperty(ref _selectedTab, value);
    }

    /// <summary>
    /// Команда возврата в меню.
    /// </summary>
    public ICommand BackToMenuCommand { get; }

    /// <summary>
    /// Название вкладки.
    /// </summary>
    public string Tab1Name => "Основные правила";
    public string Tab2Name => "Способности карт";
    public string Tab3Name => "Тактика и советы";

    private void BackToMenu(object? parameter)
    {
        _navigationService.NavigateTo<MainViewModel>();
    }
}
