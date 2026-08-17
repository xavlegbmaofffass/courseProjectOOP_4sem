using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.ViewModels;

namespace FrontlineCardWarfare.Views;

/// <summary>
/// Interaction logic for CollectionView.xaml
/// </summary>
public partial class CollectionView : UserControl
{
    public CollectionView()
    {
        InitializeComponent();
    }

    private void CardBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is CollectionViewModel vm &&
            sender is Border border &&
            border.DataContext is Card card)
        {
            vm.ViewCardDetailsCommand.Execute(card);
        }
    }
}