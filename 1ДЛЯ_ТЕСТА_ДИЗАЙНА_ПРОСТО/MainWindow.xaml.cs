using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media; // <-- Убедись, что эта строка есть

namespace _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО
{
    public partial class MainWindow : Window
    {
        public static MainWindow AppWindow { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            // ФИКС: Ограничиваем максимальный размер окна размером рабочей области (без панели задач)
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;

            ThemeManager.ApplyThemeFromSettings();
            AppWindow = this;
            MainFrame.Navigate(new LoginPage());
        }

        // ИЗМЕНЕНИЕ: Метод теперь создает и передает цвет акцента из настроек
        public void ShowNotification(string message, bool isError = false)
        {
            try
            {
                // 1. Получаем цвет акцента из настроек темы
                string accentColorHex = Properties.Settings.Default.AccentColor;

                // 2. Создаем кисть (Brush) из этого цвета
                var accentBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(accentColorHex));
                accentBrush.Freeze(); // Оптимизация производительности

                // 3. Передаем кисть в конструктор уведомления
                NotificationArea.Children.Add(new NotificationControl(message, isError, accentBrush));
            }
            catch (Exception)
            {
                // На случай, если в настройках сохранен некорректный цвет,
                // показываем уведомление с цветом по умолчанию
                var defaultBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6A359C"));
                defaultBrush.Freeze();
                NotificationArea.Children.Add(new NotificationControl(message, isError, defaultBrush));
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                // 1. Убираем скругления, чтобы углы были четкими
                MainWindowBorder.CornerRadius = new CornerRadius(0);

                // 2. ФИКС "ЗАЕЗДА" ЗА ЭКРАН: 
                // Когда окно развернуто, Windows добавляет невидимую рамку (обычно 7-8 пикселей).
                // Мы компенсируем её внутренним отступом самого Border.
                MainWindowBorder.Padding = new Thickness(7);
                MainWindowBorder.BorderThickness = new Thickness(0);
            }
            else
            {
                // Возвращаем как было
                MainWindowBorder.CornerRadius = new CornerRadius(12);
                MainWindowBorder.Padding = new Thickness(0);
                MainWindowBorder.BorderThickness = new Thickness(1);
            }
        }
    }
}

