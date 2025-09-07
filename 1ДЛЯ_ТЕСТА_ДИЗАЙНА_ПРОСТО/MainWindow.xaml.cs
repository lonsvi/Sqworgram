using System;
using System.Windows;
using System.Windows.Input;

namespace _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new LoginPage());
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
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // НОВЫЙ МЕТОД: Исправляет баг с полноэкранным режимом
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                // Убираем скругление и добавляем отступ, чтобы окно не залезало на панель задач
                MainWindowBorder.CornerRadius = new CornerRadius(0);
                MainWindowBorder.Margin = new Thickness(8);
            }
            else
            {
                // Возвращаем скругление и убираем отступ
                MainWindowBorder.CornerRadius = new CornerRadius(10);
                MainWindowBorder.Margin = new Thickness(0);
            }
        }
    }
}
