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

            // Подписываемся на события изменения текста и фокуса
            LoginTextBox.TextChanged += LoginTextBox_TextChanged;
            LoginTextBox.GotFocus += LoginTextBox_GotFocus;
            LoginTextBox.LostFocus += LoginTextBox_LostFocus;

            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
            PasswordBox.GotFocus += PasswordBox_GotFocus;
            PasswordBox.LostFocus += PasswordBox_LostFocus;

          
        }

        // Обработчики для TextBox (Логин)
        private void LoginTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLoginPlaceholderVisibility();
        }

        private void LoginTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdateLoginPlaceholderVisibility();
        }

        private void LoginTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateLoginPlaceholderVisibility();
        }

        private void UpdateLoginPlaceholderVisibility()
        {
            LoginPlaceholder.Visibility = string.IsNullOrEmpty(LoginTextBox.Text) && !LoginTextBox.IsKeyboardFocused
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // Обработчики для PasswordBox (Пароль)
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePasswordPlaceholderVisibility();
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdatePasswordPlaceholderVisibility();
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePasswordPlaceholderVisibility();
        }

        private void UpdatePasswordPlaceholderVisibility()
        {
            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordBox.Password) && !PasswordBox.IsKeyboardFocused
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // Обработка нажатия на «Войти»
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;

            if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
            {
                var (userId, userLogin, avatarUrl) = dbHelper.LoginUser(login, password); // Исправляем деконструкцию
                if (userId.HasValue)
                {
                    // Сохраняем данные пользователя в настройки
                    Properties.Settings.Default.UserId = userId.Value;
                    Properties.Settings.Default.UserLogin = userLogin;
                    Properties.Settings.Default.AvatarUrl = avatarUrl ?? string.Empty;
                    Properties.Settings.Default.SaveSession = true; // По умолчанию сохраняем сессию
                    Properties.Settings.Default.Save();

                    this.NavigationService.Navigate(new ChatsPage(userId.Value, userLogin, avatarUrl));
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
            this.NavigationService.Navigate(new RegisterPage());
        }
    }
}