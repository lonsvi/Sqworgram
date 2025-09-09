using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
// ИЗМЕНЕНИЕ: System.Windows.Threading больше не нужен
// using System.Windows.Threading; 

namespace _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО
{
    public partial class NotificationControl : UserControl
    {
        // ИЗМЕНЕНИЕ: DispatcherTimer полностью удален, так как мы используем анимацию
        // private DispatcherTimer _lifeTimer;

        public NotificationControl(string message, bool isError, Brush accentBrush)
        {
            InitializeComponent();
            MessageText.Text = message;

            this.Foreground = accentBrush;

            if (isError)
            {
                IconSuccess.Visibility = Visibility.Collapsed;
                IconError.Visibility = Visibility.Visible;
                NotificationBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#80F44336"));
            }
            else
            {
                IconSuccess.Visibility = Visibility.Visible;
                IconError.Visibility = Visibility.Collapsed;
                var accentWithOpacity = accentBrush.Clone();
                accentWithOpacity.Opacity = 0.5;
                NotificationBorder.BorderBrush = accentWithOpacity;
            }

            // ИЗМЕНЕНИЕ: Инициализация таймера удалена
        }

        // ИЗМЕНЕНИЕ: Метод таймера удален
        // private void LifeTimer_Tick(object sender, EventArgs e) { ... }

        public void Close()
        {
            // ИЗМЕНЕНИЕ: Останавливаем все анимации на этом контроле перед его закрытием
            // Это предотвратит любые конфликты, если пользователь нажмет кнопку закрытия вручную
            this.BeginAnimation(OpacityProperty, null);
            LifeProgressBar.BeginAnimation(ProgressBar.ValueProperty, null);

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
            // ИЗМЕНЕНИЕ: Вместо запуска таймера, мы теперь запускаем плавную анимацию
            LifeProgressBar.Maximum = 100;
            LifeProgressBar.Value = 100;

            // Создаем анимацию, которая будет изменять значение ProgressBar от 100 до 0 за 5 секунд
            var lifeAnimation = new DoubleAnimation
            {
                From = 100,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(5))
            };

            // Когда анимация завершится, вызываем метод Close(), чтобы уведомление исчезло
            lifeAnimation.Completed += (s, a) => Close();

            // Применяем и запускаем анимацию
            LifeProgressBar.BeginAnimation(ProgressBar.ValueProperty, lifeAnimation);
        }
    }
}
