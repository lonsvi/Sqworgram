using System;
using System.Windows;
using System.Windows.Media;

namespace _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО
{
    public static class ThemeManager
    {
        /// <summary>
        /// Applies the theme based on colors saved in application settings.
        /// </summary>
        public static void ApplyThemeFromSettings()
        {
            var settings = Properties.Settings.Default;
            ApplyTheme(settings.AccentColor, settings.BlobColor1, settings.BlobColor2);
        }

        /// <summary>
        /// Applies a new theme to the entire application in real-time.
        /// </summary>
        public static void ApplyTheme(string accentHex, string blob1Hex, string blob2Hex)
        {
            try
            {
                // Convert hex strings to Color objects
                var accentColor = (Color)ColorConverter.ConvertFromString(accentHex);
                var blob1Color = (Color)ColorConverter.ConvertFromString(blob1Hex);
                var blob2Color = (Color)ColorConverter.ConvertFromString(blob2Hex);

                // Get the application's central resource dictionary
                var themeDictionary = Application.Current.Resources;

                // Update the color values
                themeDictionary["AccentColorValue"] = accentColor;
                themeDictionary["BlobColor1Value"] = blob1Color;
                themeDictionary["BlobColor2Value"] = blob2Color;
            }
            catch (Exception)
            {
                // This is a fallback in case the saved colors are invalid.
                // It applies the default purple theme.
                var accentColor = (Color)ColorConverter.ConvertFromString("#FF6A359C");
                var blob1Color = (Color)ColorConverter.ConvertFromString("#FF4A00E0");
                var blob2Color = (Color)ColorConverter.ConvertFromString("#FF8E2DE2");

                var themeDictionary = Application.Current.Resources;

                themeDictionary["AccentColorValue"] = accentColor;
                themeDictionary["BlobColor1Value"] = blob1Color;
                themeDictionary["BlobColor2Value"] = blob2Color;
            }
        }
    }
}
