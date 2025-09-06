using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО
{
    public class AvatarUrlToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string url = value as string;
            Console.WriteLine($"Converting AvatarUrl: {url}");

            if (string.IsNullOrWhiteSpace(url))
            {
                Console.WriteLine("AvatarUrl is null or empty.");
                return null;
            }

            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(url, UriKind.Absolute);
                bitmap.EndInit();
                Console.WriteLine($"Successfully loaded image from {url}");
                return bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load image from {url}. Error: {ex.Message}");
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}