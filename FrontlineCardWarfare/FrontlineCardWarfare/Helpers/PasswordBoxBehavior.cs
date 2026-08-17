using System.Windows;
using System.Windows.Controls;

namespace FrontlineCardWarfare.Helpers;

/// <summary>
/// Attached behaviour для привязки PasswordBox к свойству ViewModel.
/// </summary>
public static class PasswordBoxBehavior
{
    public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.RegisterAttached(
            "Password",
            typeof(string),
            typeof(PasswordBoxBehavior),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordPropertyChanged));

    public static readonly DependencyProperty BindPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BindPassword",
            typeof(bool),
            typeof(PasswordBoxBehavior),
            new PropertyMetadata(false, OnBindPasswordChanged));

    private static readonly DependencyProperty IsUpdatingProperty =
        DependencyProperty.RegisterAttached(
            "IsUpdating",
            typeof(bool),
            typeof(PasswordBoxBehavior),
            new PropertyMetadata(false));

    private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PasswordBox box && !GetIsUpdating(box))
        {
            var newPassword = e.NewValue as string ?? string.Empty;
            if (box.Password != newPassword)
            {
                box.Password = newPassword;
            }
        }
    }

    private static void OnBindPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PasswordBox box)
        {
            if ((bool)e.OldValue)
                box.PasswordChanged -= OnPasswordBoxPasswordChanged;
            if ((bool)e.NewValue)
                box.PasswordChanged += OnPasswordBoxPasswordChanged;
        }
    }

    private static void OnPasswordBoxPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox box)
        {
            SetIsUpdating(box, true);
            SetPassword(box, box.Password);
            SetIsUpdating(box, false);
        }
    }

    private static void SetIsUpdating(PasswordBox box, bool value)
    {
        box.SetValue(IsUpdatingProperty, value);
    }

    private static bool GetIsUpdating(PasswordBox box)
    {
        return (bool)box.GetValue(IsUpdatingProperty);
    }

    public static void SetBindPassword(DependencyObject element, bool value)
    {
        element.SetValue(BindPasswordProperty, value);
    }

    public static bool GetBindPassword(DependencyObject element)
    {
        return (bool)element.GetValue(BindPasswordProperty);
    }

    public static void SetPassword(DependencyObject element, string value)
    {
        element.SetValue(PasswordProperty, value);
    }

    public static string GetPassword(DependencyObject element)
    {
        return (string)element.GetValue(PasswordProperty);
    }
}