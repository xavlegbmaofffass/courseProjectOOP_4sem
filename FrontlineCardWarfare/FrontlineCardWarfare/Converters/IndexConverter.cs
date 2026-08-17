using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace FrontlineCardWarfare.Converters;

/// <summary>
/// Возвращает порядковый номер элемента в ListView (начиная с 1).
/// </summary>
public class IndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ListViewItem item)
            return string.Empty;

        var itemsControl = ItemsControl.ItemsControlFromItemContainer(item);
        if (itemsControl == null)
            return string.Empty;

        var index = itemsControl.ItemContainerGenerator.IndexFromContainer(item);
        return index >= 0 ? (index + 1).ToString() : string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
