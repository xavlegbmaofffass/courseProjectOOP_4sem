using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.ViewModels;

namespace FrontlineCardWarfare.Views;

/// <summary>
/// Interaction logic for DeckBuilderView.xaml
/// </summary>
public partial class DeckBuilderView : UserControl
{
    public DeckBuilderView()
    {
        InitializeComponent();
    }

    private void AvailableCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is DeckBuilderViewModel vm &&
            sender is Border border &&
            border.DataContext is Card card)
        {
            vm.AddCardToDeckCommand.Execute(card);
        }
    }
}