using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FrontlineCardWarfare.Models;

namespace FrontlineCardWarfare.Controls;

public partial class CellControl : UserControl
{
    public static readonly DependencyProperty RowProperty =
        DependencyProperty.Register(nameof(Row), typeof(int), typeof(CellControl));

    public static readonly DependencyProperty ColumnProperty =
        DependencyProperty.Register(nameof(Column), typeof(int), typeof(CellControl));

    public static readonly DependencyProperty IsEmptyProperty =
        DependencyProperty.Register(nameof(IsEmpty), typeof(bool), typeof(CellControl),
            new PropertyMetadata(true));

    public static readonly DependencyProperty UnitImagePathProperty =
        DependencyProperty.Register(nameof(UnitImagePath), typeof(string), typeof(CellControl),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty HasUnitProperty =
        DependencyProperty.Register(nameof(HasUnit), typeof(bool), typeof(CellControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsAvailableForMoveProperty =
        DependencyProperty.Register(nameof(IsAvailableForMove), typeof(bool), typeof(CellControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsAvailableForAttackProperty =
        DependencyProperty.Register(nameof(IsAvailableForAttack), typeof(bool), typeof(CellControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(CellControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HealthPercentProperty =
        DependencyProperty.Register(nameof(HealthPercent), typeof(double), typeof(CellControl),
            new PropertyMetadata(1.0));

    public static readonly DependencyProperty HealthBarColorProperty =
        DependencyProperty.Register(nameof(HealthBarColor), typeof(Brush), typeof(CellControl),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0, 119, 182))));

    public static readonly DependencyProperty IsFrozenProperty =
        DependencyProperty.Register(nameof(IsFrozen), typeof(bool), typeof(CellControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty CannotAttackProperty =
        DependencyProperty.Register(nameof(CannotAttack), typeof(bool), typeof(CellControl),
            new PropertyMetadata(false));

    public int Row
    {
        get => (int)GetValue(RowProperty);
        set => SetValue(RowProperty, value);
    }

    public int Column
    {
        get => (int)GetValue(ColumnProperty);
        set => SetValue(ColumnProperty, value);
    }

    public bool IsEmpty
    {
        get => (bool)GetValue(IsEmptyProperty);
        set => SetValue(IsEmptyProperty, value);
    }

    public string UnitImagePath
    {
        get => (string)GetValue(UnitImagePathProperty);
        set => SetValue(UnitImagePathProperty, value);
    }

    public bool HasUnit
    {
        get => (bool)GetValue(HasUnitProperty);
        set => SetValue(HasUnitProperty, value);
    }

    public bool IsAvailableForMove
    {
        get => (bool)GetValue(IsAvailableForMoveProperty);
        set => SetValue(IsAvailableForMoveProperty, value);
    }

    public bool IsAvailableForAttack
    {
        get => (bool)GetValue(IsAvailableForAttackProperty);
        set => SetValue(IsAvailableForAttackProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public double HealthPercent
    {
        get => (double)GetValue(HealthPercentProperty);
        set => SetValue(HealthPercentProperty, value);
    }

    public Brush HealthBarColor
    {
        get => (Brush)GetValue(HealthBarColorProperty);
        set => SetValue(HealthBarColorProperty, value);
    }

    public bool IsFrozen
    {
        get => (bool)GetValue(IsFrozenProperty);
        set => SetValue(IsFrozenProperty, value);
    }

    public bool CannotAttack
    {
        get => (bool)GetValue(CannotAttackProperty);
        set => SetValue(CannotAttackProperty, value);
    }

    public CellControl()
    {
        InitializeComponent();
    }

    // Методы заглушки для совместимости с внешними вызовами (если остались)
    public void ShowDamageAnimation() { }
    public void ShowAttackRushAnimation() { }
    public void ShowSelectAnimation() { }
    public void ShowDestroyAnimation(Action? onComplete = null) { onComplete?.Invoke(); }
}
