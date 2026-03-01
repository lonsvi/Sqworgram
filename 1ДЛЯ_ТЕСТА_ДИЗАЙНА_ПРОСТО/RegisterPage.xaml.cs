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
            // Логика для плейсхолдеров больше не нужна, так как дизайн изменился
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
                this.NavigationService?.Navigate(new LoginPage());
            }
            else
            {
                MessageBox.Show("Пользователь с таким логином уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработка нажатия на «Назад»
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new LoginPage());
        }
    }
}
