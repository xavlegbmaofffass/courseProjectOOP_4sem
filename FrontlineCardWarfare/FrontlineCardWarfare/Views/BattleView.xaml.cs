using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FrontlineCardWarfare.Controls;
using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Models;
using FrontlineCardWarfare.ViewModels;
using FrontlineCardWarfare.Services;

namespace FrontlineCardWarfare.Views;

/// <summary>
/// Логика взаимодействия для BattleView.xaml
/// </summary>
public partial class BattleView : UserControl
{
    private BattleViewModel? _viewModel;
    private bool _isDragging;
    private Point _dragStartPoint;
    private const double DragThreshold = 5.0;
    private readonly IBattleAnimationService _animationService;

    public BattleView()
    {
        InitializeComponent();
        _animationService = ((App)Application.Current).GetService<IBattleAnimationService>();
        Loaded += BattleView_Loaded;
        Unloaded += BattleView_Unloaded;
    }

    /// <summary>
    /// Запускает анимацию появления карты из руки противника.
    /// </summary>
    public void AnimateEnemyCardAppearance()
    {
        _ = _animationService.PlayEnemyCardAppearanceAsync(EnemyZone, BattleCanvas);
    }

    private void BattleView_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel = DataContext as BattleViewModel;

        if (_viewModel != null)
        {
            // Подписка на события анимаций
            _viewModel.AnimationEvents.OnCardPlayed += OnCardPlayed;
            _viewModel.AnimationEvents.OnCardAddedToHand += OnCardAddedToHand;
            _viewModel.AnimationEvents.OnUnitPlaced += OnUnitPlaced;
            _viewModel.AnimationEvents.OnAttackOccurred += OnAttackOccurred;
            _viewModel.AnimationEvents.OnUnitTookDamage += OnUnitTookDamage;
            _viewModel.AnimationEvents.OnUnitDestroyed += OnUnitDestroyed;
            _viewModel.AnimationEvents.OnTurnChanged += OnTurnChanged;

            // Если ряды пусты, вызываем инициализацию игры
            if (_viewModel.BoardCellsRow0.Count == 0 && _viewModel.BoardCellsRow1.Count == 0 &&
                _viewModel.BoardCellsRow2.Count == 0 && _viewModel.BoardCellsRow3.Count == 0)
            {
                _ = InitializeGameAsync();
            }
        }
    }

    private async Task InitializeGameAsync()
    {
        if (_viewModel == null) return;

        try
        {
            var playerDeck = new Deck { Id = 1, Name = "Default Deck" };
            var enemyDeck = new Deck { Id = 2, Name = "Enemy Deck" };

            await _viewModel.InitializeGameAsync(playerDeck, enemyDeck, "Medium");
            _viewModel.UpdateBoardFromModel();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BattleView] Ошибка при инициализации игры: {ex.Message}");
        }
    }

    private void BattleView_Unloaded(object sender, RoutedEventArgs e)
    {
        _animationService.StopAll();
        if (_viewModel != null)
        {
            _viewModel.AnimationEvents.OnCardPlayed -= OnCardPlayed;
            _viewModel.AnimationEvents.OnCardAddedToHand -= OnCardAddedToHand;
            _viewModel.AnimationEvents.OnUnitPlaced -= OnUnitPlaced;
            _viewModel.AnimationEvents.OnAttackOccurred -= OnAttackOccurred;
            _viewModel.AnimationEvents.OnUnitTookDamage -= OnUnitTookDamage;
            _viewModel.AnimationEvents.OnUnitDestroyed -= OnUnitDestroyed;
            _viewModel.AnimationEvents.OnTurnChanged -= OnTurnChanged;
        }
    }

    #region Animation Handlers

    private void OnCardAddedToHand(Data.Card card)
    {
        var element = FindCardControl(card);
        if (element != null)
        {
            _ = _animationService.PlayCardAppearAsync(element);
        }
    }

    private void OnCardPlayed(Data.Card card, int row, int col)
    {
        // Ищем CellControl по координатам
        var cell = FindCellControl(row, col);

        if (cell != null)
        {
            // Только анимация появления юнита в клетке
            _ = _animationService.PlayCardAppearAsync(cell);
        }
    }

    private void OnUnitPlaced(Unit unit)
    {
        var element = FindCellControl(unit.Row, unit.Column);
        if (element != null)
        {
            _ = _animationService.PlayUnitPlaceAsync(element);
        }
    }

    private void OnAttackOccurred(Unit attacker, Unit target, int damage)
    {
        var attackerEl = FindCellControl(attacker.Row, attacker.Column);
        var targetEl = FindCellControl(target.Row, target.Column);
        
        if (attackerEl != null && targetEl != null)
        {
            _ = _animationService.PlayAttackAsync(attackerEl, targetEl, BattleGrid);
        }
    }

    private void OnUnitTookDamage(Unit unit, int damage)
    {
        var element = FindCellControl(unit.Row, unit.Column);
        if (element != null)
        {
            _ = _animationService.PlayDamageAsync(element, damage, BattleCanvas);
        }
    }

    private void OnUnitDestroyed(Unit unit)
    {
        var element = FindCellControl(unit.Row, unit.Column);
        if (element != null)
        {
            _ = _animationService.PlayUnitDestroyAsync(element, BattleCanvas);
        }
    }

    private void OnTurnChanged()
    {
        _ = _animationService.PlayTurnChangeAsync(CentralLine, BattleGrid);
    }

    private FrameworkElement? FindCardControl(Data.Card card)
    {
        return FindVisualChild<CardControl>(this, c => c.DataContext == card);
    }

    private FrameworkElement? FindCellControl(int row, int col)
    {
        // Поиск контроллера клетки по координатам в визуальном дереве
        return FindVisualChild<CellControl>(this, c => 
            c.DataContext is Models.BoardCell cell && cell.Row == row && cell.Column == col);
    }

    private T? FindVisualChild<T>(DependencyObject parent, Func<T, bool> predicate) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t && predicate(t))
                return t;
            
            var result = FindVisualChild(child, predicate);
            if (result != null)
                return result;
        }
        return null;
    }

    #endregion

    #region Drag-Drop Logic

    private void CardControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[DND] MouseDown. Interaction: {_viewModel?.IsInteractionEnabled}");
        if (sender is FrameworkElement cardElement &&
            cardElement.DataContext is Data.Card card &&
            _viewModel?.IsInteractionEnabled == true)
        {
            _dragStartPoint = e.GetPosition(this);
            _isDragging = false;
            System.Diagnostics.Debug.WriteLine($"[DND] Drag initialized for {card.Name}");
        }
    }

    private void CardControl_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _isDragging) return;

        if (sender is FrameworkElement cardElement &&
            cardElement.DataContext is Data.Card card)
        {
            var currentPoint = e.GetPosition(this);
            var diff = _dragStartPoint - currentPoint;

            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || 
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                if (_viewModel?.IsInteractionEnabled != true)
                {
                    System.Diagnostics.Debug.WriteLine("[DND] MouseMove: Interaction disabled, aborting drag");
                    return;
                }

                _isDragging = true;
                System.Diagnostics.Debug.WriteLine($"[DND] Starting DoDragDrop for {card.Name}");
                
                var dataObject = new DataObject("Card", card); 
                cardElement.Opacity = 0.5;

                try
                {
                    DragDrop.DoDragDrop(cardElement, dataObject, DragDropEffects.Move);
                }
                finally
                {
                    cardElement.Opacity = 1.0;
                    _isDragging = false;
                    System.Diagnostics.Debug.WriteLine("[DND] DoDragDrop finished");
                }
            }
        }
    }

    private void CardControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
    }

    private void BattleField_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent("Card") && _viewModel != null && _viewModel.IsInteractionEnabled)
        {
            e.Effects = DragDropEffects.Move | DragDropEffects.Copy;
            e.Handled = true;
            return;
        }
    
        e.Effects = DragDropEffects.None;
        e.Handled = true;
    }

    private void BattleField_Drop(object sender, DragEventArgs e)
    {
        _isDragging = false;
        if (_viewModel == null || !_viewModel.IsInteractionEnabled) return;

        var card = e.Data.GetData("Card") as Data.Card;
        if (card == null) return;

        // Ищем ячейку (CellControl) в которую бросили
        // Проверяем отправителя (sender) или источник (OriginalSource)
        CellControl? targetCell = sender as CellControl;
        if (targetCell == null)
        {
            var element = e.OriginalSource as DependencyObject;
            while (element != null && element is not CellControl)
            {
                element = VisualTreeHelper.GetParent(element);
            }
            targetCell = element as CellControl;
        }

        if (targetCell != null)
        {
            int row = targetCell.Row;
            int col = targetCell.Column;

            System.Diagnostics.Debug.WriteLine($"[DND] SUCCESSFUL DROP: {card.Name} -> ({row}, {col})");

            // Вызываем команду установки
            if (row >= 2)
            {
                // Используем прямое асинхронное выполнение
                _ = _viewModel.ExecutePlayCard(card, row, col);
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[DND] DROP FAILED: No CellControl found under drop point");
        }
        
        e.Handled = true;
    }

    /// <summary>
    /// Обработчик клика по клетке поля.
    /// </summary>
    private void CellControl_MouseLeftButtonUp_OnField(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel == null || !_viewModel.IsInteractionEnabled) return;

        if (sender is FrameworkElement element && element.DataContext is Models.BoardCell cell)
        {
            _ = _viewModel.HandleBoardCellClick(cell.Row, cell.Column);
            e.Handled = true;
        }
    }

    // Заглушки для предотвращения ошибок если они еще привязаны в XAML
    private void Card_DragOver(object sender, DragEventArgs e) { e.Effects = DragDropEffects.None; e.Handled = true; }
    private void Card_Drop(object sender, DragEventArgs e) { e.Effects = DragDropEffects.None; e.Handled = true; }
    private void PlayerHand_DragOver(object sender, DragEventArgs e) { e.Handled = true; }
    private void PlayerHand_Drop(object sender, DragEventArgs e) { e.Handled = true; }

    #endregion
}
