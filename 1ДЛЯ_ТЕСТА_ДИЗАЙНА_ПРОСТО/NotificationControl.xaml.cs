using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО
{
    public partial class NotificationControl : UserControl
    {
        private DispatcherTimer _lifeTimer;

        // ИЗМЕНЕНИЕ: Конструктор теперь принимает кисть для цвета акцента
        public NotificationControl(string message, bool isError, Brush accentBrush)
        {
            InitializeComponent();
            MessageText.Text = message;

            // Устанавливаем Foreground для всего UserControl. 
            // XAML будет использовать этот цвет для иконки и прогресс-бара.
            this.Foreground = accentBrush;

            if (isError)
            {
                IconSuccess.Visibility = Visibility.Collapsed;
                IconError.Visibility = Visibility.Visible;
                // Для ошибки оставляем красный цвет
                NotificationBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#80F44336"));
            }
            else
            {
                IconSuccess.Visibility = Visibility.Visible;
                IconError.Visibility = Visibility.Collapsed;
                // ИЗМЕНЕНИЕ: Для успеха используем цвет акцента с прозрачностью
                var accentWithOpacity = accentBrush.Clone();
                accentWithOpacity.Opacity = 0.5;
                NotificationBorder.BorderBrush = accentWithOpacity;
            }

            _lifeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _lifeTimer.Tick += LifeTimer_Tick;
        }

        private void LifeTimer_Tick(object sender, EventArgs e)
        {
            LifeProgressBar.Value -= 1;
            if (LifeProgressBar.Value <= 0)
            {
                Close();
            }
        }

        public void Close()
        {
            _lifeTimer.Stop();
            var fadeOutAnimation = new DoubleAnimation { From = 1, To = 0, Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
            fadeOutAnimation.Completed += (s, a) =>
            {
                (this.Parent as Panel)?.Children.Remove(this);
            };
            this.BeginAnimation(OpacityProperty, fadeOutAnimation);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LifeProgressBar.Maximum = 100; // 50ms * 100 = 5 seconds
            LifeProgressBar.Value = 100;
            _lifeTimer.Start();
        }
    }
}
