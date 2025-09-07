using System.Windows;
using System.Windows.Controls;

namespace _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО
{
    public partial class LoginPage : Page
    {
        private readonly DatabaseHelper dbHelper;

        public LoginPage()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            // Вся логика для плейсхолдеров удалена, так как дизайн теперь использует метки над полями
        }

        // Обработка нажатия на «Войти»
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;

            if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
            {
                var (userId, userLogin, avatarUrl) = dbHelper.LoginUser(login, password);
                if (userId.HasValue)
                {
                    // Сохраняем данные пользователя в настройки
                    Properties.Settings.Default.UserId = userId.Value;
                    Properties.Settings.Default.UserLogin = userLogin;
                    Properties.Settings.Default.AvatarUrl = avatarUrl ?? string.Empty;
                    Properties.Settings.Default.SaveSession = true;
                    Properties.Settings.Default.Save();

                    this.NavigationService?.Navigate(new ChatsPage(userId.Value, userLogin, avatarUrl));
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработка нажатия на «Зарегистрироваться»
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new RegisterPage());
        }
    }
}

