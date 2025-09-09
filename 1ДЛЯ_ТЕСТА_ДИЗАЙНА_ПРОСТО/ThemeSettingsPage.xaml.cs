using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО
{
    public partial class ThemeSettingsPage : Page
    {
        public ThemeSettingsPage()
        {
            InitializeComponent();
            Loaded += ThemeSettingsPage_Loaded;
        }

        private void ThemeSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCurrentTheme();
        }

        private void LoadCurrentTheme()
        {
            AccentColorTextBox.Text = Properties.Settings.Default.AccentColor;
            BlobColor1TextBox.Text = Properties.Settings.Default.BlobColor1;
            BlobColor2TextBox.Text = Properties.Settings.Default.BlobColor2;
            UpdateColorPreviews();
        }

        private void UpdateColorPreviews()
        {
            try
            {
                AccentColorPreview.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(AccentColorTextBox.Text));
                BlobColor1Preview.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BlobColor1TextBox.Text));
                BlobColor2Preview.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BlobColor2TextBox.Text));
            }
            catch
            {
                // Игнорируем ошибки, если пользователь вводит некорректный hex
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void PresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string colors)
            {
                var colorArray = colors.Split(';');
                if (colorArray.Length == 3)
                {
                    ApplyAndSaveTheme(colorArray[0], colorArray[1], colorArray[2]);
                }
            }
        }

        private void SaveCustomTheme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ColorConverter.ConvertFromString(AccentColorTextBox.Text);
                ColorConverter.ConvertFromString(BlobColor1TextBox.Text);
                ColorConverter.ConvertFromString(BlobColor2TextBox.Text);

                ApplyAndSaveTheme(AccentColorTextBox.Text, BlobColor1TextBox.Text, BlobColor2TextBox.Text);
                MainWindow.AppWindow?.ShowNotification("Ваша тема сохранена! Чтобы применилась полностью, перезайдите!");
            }
            catch
            {
                MainWindow.AppWindow?.ShowNotification("Один или несколько кодов цвета введены неверно. Используйте hex-формат, например, #FF6A359C.", true);
            }
        }

        private void ApplyAndSaveTheme(string accent, string blob1, string blob2)
        {
            AccentColorTextBox.Text = accent;
            BlobColor1TextBox.Text = blob1;
            BlobColor2TextBox.Text = blob2;

            UpdateColorPreviews();

            Properties.Settings.Default.AccentColor = accent;
            Properties.Settings.Default.BlobColor1 = blob1;
            Properties.Settings.Default.BlobColor2 = blob2;
            Properties.Settings.Default.Save();
            ThemeManager.ApplyTheme(accent, blob1, blob2);
        }

        private void ColorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateColorPreviews();
        }
    }
}

