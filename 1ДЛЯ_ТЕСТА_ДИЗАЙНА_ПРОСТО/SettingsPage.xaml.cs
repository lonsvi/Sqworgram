using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО
{
    public partial class SettingsPage : Page
    {
        private readonly DatabaseHelper dbHelper;
        private readonly int userId;
        private readonly string userLogin;
        private string avatarUrl;

        public SettingsPage(int userId, string userLogin, string avatarUrl)
        {
            InitializeComponent();
            this.dbHelper = new DatabaseHelper();
            this.userId = userId;
            this.userLogin = userLogin;
            this.avatarUrl = avatarUrl;

          

            // Устанавливаем текущий URL в текстовое поле
            AvatarUrlTextBox.Text = avatarUrl ?? string.Empty;

            // Загружаем аватарку, если она есть
            if (!string.IsNullOrWhiteSpace(avatarUrl))
            {
                try
                {
                    Uri uri;
                    if (Uri.TryCreate(avatarUrl, UriKind.Absolute, out uri))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = uri;
                        bitmap.EndInit();
                        AvatarImage.Source = bitmap;
                    }
                    else
                    {
                        AvatarImage.Source = null;
                    }
                }
                catch
                {
                    AvatarImage.Source = null;
                }
            }
            else
            {
                AvatarImage.Source = null;
            }
        }

        private void SaveAvatarUrl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string newAvatarUrl = AvatarUrlTextBox.Text.Trim();

                // Проверяем, является ли введённая строка валидным URL
                if (!string.IsNullOrWhiteSpace(newAvatarUrl))
                {
                    if (!Uri.TryCreate(newAvatarUrl, UriKind.Absolute, out Uri uriResult) ||
                        (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                    {
                        MessageBox.Show("Пожалуйста, введите корректный URL (начинающийся с http:// или https://).",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Проверяем, доступно ли изображение по этому URL
                    try
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = uriResult;
                        bitmap.EndInit();
                        AvatarImage.Source = bitmap;
                    }
                    catch
                    {
                        MessageBox.Show("Не удалось загрузить изображение по указанному URL. Убедитесь, что ссылка ведёт на изображение.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    newAvatarUrl = null; // Если поле пустое, сбрасываем аватар
                    AvatarImage.Source = null;
                }

                // Сохраняем URL в базе данных
                avatarUrl = newAvatarUrl;
                dbHelper.UpdateAvatarUrl(userId, avatarUrl);

                // Обновляем аватар в настройках
                Properties.Settings.Default.AvatarUrl = avatarUrl;
                Properties.Settings.Default.Save();

                MessageBox.Show("Аватар успешно обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении аватара: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackToChats_Click(object sender, RoutedEventArgs e)
        {
            // Передаём обновлённое значение avatarUrl
            NavigationService?.Navigate(new ChatsPage(userId, userLogin, avatarUrl));
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {

            this.NavigationService.Navigate(new LoginPage());


        }
    }
}