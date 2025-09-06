using Microsoft.Owin.Hosting; // Для WebApp.Start
using Owin; // Для IAppBuilder
using Microsoft.AspNet.SignalR; // Для SignalR
using System;
using System.Windows;
using System.Windows.Input;

namespace _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО
{
    public partial class MainWindow : Window
    {
        private IDisposable _signalRServer;

        public MainWindow()
        {
            InitializeComponent();
            StartSignalRServer();
            MainFrame.Navigate(new LoginPage());
        }

        private void StartSignalRServer()
        {
            string url = "http://localhost:5000";
            try
            {
                // Явно указываем использование HttpListener
                var options = new StartOptions(url)
                {
                    ServerFactory = "Microsoft.Owin.Host.HttpListener"
                };
                _signalRServer = WebApp.Start<Startup>(options);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска сервера: {ex.Message}");
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _signalRServer?.Dispose(); // Останавливаем сервер
        }
    }

    // Класс для настройки SignalR
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR(); // Маппим SignalR на /signalr
        }
    }
}