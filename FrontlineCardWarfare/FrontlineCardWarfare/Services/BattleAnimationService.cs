using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Сервис анимаций боя — управляет всеми визуальными эффектами через WPF Storyboard.
/// Не блокирует UI-поток, использует асинхронные задачи для последовательности.
/// </summary>
public interface IBattleAnimationService
{
    Task PlayCardAppearAsync(UIElement target);
    Task PlayEnemyCardAppearanceAsync(UIElement handContainer, Canvas boardCanvas);
    Task PlayUnitPlaceAsync(UIElement target);
    Task PlayAttackAsync(UIElement attacker, UIElement target, Grid? boardGrid);
    Task PlayDamageAsync(UIElement target, int damage, Canvas? boardCanvas = null);
    Task PlayUnitDestroyAsync(UIElement target, Canvas? particleCanvas = null);
    Task PlayTurnChangeAsync(Border turnIndicator, Grid? boardGrid);
    void StopAll();
}

public class BattleAnimationService : IBattleAnimationService
{
    private readonly Random _random = new Random();
    private readonly List<Storyboard> _activeStoryboards = new();

    public BattleAnimationService()
    {
    }

    /// <summary>
    /// Останавливает все активные анимации.
    /// </summary>
    public void StopAll()
    {
        foreach (var sb in _activeStoryboards.ToList())
        {
            sb.Stop();
            _activeStoryboards.Remove(sb);
        }
    }

    /// <summary>
    /// Анимация появления карты в руке (slide+fade 0.3с)
    /// </summary>
    public async Task PlayCardAppearAsync(UIElement target)
    {
        if (target == null) return;

        var storyboard = CreateStoryboard();

        // Начальное состояние
        target.Opacity = 0;
        
        if (target is FrameworkElement fe)
        {
            fe.RenderTransformOrigin = new Point(0.5, 0.5);
            var tt = new TranslateTransform(0, 50);
            fe.RenderTransform = tt;

            // Fade-in
            var opacityAnim = new DoubleAnimation(1, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(opacityAnim, target);
            Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(opacityAnim);

            // Slide-up
            var slideAnim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(slideAnim, tt);
            Storyboard.SetTargetProperty(slideAnim, new PropertyPath(TranslateTransform.YProperty));
            storyboard.Children.Add(slideAnim);

            await PlayStoryboardAsync(storyboard);

            // Очищаем анимацию, чтобы разблокировать свойства
            target.BeginAnimation(UIElement.OpacityProperty, null);
            if (target is FrameworkElement element)
            {
                element.RenderTransform = null;
            }
            target.Opacity = 1;
        }
    }

    /// <summary>
    /// Анимация появления рандомной карты из руки противника.
    /// </summary>
    public async Task PlayEnemyCardAppearanceAsync(UIElement handContainer, Canvas boardCanvas)
    {
        if (handContainer == null || boardCanvas == null) return;

        // Создаем фантомную карту
        var cardGhost = new Border
        {
            Width = 100,
            Height = 150,
            Background = new SolidColorBrush(Color.FromRgb(30, 30, 50)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(255, 68, 68)),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Opacity = 0,
            RenderTransformOrigin = new Point(0.5, 0.5),
            Child = new TextBlock 
            { 
                Text = "?", 
                Foreground = Brushes.White, 
                FontSize = 32, 
                HorizontalAlignment = HorizontalAlignment.Center, 
                VerticalAlignment = VerticalAlignment.Center 
            }
        };

        var startPos = handContainer.TranslatePoint(new Point(handContainer.RenderSize.Width / 2, 0), boardCanvas);
        Canvas.SetLeft(cardGhost, startPos.X - 50);
        Canvas.SetTop(cardGhost, startPos.Y);
        boardCanvas.Children.Add(cardGhost);

        var storyboard = CreateStoryboard();
        
        // Появление и движение к центру
        var opacityAnim = new DoubleAnimation(1, TimeSpan.FromMilliseconds(300));
        var moveAnim = new DoubleAnimation(startPos.Y + 100, TimeSpan.FromMilliseconds(500))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        Storyboard.SetTarget(opacityAnim, cardGhost);
        Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(opacityAnim);

        Storyboard.SetTarget(moveAnim, cardGhost);
        Storyboard.SetTargetProperty(moveAnim, new PropertyPath(Canvas.TopProperty));
        storyboard.Children.Add(moveAnim);

        await PlayStoryboardAsync(storyboard);
        await Task.Delay(200);

        // Исчезновение
        var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300));
        var sbFade = CreateStoryboard();
        Storyboard.SetTarget(fadeOut, cardGhost);
        Storyboard.SetTargetProperty(fadeOut, new PropertyPath(UIElement.OpacityProperty));
        sbFade.Children.Add(fadeOut);
        await PlayStoryboardAsync(sbFade);

        boardCanvas.Children.Remove(cardGhost);
    }

    /// <summary>
    /// Анимация размещения юнита на поле (scale+fade 0.4с)
    /// </summary>
    public async Task PlayUnitPlaceAsync(UIElement target)
    {
        if (target == null) return;

        target.Opacity = 0;
        var storyboard = CreateStoryboard();

        if (target is FrameworkElement fe)
        {
            fe.RenderTransformOrigin = new Point(0.5, 0.5);
            var st = new ScaleTransform(0.5, 0.5);
            fe.RenderTransform = st;

            var opacityAnim = new DoubleAnimation(1, TimeSpan.FromMilliseconds(400));
            var scaleAnimX = new DoubleAnimation(1, TimeSpan.FromMilliseconds(400)) { EasingFunction = new BackEase { Amplitude = 0.5, EasingMode = EasingMode.EaseOut } };
            var scaleAnimY = new DoubleAnimation(1, TimeSpan.FromMilliseconds(400)) { EasingFunction = new BackEase { Amplitude = 0.5, EasingMode = EasingMode.EaseOut } };

            Storyboard.SetTarget(opacityAnim, target);
            Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(opacityAnim);

            Storyboard.SetTarget(scaleAnimX, st);
            Storyboard.SetTargetProperty(scaleAnimX, new PropertyPath(ScaleTransform.ScaleXProperty));
            storyboard.Children.Add(scaleAnimX);

            Storyboard.SetTarget(scaleAnimY, st);
            Storyboard.SetTargetProperty(scaleAnimY, new PropertyPath(ScaleTransform.ScaleYProperty));
            storyboard.Children.Add(scaleAnimY);

            await PlayStoryboardAsync(storyboard);

            // Очищаем анимацию и трансформацию
            target.BeginAnimation(UIElement.OpacityProperty, null);
            if (target is FrameworkElement element)
            {
                element.RenderTransform = null;
            }
            target.Opacity = 1;
        }
    }

    /// <summary>
    /// Анимация атаки (рывок к цели + тряска экрана + вспышка 0.4с)
    /// </summary>
    public async Task PlayAttackAsync(UIElement attacker, UIElement target, Grid? boardGrid)
    {
        if (attacker == null || target == null) return;

        // 1. Анимация рывка атакующего (150мс)
        if (attacker is FrameworkElement attackerFe)
        {
            attackerFe.RenderTransformOrigin = new Point(0.5, 0.5);
            var tt = new TranslateTransform(0, 0);
            attackerFe.RenderTransform = tt;

            double direction = 1;
            if (boardGrid != null)
            {
                var aPos = attackerFe.TranslatePoint(new Point(0, 0), boardGrid);
                var tPos = target.TranslatePoint(new Point(0, 0), boardGrid);
                if (aPos.X > tPos.X) direction = -1;
            }

            var lungeAnim = new DoubleAnimation(40 * direction, TimeSpan.FromMilliseconds(100)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } };
            var returnAnim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(100)) { BeginTime = TimeSpan.FromMilliseconds(100), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };

            var sb = CreateStoryboard();
            Storyboard.SetTarget(lungeAnim, tt);
            Storyboard.SetTargetProperty(lungeAnim, new PropertyPath(TranslateTransform.XProperty));
            sb.Children.Add(lungeAnim);
            Storyboard.SetTarget(returnAnim, tt);
            Storyboard.SetTargetProperty(returnAnim, new PropertyPath(TranslateTransform.XProperty));
            sb.Children.Add(returnAnim);
            
            await PlayStoryboardAsync(sb);
        }

        // 2. Тряска экрана + вспышка (200мс)
        var shakeTask = boardGrid != null ? PlayScreenShakeAsync(boardGrid) : Task.CompletedTask;
        var flashTask = PlayTargetFlashAsync(target);
        
        await Task.WhenAll(shakeTask, flashTask);
    }

    /// <summary>
    /// Анимация получения урона (красный оверлей + всплывающие цифры 0.5с)
    /// </summary>
    public async Task PlayDamageAsync(UIElement target, int damage, Canvas? boardCanvas = null)
    {
        if (target == null) return;

        var flashTask = Task.CompletedTask;
        if (target is FrameworkElement fe)
        {
            var originalEffect = fe.Effect;
            var redFlashEffect = new DropShadowEffect { Color = Colors.Red, BlurRadius = 40, ShadowDepth = 0, Opacity = 0 };
            fe.Effect = redFlashEffect;

            var sb = CreateStoryboard();
            var flashIn = new DoubleAnimation(0.8, TimeSpan.FromMilliseconds(100));
            var flashOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(400)) { BeginTime = TimeSpan.FromMilliseconds(100) };
            
            Storyboard.SetTarget(flashIn, redFlashEffect);
            Storyboard.SetTargetProperty(flashIn, new PropertyPath(DropShadowEffect.OpacityProperty));
            sb.Children.Add(flashIn);
            Storyboard.SetTarget(flashOut, redFlashEffect);
            Storyboard.SetTargetProperty(flashOut, new PropertyPath(DropShadowEffect.OpacityProperty));
            sb.Children.Add(flashOut);

            flashTask = PlayStoryboardAsync(sb).ContinueWith(_ => Application.Current.Dispatcher.Invoke(() => fe.Effect = originalEffect));
        }

        var damageNumTask = Task.CompletedTask;
        if (boardCanvas != null)
        {
            damageNumTask = ShowDamageNumberAsync(boardCanvas, target, damage);
        }

        await Task.WhenAll(flashTask, damageNumTask);
    }

    private async Task ShowDamageNumberAsync(Canvas canvas, UIElement target, int damage)
    {
        var text = new TextBlock
        {
            Text = $"-{damage}",
            Foreground = Brushes.Red,
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            Opacity = 1,
            IsHitTestVisible = false
        };

        var pos = target.TranslatePoint(new Point(0, 0), canvas);
        Canvas.SetLeft(text, pos.X + 20);
        Canvas.SetTop(text, pos.Y - 20);
        canvas.Children.Add(text);

        var sb = CreateStoryboard();
        var moveUp = new DoubleAnimation(pos.Y - 60, TimeSpan.FromMilliseconds(500)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
        var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(500)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } };

        Storyboard.SetTarget(moveUp, text);
        Storyboard.SetTargetProperty(moveUp, new PropertyPath(Canvas.TopProperty));
        sb.Children.Add(moveUp);

        Storyboard.SetTarget(fadeOut, text);
        Storyboard.SetTargetProperty(fadeOut, new PropertyPath(UIElement.OpacityProperty));
        sb.Children.Add(fadeOut);

        await PlayStoryboardAsync(sb);
        canvas.Children.Remove(text);
    }

    /// <summary>
    /// Анимация уничтожения юнита (fade-out + рассыпание частиц 0.6с)
    /// </summary>
    public async Task PlayUnitDestroyAsync(UIElement target, Canvas? particleCanvas = null)
    {
        if (target == null) return;

        // Очищаем предыдущие анимации Opacity, если они были
        target.BeginAnimation(UIElement.OpacityProperty, null);
        target.Opacity = 1;

        var sb = CreateStoryboard();
        var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(600)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } };
        Storyboard.SetTarget(fadeOut, target);
        Storyboard.SetTargetProperty(fadeOut, new PropertyPath(UIElement.OpacityProperty));
        sb.Children.Add(fadeOut);

        var destroyTask = PlayStoryboardAsync(sb);
        var particlesTask = Task.CompletedTask;
        
        if (particleCanvas != null && target is FrameworkElement fe)
        {
            particlesTask = PlayParticlesAsync(particleCanvas, fe);
        }

        await Task.WhenAll(destroyTask, particlesTask);

        // ВАЖНО: Очищаем анимацию, чтобы разблокировать свойство Opacity,
        // и возвращаем прозрачность в 1, чтобы ячейка поля не исчезла навсегда.
        target.BeginAnimation(UIElement.OpacityProperty, null);
        target.Opacity = 1;
    }

    /// <summary>
    /// Анимация смены хода (пульсация центральной линии 0.3с)
    /// </summary>
    public async Task PlayTurnChangeAsync(Border turnIndicator, Grid boardGrid)
    {
        if (turnIndicator == null) return;

        var sb = CreateStoryboard();
        var pulse = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(150)) { AutoReverse = true, EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut } };
        Storyboard.SetTarget(pulse, turnIndicator);
        Storyboard.SetTargetProperty(pulse, new PropertyPath(UIElement.OpacityProperty));
        sb.Children.Add(pulse);

        await PlayStoryboardAsync(sb);
    }

    #region Helper Methods

    private Storyboard CreateStoryboard()
    {
        var sb = new Storyboard();
        // Устанавливаем 60 FPS для всех анимаций для максимальной плавности
        Timeline.SetDesiredFrameRate(sb, 60);
        _activeStoryboards.Add(sb);
        sb.Completed += (s, e) => _activeStoryboards.Remove(sb);
        return sb;
    }

    private async Task PlayStoryboardAsync(Storyboard storyboard)
    {
        var tcs = new TaskCompletionSource();
        
        // Даем UI-потоку возможность обработать текущие события перед началом анимации
        await Task.Yield();

        storyboard.Completed += (s, e) => tcs.TrySetResult();
        storyboard.Begin();

        await tcs.Task;
    }

    private async Task PlayTargetFlashAsync(UIElement target)
    {
        if (target == null) return;

        var flashBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(120, 255, 255, 100)),
            Opacity = 0,
            IsHitTestVisible = false
        };

        if (target is Panel panel)
        {
            panel.Children.Add(flashBorder);
        }

        var storyboard = CreateStoryboard();
        var flashAnim = new DoubleAnimation(1, TimeSpan.FromMilliseconds(50));
        var fadeOutAnim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(150))
        {
            BeginTime = TimeSpan.FromMilliseconds(50)
        };

        Storyboard.SetTarget(flashAnim, flashBorder);
        Storyboard.SetTargetProperty(flashAnim, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(flashAnim);

        Storyboard.SetTarget(fadeOutAnim, flashBorder);
        Storyboard.SetTargetProperty(fadeOutAnim, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(fadeOutAnim);

        await PlayStoryboardAsync(storyboard);

        if (target is Panel panel2 && panel2.Children.Contains(flashBorder))
        {
            panel2.Children.Remove(flashBorder);
        }
    }

    private async Task PlayScreenShakeAsync(Grid boardGrid)
    {
        if (boardGrid == null) return;

        // Создаём TransformGroup с TranslateTransform для тряски
        var transformGroup = new TransformGroup();
        var translateTransform = new TranslateTransform(0, 0);
        transformGroup.Children.Add(translateTransform);
        boardGrid.RenderTransform = transformGroup;
        boardGrid.RenderTransformOrigin = new Point(0.5, 0.5);

        var storyboard = CreateStoryboard();

        // Последовательные позиции тряски (5 кадров по 40мс = 200мс всего)
        var shakePositions = new[] {
            (-3, -2), (3, 2), (-3, 2), (3, -2), (0, 0)
        };

        for (int i = 0; i < shakePositions.Length; i++)
        {
            var delay = TimeSpan.FromMilliseconds(i * 40);
            
            var translateAnimX = new DoubleAnimation(shakePositions[i].Item1, TimeSpan.FromMilliseconds(40))
            {
                BeginTime = delay,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            var translateAnimY = new DoubleAnimation(shakePositions[i].Item2, TimeSpan.FromMilliseconds(40))
            {
                BeginTime = delay,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(translateAnimX, translateTransform);
            Storyboard.SetTargetProperty(translateAnimX, new PropertyPath(TranslateTransform.XProperty));
            storyboard.Children.Add(translateAnimX);

            Storyboard.SetTarget(translateAnimY, translateTransform);
            Storyboard.SetTargetProperty(translateAnimY, new PropertyPath(TranslateTransform.YProperty));
            storyboard.Children.Add(translateAnimY);
        }

        await PlayStoryboardAsync(storyboard);
    }

    private async Task PlayParticlesAsync(Canvas particleCanvas, FrameworkElement sourceElement)
    {
        var particles = new List<Ellipse>();
        
        // Координаты источника относительно Canvas
        var sourcePosition = sourceElement.TranslatePoint(new Point(sourceElement.ActualWidth / 2, sourceElement.ActualHeight / 2), particleCanvas);

        for (int i = 0; i < 12; i++)
        {
            var particle = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Color.FromArgb(200, 255, 200, 50)),
                Opacity = 1
            };

            Canvas.SetLeft(particle, sourcePosition.X);
            Canvas.SetTop(particle, sourcePosition.Y);
            particleCanvas.Children.Add(particle);
            particles.Add(particle);

            var storyboard = CreateStoryboard();

            var angle = (360.0 / 12) * i;
            var radians = angle * Math.PI / 180;
            var velocity = 60 + _random.Next(-20, 20);
            var targetX = sourcePosition.X + (float)(Math.Cos(radians) * velocity);
            var targetY = sourcePosition.Y + (float)(Math.Sin(radians) * velocity);

            var translateX = new DoubleAnimation(targetX, TimeSpan.FromMilliseconds(600))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            var translateY = new DoubleAnimation(targetY, TimeSpan.FromMilliseconds(600))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(600))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            Storyboard.SetTarget(translateX, particle);
            Storyboard.SetTargetProperty(translateX, new PropertyPath(Canvas.LeftProperty));
            storyboard.Children.Add(translateX);

            Storyboard.SetTarget(translateY, particle);
            Storyboard.SetTargetProperty(translateY, new PropertyPath(Canvas.TopProperty));
            storyboard.Children.Add(translateY);

            Storyboard.SetTarget(fadeOut, particle);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(fadeOut);

            storyboard.Completed += (s, e) =>
            {
                if (particleCanvas.Children.Contains(particle))
                {
                    particleCanvas.Children.Remove(particle);
                }
            };

            storyboard.Begin();
        }

        await Task.Delay(600);
    }

    #endregion
}
