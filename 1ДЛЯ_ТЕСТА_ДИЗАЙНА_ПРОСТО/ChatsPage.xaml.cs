using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО
{
    public class MessageViewModel : INotifyPropertyChanged
    {
        private string _messageText;
        public int Id { get; set; }
        public string MessageText
        {
            get => _messageText;
            set
            {
                if (_messageText != value)
                {
                    _messageText = value;
                    OnPropertyChanged(nameof(MessageText));
                }
            }
        }
        public string AttachmentUrl { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsSentByMe { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TextMessageToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text && !string.IsNullOrEmpty(text))
            {
                bool isImageUrl = (text.StartsWith("http://") || text.StartsWith("https://")) &&
                                  (text.EndsWith(".png") || text.EndsWith(".jpg") || text.EndsWith(".jpeg"));
                return isImageUrl ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public partial class ChatsPage : Page
    {
        private readonly DatabaseHelper dbHelper;
        private readonly int currentUserId;
        private readonly string currentUserLogin;
        private string currentAvatarUrl;
        private int currentChatId = -1;
        private int otherUserId = -1;
        private readonly Dictionary<string, int> chatMapping = new Dictionary<string, int>();
        private DispatcherTimer statusTimer;
        private readonly TranslationService translationService;
        private bool isFavoriteMode = false;
        private DateTime lastMessageCheck = DateTime.MinValue;
        private string selectedChatLogin;
        private string selectedAttachmentPath = null;
        private readonly ImageUploader imageUploader;
        private bool isWindowActive = false;
        private bool isUnloading = false;
        private bool isPageLoaded = false;

        public ObservableCollection<MessageViewModel> Messages { get; set; }

        public ChatsPage(int userId, string userLogin, string avatarUrl = null)
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            currentUserId = userId;
            currentUserLogin = userLogin;
            currentAvatarUrl = avatarUrl;
            translationService = new TranslationService();
            imageUploader = new ImageUploader();

            Messages = new ObservableCollection<MessageViewModel>();
            MessagesPanel.ItemsSource = Messages;

            statusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            statusTimer.Tick += StatusTimer_Tick;

            Loaded += ChatsPage_Loaded;
            Unloaded += Page_Unloaded;
        }

        private async void ChatsPage_Loaded(object sender, RoutedEventArgs e)
        {
            isUnloading = false;
            ApplyPerformanceAndThemeSettings();

            var currentUser = await Task.Run(() => dbHelper.GetUserById(currentUserId));
            if (currentUser != null) { currentAvatarUrl = currentUser.AvatarUrl; }

            await LoadChatsAsync(true);

            if (currentChatId != -1)
            {
                await OpenChatAsync(selectedChatLogin);
            }

            statusTimer.Start();
            await Task.Run(() => dbHelper.SetUserOnline(currentUserId, true));

            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Activated += ParentWindow_Activated;
                parentWindow.Deactivated += ParentWindow_Deactivated;
                parentWindow.KeyDown += ParentWindow_KeyDown;
            }
            isPageLoaded = true;
        }

        // ИЗМЕНЕНИЕ: Метод теперь также применяет настройки темы
        private void ApplyPerformanceAndThemeSettings()
        {
            // Примечание: Анимированный фон иб будет показываться всегда (оптимизирован для производительности)
            // BackgroundAnimationGrid теперь содержит статичный кэшированный градиент, не требующий отключения

            bool glassEnabled = Properties.Settings.Default.GlassEffectEnabled;
            SolidColorBrush solidBackground = (SolidColorBrush)FindResource("SecondaryBackgroundColor");
            SolidColorBrush glassBackground = new SolidColorBrush(Color.FromArgb(0x99, 0x1C, 0x1C, 0x1C));
            SolidColorBrush glassBorder = new SolidColorBrush(Color.FromArgb(0x22, 0xFF, 0xFF, 0xFF));

            Action<Border> setStyle = (border) => {
                border.Background = glassEnabled ? glassBackground : solidBackground;
                border.BorderBrush = glassEnabled ? glassBorder : Brushes.Transparent;
                border.BorderThickness = glassEnabled ? new Thickness(1) : new Thickness(0);
            };

            setStyle(LeftPanelBorder);
            if (ChatArea.Visibility == Visibility.Visible)
            {
                setStyle(ChatHeaderBorder);
                setStyle(MessageInputPanelBorder);
            }

            // --- Управление темами ---
            try
            {
                // Акцентный цвет
                var accentColor = (Color)ColorConverter.ConvertFromString(Properties.Settings.Default.AccentColor);
                Resources["AccentColorValue"] = accentColor;
                Resources["AccentColor"] = new SolidColorBrush(accentColor);
                Resources["AccentTextColor"] = new SolidColorBrush(accentColor);

                // Цвета для фона
                var blobColor1 = (Color)ColorConverter.ConvertFromString(Properties.Settings.Default.BlobColor1);
                var blobColor2 = (Color)ColorConverter.ConvertFromString(Properties.Settings.Default.BlobColor2);
                Resources["BlobColor1Value"] = blobColor1;
                Resources["BlobColor2Value"] = blobColor2;
            }
            catch
            {
                // В случае ошибки (неверный формат цвета), ничего не делаем, останутся цвета по-умолчанию
            }
        }

        // ... (остальной код остается без изменений) ...
        private void ParentWindow_Activated(object sender, EventArgs e) => isWindowActive = true;
        private void ParentWindow_Deactivated(object sender, EventArgs e) => isWindowActive = false;
        private void ParentWindow_KeyDown(object sender, KeyEventArgs e) { if (isWindowActive && !MessageTextBox.IsFocused && !SearchTextBox.IsFocused && currentChatId != -1) { if (e.Key < Key.D0 || (e.Key > Key.Z && e.Key < Key.NumPad0)) return; MessageTextBox.Focus(); } }
        private async void StatusTimer_Tick(object sender, EventArgs e) { if (isUnloading || !IsLoaded) return; await LoadChatsAsync(false); if (isUnloading) return; if (currentChatId != -1 && otherUserId != -1) { var (isOnline, _, isTyping) = await Task.Run(() => dbHelper.GetUserStatus(otherUserId)); if (isUnloading) return; Dispatcher.Invoke(() => { if (isUnloading || !IsLoaded) return; TypingIndicator.Text = isTyping ? $"{selectedChatLogin} печатает..." : ""; TypingIndicator.Visibility = isTyping ? Visibility.Visible : Visibility.Collapsed; }); await CheckNewMessagesAsync(); } }
        private async Task LoadChatsAsync(bool fullReload) { var users = await Task.Run(() => dbHelper.GetUsers(currentUserId)); var favoriteChats = await Task.Run(() => dbHelper.GetFavoriteChats(currentUserId).ToHashSet()); string searchText = ""; await Dispatcher.InvokeAsync(() => { searchText = (SearchTextBox?.Text == "Поиск" ? "" : SearchTextBox?.Text.Trim().ToLower()) ?? ""; }); var chatItems = new List<object>(); foreach (var user in users.OrderBy(u => u.Login)) { int chatId = chatMapping.ContainsKey(user.Login) ? chatMapping[user.Login] : await Task.Run(() => dbHelper.GetOrCreateChat(currentUserId, user.Id)); chatMapping[user.Login] = chatId; bool shouldDisplay = (!isFavoriteMode || favoriteChats.Contains(chatId)) && (string.IsNullOrWhiteSpace(searchText) || user.Login.ToLower().Contains(searchText)); if (shouldDisplay) { chatItems.Add(new { user.Login, user.AvatarUrl, user.IsOnline, user.LastSeen, user.IsTyping }); } } if (isUnloading) return; Dispatcher.Invoke(() => { try { if (isUnloading || !IsLoaded || ChatListBox == null) return; object selectedItem = ChatListBox.SelectedItem; ChatListBox.ItemsSource = chatItems; if (selectedItem != null) { var currentLogin = ((dynamic)selectedItem).Login; ChatListBox.SelectedItem = ChatListBox.Items.Cast<object>().FirstOrDefault(i => ((dynamic)i).Login == currentLogin); } } catch { /* Игнорируем ошибку при выгрузке */ } }); }
        private async void ChatListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (ChatListBox.SelectedItem == null) return; string newSelectedChat = ((dynamic)ChatListBox.SelectedItem).Login; if (newSelectedChat == selectedChatLogin && ChatArea.Visibility == Visibility.Visible) return; selectedChatLogin = newSelectedChat; currentChatId = chatMapping[selectedChatLogin]; var selectedUser = (await Task.Run(() => dbHelper.GetUsers(currentUserId))).FirstOrDefault(u => u.Login == selectedChatLogin); if (selectedUser.Id == 0) return; otherUserId = selectedUser.Id; await OpenChatAsync(selectedChatLogin); }
        private async Task OpenChatAsync(string? chatName) { if (chatName == null) return; Dispatcher.Invoke(() => { NoChatSelectedTextBlock.Visibility = Visibility.Collapsed; ChatArea.Visibility = Visibility.Visible; Messages.Clear(); SelectedChatLoginTextBlock.Text = chatName; TranslationDirectionComboBox.SelectedIndex = 1; ApplyPerformanceAndThemeSettings(); }); var messages = await Task.Run(() => dbHelper.GetMessages(currentChatId)); foreach (var msg in messages.OrderBy(m => m.Timestamp)) { AddMessageToUI(msg.Id, msg.MessageText, msg.AttachmentUrl, msg.SenderId == currentUserId, msg.Timestamp); } lastMessageCheck = messages.Any() ? messages.Max(m => m.Timestamp) : DateTime.MinValue; await Dispatcher.InvokeAsync(() => ChatScrollViewer?.ScrollToEnd()); }
        private async Task CheckNewMessagesAsync() { if (currentChatId == -1 || isUnloading) return; var newMessages = await Task.Run(() => dbHelper.GetMessagesAfter(currentChatId, lastMessageCheck)); if (isUnloading || !newMessages.Any()) return; foreach (var msg in newMessages) { if (!Messages.Any(m => m.Id == msg.Id)) { AddMessageToUI(msg.Id, msg.MessageText, msg.AttachmentUrl, msg.SenderId == currentUserId, msg.Timestamp); } } lastMessageCheck = newMessages.Max(m => m.Timestamp); await Dispatcher.InvokeAsync(() => ChatScrollViewer?.ScrollToEnd()); }
        private void AddMessageToUI(int id, string? message, string? attachmentUrl, bool isSentByMe, DateTime timestamp) { if (isUnloading) return; Dispatcher.Invoke(() => { Messages.Add(new MessageViewModel { Id = id, MessageText = message, AttachmentUrl = attachmentUrl, IsSentByMe = isSentByMe, Timestamp = timestamp }); }); }
        private async void TranslateButton_Click(object sender, RoutedEventArgs e) { if (sender is Button button && button.Tag is MessageViewModel vm) { string originalText = vm.MessageText.Split(new[] { "\n\n[Перевод]:" }, StringSplitOptions.None)[0]; string direction = (TranslationDirectionComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "EN → RU"; string sourceLanguage = direction == "EN → RU" ? "en" : "ru"; string targetLanguage = direction == "EN → RU" ? "ru" : "en"; string translatedText = await translationService.TranslateTextAsync(originalText, sourceLanguage, targetLanguage); vm.MessageText = $"{originalText}\n\n[Перевод]: {translatedText}"; button.Visibility = Visibility.Collapsed; } }
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (sender is Image image && image.DataContext is MessageViewModel vm && !string.IsNullOrEmpty(vm.AttachmentUrl)) { OpenImage(vm.AttachmentUrl); } }
        private async void SendMessage_Click(object sender, RoutedEventArgs e) 
        { 
            if (currentChatId == -1) return; 
            string newMessage = MessageTextBox.Text; 
            string attachmentUrl = null; 
            if (!string.IsNullOrEmpty(selectedAttachmentPath)) 
            { 
                try 
                { 
                    attachmentUrl = await imageUploader.UploadImageAsync(selectedAttachmentPath); 
                } 
                catch (Exception ex) 
                { 
                    MessageBox.Show($"Не удалось загрузить изображение: {ex.Message}"); 
                    return; 
                } 
            } 
            if (string.IsNullOrWhiteSpace(newMessage) && string.IsNullOrEmpty(attachmentUrl)) return; 
            DateTime timestamp = DateTime.Now; 
            int messageId = await Task.Run(() => dbHelper.SaveMessageAndGetId(currentChatId, currentUserId, newMessage, attachmentUrl, timestamp)); 
            AddMessageToUI(messageId, newMessage, attachmentUrl, true, timestamp); 
            Dispatcher.Invoke(() => 
            { 
                MessageTextBox.Clear(); 
                selectedAttachmentPath = null; 
                AttachmentPreviewContainer.Visibility = Visibility.Collapsed;
                AttachmentPreviewImage.Source = null;
                ChatScrollViewer.ScrollToEnd(); 
            }); 
            lastMessageCheck = timestamp; 
        }
        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift) { SendMessage_Click(sender, e); e.Handled = true; } }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) { if (!isPageLoaded) return; _ = LoadChatsAsync(true); }
        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e) { var textBox = sender as TextBox; if (textBox.Text == "Поиск") { textBox.Text = ""; textBox.Foreground = (Brush)FindResource("PrimaryTextColor"); } }
        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e) { var textBox = sender as TextBox; if (string.IsNullOrWhiteSpace(textBox.Text)) { textBox.Text = "Поиск"; textBox.Foreground = (Brush)FindResource("SecondaryTextColor"); } }
        private void MessageTextBox_TextChanged(object sender, TextChangedEventArgs e) { string text = MessageTextBox.Text; _ = Task.Run(() => dbHelper.SetUserTyping(currentUserId, !string.IsNullOrEmpty(text))); }
        private void AttachFileButton_Click(object sender, RoutedEventArgs e) 
        { 
            var openFileDialog = new Microsoft.Win32.OpenFileDialog 
            { 
                Filter = "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*", 
                Multiselect = false 
            }; 
            if (openFileDialog.ShowDialog() == true) 
            { 
                selectedAttachmentPath = openFileDialog.FileName;
                UpdateAttachmentPreview();
            } 
        }

        private void UpdateAttachmentPreview()
        {
            if (string.IsNullOrEmpty(selectedAttachmentPath))
            {
                AttachmentPreviewContainer.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                var fileInfo = new System.IO.FileInfo(selectedAttachmentPath);
                
                // Отображение превью изображения
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(selectedAttachmentPath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                
                AttachmentPreviewImage.Source = bitmap;
                
                // Информация о файле
                AttachmentFileName.Text = System.IO.Path.GetFileName(selectedAttachmentPath);
                double fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);
                AttachmentFileSize.Text = fileSizeMB > 0.01 ? $"{fileSizeMB:F2} МБ" : $"{fileInfo.Length / 1024.0:F1} КБ";
                
                AttachmentPreviewContainer.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить превью: {ex.Message}");
                AttachmentPreviewContainer.Visibility = Visibility.Collapsed;
            }
        }

        private void RemoveAttachmentButton_Click(object sender, RoutedEventArgs e)
        {
            selectedAttachmentPath = null;
            AttachmentPreviewContainer.Visibility = Visibility.Collapsed;
            AttachmentPreviewImage.Source = null;
        }

        private async void DownloadFile(string fileUrl) { var saveFileDialog = new Microsoft.Win32.SaveFileDialog { FileName = System.IO.Path.GetFileName(fileUrl), Filter = "All files (*.*)|*.*" }; if (saveFileDialog.ShowDialog() == true) { try { using (var client = new System.Net.Http.HttpClient()) { var response = await client.GetAsync(fileUrl); response.EnsureSuccessStatusCode(); using (var contentStream = await response.Content.ReadAsStreamAsync()) { using (var fileStream = System.IO.File.Create(saveFileDialog.FileName)) { await contentStream.CopyToAsync(fileStream); } } } MessageBox.Show("Файл успешно сохранён!"); } catch (Exception ex) { MessageBox.Show($"Ошибка при скачивании файла: {ex.Message}"); } } }
        private void OpenImage(string imageUrl) { try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = imageUrl, UseShellExecute = true }); } catch (Exception ex) { MessageBox.Show($"Не удалось открыть изображение: {ex.Message}"); } }
        private void SettingsButton_Click(object sender, RoutedEventArgs e) { NavigationService?.Navigate(new SettingsPage(currentUserId, currentUserLogin, currentAvatarUrl)); }
        private async void ChatListBox_MouseRightButtonUp(object sender, MouseButtonEventArgs e) { if (ChatListBox.SelectedItem != null) { string selectedChat = ((dynamic)ChatListBox.SelectedItem).Login; int chatId = chatMapping[selectedChat]; var favoriteChats = await Task.Run(() => dbHelper.GetFavoriteChats(currentUserId).ToHashSet()); var contextMenu = new ContextMenu(); var favoriteItem = new MenuItem { Header = favoriteChats.Contains(chatId) ? "Убрать из избранного" : "Добавить в избранное" }; favoriteItem.Click += async (s, args) => { if (favoriteChats.Contains(chatId)) { await Task.Run(() => dbHelper.RemoveFavoriteChat(currentUserId, chatId)); } else { await Task.Run(() => dbHelper.AddFavoriteChat(currentUserId, chatId)); } await LoadChatsAsync(true); }; contextMenu.Items.Add(favoriteItem); contextMenu.IsOpen = true; } }
        private async void ToggleFavoriteMode_Click(object sender, RoutedEventArgs e) { isFavoriteMode = !isFavoriteMode; await LoadChatsAsync(true); }
        private void LogoutButton_Click(object sender, RoutedEventArgs e) { isUnloading = true; statusTimer?.Stop(); Task.Run(() => { dbHelper.SetUserOnline(currentUserId, false); dbHelper.SetUserTyping(currentUserId, false); }); NavigationService?.GoBack(); }
        private void SendMessageButton_Click(object sender, RoutedEventArgs e) { SendMessage_Click(sender, e); }
        private void Page_Unloaded(object sender, RoutedEventArgs e) { isUnloading = true; statusTimer?.Stop(); Task.Run(() => { dbHelper.SetUserOnline(currentUserId, false); dbHelper.SetUserTyping(currentUserId, false); }); Window parentWindow = Window.GetWindow(this); if (parentWindow != null) { parentWindow.Activated -= ParentWindow_Activated; parentWindow.Deactivated -= ParentWindow_Deactivated; parentWindow.KeyDown -= ParentWindow_KeyDown; } }

        private void ChatItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid grid)
            {
                // Получаем данные из DataContext
                var dataContext = grid.DataContext;
                if (dataContext != null)
                {
                    // Используем отражение для получения свойств
                    var loginProperty = dataContext.GetType().GetProperty("Login");
                    var avatarUrlProperty = dataContext.GetType().GetProperty("AvatarUrl");
                    var isOnlineProperty = dataContext.GetType().GetProperty("IsOnline");
                    var isTypingProperty = dataContext.GetType().GetProperty("IsTyping");

                    if (loginProperty != null)
                    {
                        string login = loginProperty.GetValue(dataContext)?.ToString() ?? "";
                        string avatarUrl = avatarUrlProperty?.GetValue(dataContext)?.ToString() ?? "";
                        bool isOnline = (bool)(isOnlineProperty?.GetValue(dataContext) ?? false);
                        bool isTyping = (bool)(isTypingProperty?.GetValue(dataContext) ?? false);

                        ShowFriendProfile(login, avatarUrl, isOnline, isTyping);
                    }
                }
            }
        }

        private void ShowFriendProfile(string login, string avatarUrl, bool isOnline, bool isTyping)
        {
            try
            {
                FriendLoginTextBlock.Text = login;
                
                // Загрузить аватар
                if (!string.IsNullOrEmpty(avatarUrl))
                {
                    var converter = new AvatarUrlToImageSourceConverter();
                    FriendAvatarImage.Source = (ImageSource?)converter.Convert(avatarUrl, null, null, null);
                }

                // Установить статус
                if (isTyping)
                {
                    FriendStatusTextBlock.Text = "печатает...";
                    FriendStatusTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(156, 39, 176)); // Фиолетовый
                }
                else if (isOnline)
                {
                    FriendStatusTextBlock.Text = "Online";
                    FriendStatusTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Зелёный
                }
                else
                {
                    FriendStatusTextBlock.Text = "Offline";
                    FriendStatusTextBlock.Foreground = (Brush?)TryFindResource("SecondaryTextColor") ?? new SolidColorBrush(Colors.Gray);
                }

                // Открыть Popup
                FriendProfilePopup.IsOpen = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии профиля: {ex.Message}");
            }
        }

        private void ProfileBackButton_Click(object sender, RoutedEventArgs e)
        {
            FriendProfilePopup.IsOpen = false;
        }
    }
}


