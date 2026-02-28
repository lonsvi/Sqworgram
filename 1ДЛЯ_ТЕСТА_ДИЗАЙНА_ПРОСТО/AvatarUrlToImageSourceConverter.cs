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
            Logger.LogInfo($"Converting AvatarUrl: {url}");

            if (string.IsNullOrWhiteSpace(url))
            {
                Logger.LogWarning("AvatarUrl is null or empty.");
                return null;
            }

            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(url, UriKind.Absolute);
                bitmap.EndInit();
                Logger.LogInfo($"Successfully loaded image from {url}");
                return bitmap;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to load image from {url}", ex);
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}