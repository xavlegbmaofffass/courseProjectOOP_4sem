using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FrontlineCardWarfare.Controls;

public partial class CardControl : UserControl
{
    public static readonly DependencyProperty CardProperty =
        DependencyProperty.Register(nameof(Card), typeof(global::FrontlineCardWarfare.Data.Card), typeof(CardControl),
            new PropertyMetadata(null, OnCardChanged));

    public global::FrontlineCardWarfare.Data.Card? Card
    {
        get => (global::FrontlineCardWarfare.Data.Card?)GetValue(CardProperty);
        set => SetValue(CardProperty, value);
    }

    private static void OnCardChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CardControl control)
        {
            control.OnCardChanged();
        }
    }

    private void OnCardChanged()
    {
        if (Card == null)
        {
            NotPlayableOverlay.Visibility = Visibility.Collapsed;
            return;
        }

        // Обновление видимости индикаторов
        NotPlayableOverlay.Visibility = Visibility.Collapsed;
        DisabledOverlay.Visibility = Visibility.Collapsed;

        // Обновление цвета рамки
        CardBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 217, 255));
        CardBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect
        {
            Color = Color.FromRgb(0, 217, 255),
            BlurRadius = 10,
            ShadowDepth = 0,
            Opacity = 0.3
        };
    }

    public CardControl()
    {
        InitializeComponent();
        Loaded += CardControl_Loaded;
        MouseLeftButtonDown += CardBorder_MouseLeftButtonDown;
    }

    private void CardControl_Loaded(object sender, RoutedEventArgs e)
    {
        // Запуск анимации появления при загрузке
        StartAppearAnimation();
    }

    private void StartAppearAnimation()
    {
        var storyboard = FindResource("CardAppearAnimation") as Storyboard;
        if (storyboard != null)
        {
            // Запускаем анимацию для текущего элемента
            storyboard.Begin(this);
        }
    }

    private void CardBorder_MouseEnter(object sender, MouseEventArgs e)
    {
        if (Card == null) return;

        // Подъём карты при наведении
        CardTransform.BeginAnimation(TranslateTransform.YProperty, 
            new DoubleAnimation(-15, TimeSpan.FromMilliseconds(150))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            });

        // Подсветка тени
        if (CardBorder.Effect is System.Windows.Media.Effects.DropShadowEffect effect)
        {
            effect.Opacity = 0.6;
            effect.BlurRadius = 20;
        }
    }

    private void CardBorder_MouseLeave(object sender, MouseEventArgs e)
    {
        // Возврат позиции
        CardTransform.BeginAnimation(TranslateTransform.YProperty,
            new DoubleAnimation(0, TimeSpan.FromMilliseconds(150))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            });

        // Возврат тени
        if (CardBorder.Effect is System.Windows.Media.Effects.DropShadowEffect effect)
        {
            effect.Opacity = 0.3;
            effect.BlurRadius = 10;
        }
        
    }

    private void CardBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Card == null)
            return;

        // Анимация нажатия
        CardTransform.BeginAnimation(TranslateTransform.YProperty,
            new DoubleAnimation(-5, TimeSpan.FromMilliseconds(50)));

        // Мы не запускаем DoDragDrop здесь, так как это мешает
        // логике в BattleView, которая учитывает порог перемещения (DragThreshold)
    }

    private void CardBorder_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    private void CardBorder_Drop(object sender, DragEventArgs e)
    {
        e.Handled = true;
    }
}
