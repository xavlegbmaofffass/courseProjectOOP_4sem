using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FrontlineCardWarfare.Converters;

/// <summary>
/// Конвертирует относительный путь к изображению в pack URI для WPF.
/// </summary>
public class ImagePathConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string path && !string.IsNullOrWhiteSpace(path))
        {
            try
            {
                // Если путь уже pack URI, возвращаем как есть
                if (path.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
                {
                    return new BitmapImage(new Uri(path));
                }

                // Нормализуем путь (убираем обратные слеши)
                string normalizedPath = path.Replace("\\", "/");
                if (normalizedPath.StartsWith("/")) normalizedPath = normalizedPath.Substring(1);
                
                // Формируем правильный Pack URI для WPF
                // Теперь, когда мы используем Resource вместо Content, это более надежно
                string packUri = $"pack://application:,,,/{normalizedPath}";
                
                return new BitmapImage(new Uri(packUri, UriKind.RelativeOrAbsolute));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка ImagePathConverter: {ex.Message}");
                return null!;
            }
        }

        return null!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
