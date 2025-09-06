using System.Windows;
using System.Windows.Controls;

namespace _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО
{
    public partial class RegisterPage : Page
    {
        private readonly DatabaseHelper dbHelper;

        public RegisterPage()
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

            ConfirmPasswordBox.PasswordChanged += ConfirmPasswordBox_PasswordChanged;
            ConfirmPasswordBox.GotFocus += ConfirmPasswordBox_GotFocus;
            ConfirmPasswordBox.LostFocus += ConfirmPasswordBox_LostFocus;
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

        // Обработчики для ConfirmPasswordBox (Подтверждение пароля)
        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateConfirmPasswordPlaceholderVisibility();
        }

        private void ConfirmPasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdateConfirmPasswordPlaceholderVisibility();
        }

        private void ConfirmPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateConfirmPasswordPlaceholderVisibility();
        }

        private void UpdateConfirmPasswordPlaceholderVisibility()
        {
            ConfirmPasswordPlaceholder.Visibility = string.IsNullOrEmpty(ConfirmPasswordBox.Password) && !ConfirmPasswordBox.IsKeyboardFocused
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // Обработка нажатия на «Зарегистрироваться»
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Регистрация пользователя
            bool success = dbHelper.RegisterUser(login, password);
            if (success)
            {
                MessageBox.Show("Регистрация успешна! Теперь вы можете войти.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.NavigationService.Navigate(new LoginPage());
            }
            else
            {
                MessageBox.Show("Пользователь с таким логином уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработка нажатия на «Назад»
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LoginPage());
        }
    }
}