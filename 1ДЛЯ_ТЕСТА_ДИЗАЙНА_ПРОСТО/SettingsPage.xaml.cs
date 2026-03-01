using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО
{
    public partial class SettingsPage : Page
    {
        private readonly int currentUserId;
        private readonly string currentUserLogin;
        private string currentAvatarUrl;
        private readonly DatabaseHelper dbHelper;

        public SettingsPage(int userId, string userLogin, string avatarUrl)
        {
            InitializeComponent();
            this.currentUserId = userId;
            this.currentUserLogin = userLogin;
            this.currentAvatarUrl = avatarUrl;
            this.dbHelper = new DatabaseHelper();

            Loaded += SettingsPage_Loaded;
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            UserLoginTextBlock.Text = currentUserLogin;
            AvatarUrlTextBox.Text = currentAvatarUrl;
            LoadAvatar();

            // Загружаем сохраненные настройки и устанавливаем состояние переключателей
            AnimationToggle.IsChecked = Properties.Settings.Default.AnimationsEnabled;
            GlassToggle.IsChecked = Properties.Settings.Default.GlassEffectEnabled;
        }

        private void LoadAvatar()
        {
            if (!string.IsNullOrEmpty(currentAvatarUrl))
            {
                var converter = new AvatarUrlToImageSourceConverter();
                AvatarImage.Source = (System.Windows.Media.ImageSource)converter.Convert(currentAvatarUrl, null, null, null);
            }
        }

        private void BackToChats_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.GoBack();
        }

        private void SaveAvatarUrl_Click(object sender, RoutedEventArgs e)
        {
            currentAvatarUrl = AvatarUrlTextBox.Text;
            dbHelper.UpdateAvatarUrl(currentUserId, currentAvatarUrl);
            LoadAvatar();
            // ИЗМЕНЕНИЕ: Используем новую систему уведомлений
            MainWindow.AppWindow?.ShowNotification("Аватар успешно обновлен!");
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();
            this.NavigationService?.Navigate(new LoginPage());
        }

        private void AnimationToggle_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggle && IsLoaded)
            {
                Properties.Settings.Default.AnimationsEnabled = toggle.IsChecked ?? false;
                Properties.Settings.Default.Save();
            }
        }

        private void GlassToggle_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggle && IsLoaded)
            {
                Properties.Settings.Default.GlassEffectEnabled = toggle.IsChecked ?? false;
                Properties.Settings.Default.Save();
            }
        }

        private void ThemeSettings_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ThemeSettingsPage());
        }
    }
}

