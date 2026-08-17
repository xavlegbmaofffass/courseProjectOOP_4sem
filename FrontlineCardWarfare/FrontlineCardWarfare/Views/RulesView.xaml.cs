using System.Windows.Controls;
using System.Windows;

namespace FrontlineCardWarfare.Views;

/// <summary>
/// Логика взаимодействия для RulesView.xaml
/// </summary>
public partial class RulesView : UserControl
{
    public RulesView()
    {
        InitializeComponent();
        Tab1Content.Visibility = Visibility.Visible;
        Tab1Btn.Style = (Style)FindResource("ActiveTabButton");
        Tab2Btn.Style = (Style)FindResource("TabButton");
        Tab3Btn.Style = (Style)FindResource("TabButton");
    }

    private void Tab1_Click(object? sender, RoutedEventArgs e)
    {
        Tab1Content.Visibility = Visibility.Visible;
        Tab2Content.Visibility = Visibility.Collapsed;
        Tab3Content.Visibility = Visibility.Collapsed;

        Tab1Btn.Style = (Style)FindResource("ActiveTabButton");
        Tab2Btn.Style = (Style)FindResource("TabButton");
        Tab3Btn.Style = (Style)FindResource("TabButton");
    }

    private void Tab2_Click(object? sender, RoutedEventArgs e)
    {
        Tab1Content.Visibility = Visibility.Collapsed;
        Tab2Content.Visibility = Visibility.Visible;
        Tab3Content.Visibility = Visibility.Collapsed;

        Tab1Btn.Style = (Style)FindResource("TabButton");
        Tab2Btn.Style = (Style)FindResource("ActiveTabButton");
        Tab3Btn.Style = (Style)FindResource("TabButton");
    }

    private void Tab3_Click(object? sender, RoutedEventArgs e)
    {
        Tab1Content.Visibility = Visibility.Collapsed;
        Tab2Content.Visibility = Visibility.Collapsed;
        Tab3Content.Visibility = Visibility.Visible;

        Tab1Btn.Style = (Style)FindResource("TabButton");
        Tab2Btn.Style = (Style)FindResource("TabButton");
        Tab3Btn.Style = (Style)FindResource("ActiveTabButton");
    }
}