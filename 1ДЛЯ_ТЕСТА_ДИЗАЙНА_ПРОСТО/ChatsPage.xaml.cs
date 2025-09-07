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

        public event PropertyChangedEventHandler PropertyChanged;
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
            isUnloading = false; // ИСПРАВЛЕНИЕ: Сбрасываем флаг при возвращении на страницу
            ApplyPerformanceSettings();

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

        private void ApplyPerformanceSettings()
        {
            bool animationsEnabled = Properties.Settings.Default.AnimationsEnabled;
            BackgroundAnimationGrid.Visibility = animationsEnabled ? Visibility.Visible : Visibility.Collapsed;

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
        }

        private void ParentWindow_Activated(object sender, EventArgs e) => isWindowActive = true;
        private void ParentWindow_Deactivated(object sender, EventArgs e) => isWindowActive = false;
        private void ParentWindow_KeyDown(object sender, KeyEventArgs e) { if (isWindowActive && !MessageTextBox.IsFocused && !SearchTextBox.IsFocused && currentChatId != -1) { if (e.Key < Key.D0 || (e.Key > Key.Z && e.Key < Key.NumPad0)) return; MessageTextBox.Focus(); } }
        private async void StatusTimer_Tick(object sender, EventArgs e) { if (isUnloading || !IsLoaded) return; await LoadChatsAsync(false); if (isUnloading) return; if (currentChatId != -1 && otherUserId != -1) { var (isOnline, _, isTyping) = await Task.Run(() => dbHelper.GetUserStatus(otherUserId)); if (isUnloading) return; Dispatcher.Invoke(() => { if (isUnloading || !IsLoaded) return; TypingIndicator.Text = isTyping ? $"{selectedChatLogin} печатает..." : ""; TypingIndicator.Visibility = isTyping ? Visibility.Visible : Visibility.Collapsed; }); await CheckNewMessagesAsync(); } }
        private async Task LoadChatsAsync(bool fullReload) { var users = await Task.Run(() => dbHelper.GetUsers(currentUserId)); var favoriteChats = await Task.Run(() => dbHelper.GetFavoriteChats(currentUserId).ToHashSet()); string searchText = ""; await Dispatcher.InvokeAsync(() => { searchText = (SearchTextBox?.Text == "Поиск" ? "" : SearchTextBox?.Text.Trim().ToLower()) ?? ""; }); var chatItems = new List<object>(); foreach (var user in users.OrderBy(u => u.Login)) { int chatId = chatMapping.ContainsKey(user.Login) ? chatMapping[user.Login] : await Task.Run(() => dbHelper.GetOrCreateChat(currentUserId, user.Id)); chatMapping[user.Login] = chatId; bool shouldDisplay = (!isFavoriteMode || favoriteChats.Contains(chatId)) && (string.IsNullOrWhiteSpace(searchText) || user.Login.ToLower().Contains(searchText)); if (shouldDisplay) { chatItems.Add(new { user.Login, user.AvatarUrl, user.IsOnline, user.LastSeen, user.IsTyping }); } } if (isUnloading) return; Dispatcher.Invoke(() => { try { if (isUnloading || !IsLoaded || ChatListBox == null) return; object selectedItem = ChatListBox.SelectedItem; ChatListBox.ItemsSource = chatItems; if (selectedItem != null) { var currentLogin = ((dynamic)selectedItem).Login; ChatListBox.SelectedItem = ChatListBox.Items.Cast<object>().FirstOrDefault(i => ((dynamic)i).Login == currentLogin); } } catch { /* Игнорируем ошибку при выгрузке */ } }); }
        private async void ChatListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (ChatListBox.SelectedItem == null) return; string newSelectedChat = ((dynamic)ChatListBox.SelectedItem).Login; if (newSelectedChat == selectedChatLogin && ChatArea.Visibility == Visibility.Visible) return; selectedChatLogin = newSelectedChat; currentChatId = chatMapping[selectedChatLogin]; var selectedUser = (await Task.Run(() => dbHelper.GetUsers(currentUserId))).FirstOrDefault(u => u.Login == selectedChatLogin); if (selectedUser.Id == 0) return; otherUserId = selectedUser.Id; await OpenChatAsync(selectedChatLogin); }
        private async Task OpenChatAsync(string chatName) { Dispatcher.Invoke(() => { ChatPlaceholder.Visibility = Visibility.Collapsed; ChatArea.Visibility = Visibility.Visible; Messages.Clear(); ChatHeader.Text = chatName; ApplyPerformanceSettings(); }); var messages = await Task.Run(() => dbHelper.GetMessages(currentChatId)); foreach (var msg in messages.OrderBy(m => m.Timestamp)) { AddMessageToUI(msg.Id, msg.MessageText, msg.AttachmentUrl, msg.SenderId == currentUserId, msg.Timestamp); } lastMessageCheck = messages.Any() ? messages.Max(m => m.Timestamp) : DateTime.MinValue; Dispatcher.InvokeAsync(() => ChatScrollViewer?.ScrollToEnd()); }
        private async Task CheckNewMessagesAsync() { if (currentChatId == -1 || isUnloading) return; var newMessages = await Task.Run(() => dbHelper.GetMessagesAfter(currentChatId, lastMessageCheck)); if (isUnloading || !newMessages.Any()) return; foreach (var msg in newMessages) { if (!Messages.Any(m => m.Id == msg.Id)) { AddMessageToUI(msg.Id, msg.MessageText, msg.AttachmentUrl, msg.SenderId == currentUserId, msg.Timestamp); } } lastMessageCheck = newMessages.Max(m => m.Timestamp); Dispatcher.InvokeAsync(() => ChatScrollViewer?.ScrollToEnd()); }
        private void AddMessageToUI(int id, string message, string attachmentUrl, bool isSentByMe, DateTime timestamp) { if (isUnloading) return; Dispatcher.Invoke(() => { Messages.Add(new MessageViewModel { Id = id, MessageText = message, AttachmentUrl = attachmentUrl, IsSentByMe = isSentByMe, Timestamp = timestamp }); }); }
        private async void TranslateButton_Click(object sender, RoutedEventArgs e) { if (sender is Button button && button.Tag is MessageViewModel vm) { string originalText = vm.MessageText.Split(new[] { "\n\n[Перевод]:" }, StringSplitOptions.None)[0]; string direction = (TranslationDirectionComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "EN → RU"; string sourceLanguage = direction == "EN → RU" ? "en" : "ru"; string targetLanguage = direction == "EN → RU" ? "ru" : "en"; string translatedText = await translationService.TranslateTextAsync(originalText, sourceLanguage, targetLanguage); vm.MessageText = $"{originalText}\n\n[Перевод]: {translatedText}"; button.Visibility = Visibility.Collapsed; } }
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (sender is Image image && image.DataContext is MessageViewModel vm && !string.IsNullOrEmpty(vm.AttachmentUrl)) { OpenImage(vm.AttachmentUrl); } }
        private async void SendMessage_Click(object sender, RoutedEventArgs e) { if (currentChatId == -1) return; string newMessage = MessageTextBox.Text; string attachmentUrl = null; if (!string.IsNullOrEmpty(selectedAttachmentPath)) { try { attachmentUrl = await imageUploader.UploadImageAsync(selectedAttachmentPath); } catch (Exception ex) { MessageBox.Show($"Не удалось загрузить изображение: {ex.Message}"); return; } } if (string.IsNullOrWhiteSpace(newMessage) && string.IsNullOrEmpty(attachmentUrl)) return; DateTime timestamp = DateTime.Now; int messageId = await Task.Run(() => dbHelper.SaveMessageAndGetId(currentChatId, currentUserId, newMessage, attachmentUrl, timestamp)); AddMessageToUI(messageId, newMessage, attachmentUrl, true, timestamp); Dispatcher.Invoke(() => { MessageTextBox.Clear(); selectedAttachmentPath = null; ChatScrollViewer.ScrollToEnd(); }); lastMessageCheck = timestamp; }
        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift) { SendMessage_Click(sender, e); e.Handled = true; } }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) { if (!isPageLoaded) return; Task.Run(() => LoadChatsAsync(true)); }
        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e) { var textBox = sender as TextBox; if (textBox.Text == "Поиск") { textBox.Text = ""; textBox.Foreground = (Brush)FindResource("PrimaryTextColor"); } }
        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e) { var textBox = sender as TextBox; if (string.IsNullOrWhiteSpace(textBox.Text)) { textBox.Text = "Поиск"; textBox.Foreground = (Brush)FindResource("SecondaryTextColor"); } }
        private void MessageTextBox_TextChanged(object sender, TextChangedEventArgs e) { string text = MessageTextBox.Text; Task.Run(() => dbHelper.SetUserTyping(currentUserId, !string.IsNullOrEmpty(text))); }
        private void AttachFileButton_Click(object sender, RoutedEventArgs e) { var openFileDialog = new Microsoft.Win32.OpenFileDialog { Filter = "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*", Multiselect = false }; if (openFileDialog.ShowDialog() == true) { selectedAttachmentPath = openFileDialog.FileName; } }
        private void DownloadFile(string fileUrl) { var saveFileDialog = new Microsoft.Win32.SaveFileDialog { FileName = System.IO.Path.GetFileName(fileUrl), Filter = "All files (*.*)|*.*" }; if (saveFileDialog.ShowDialog() == true) { using (var client = new System.Net.WebClient()) { try { client.DownloadFile(fileUrl, saveFileDialog.FileName); MessageBox.Show("Файл успешно сохранён!"); } catch (Exception ex) { MessageBox.Show($"Ошибка при скачивании файла: {ex.Message}"); } } } }
        private void OpenImage(string imageUrl) { try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = imageUrl, UseShellExecute = true }); } catch (Exception ex) { MessageBox.Show($"Не удалось открыть изображение: {ex.Message}"); } }
        private void SettingsButton_Click(object sender, RoutedEventArgs e) { NavigationService?.Navigate(new SettingsPage(currentUserId, currentUserLogin, currentAvatarUrl)); }
        private async void ChatListBox_MouseRightButtonUp(object sender, MouseButtonEventArgs e) { if (ChatListBox.SelectedItem != null) { string selectedChat = ((dynamic)ChatListBox.SelectedItem).Login; int chatId = chatMapping[selectedChat]; var favoriteChats = await Task.Run(() => dbHelper.GetFavoriteChats(currentUserId).ToHashSet()); var contextMenu = new ContextMenu(); var favoriteItem = new MenuItem { Header = favoriteChats.Contains(chatId) ? "Убрать из избранного" : "Добавить в избранное" }; favoriteItem.Click += async (s, args) => { if (favoriteChats.Contains(chatId)) { await Task.Run(() => dbHelper.RemoveFavoriteChat(currentUserId, chatId)); } else { await Task.Run(() => dbHelper.AddFavoriteChat(currentUserId, chatId)); } await LoadChatsAsync(true); }; contextMenu.Items.Add(favoriteItem); contextMenu.IsOpen = true; } }
        private async void ToggleFavoriteMode_Click(object sender, RoutedEventArgs e) { isFavoriteMode = !isFavoriteMode; await LoadChatsAsync(true); }
        private void Page_Unloaded(object sender, RoutedEventArgs e) { isUnloading = true; statusTimer?.Stop(); Task.Run(() => { dbHelper.SetUserOnline(currentUserId, false); dbHelper.SetUserTyping(currentUserId, false); }); Window parentWindow = Window.GetWindow(this); if (parentWindow != null) { parentWindow.Activated -= ParentWindow_Activated; parentWindow.Deactivated -= ParentWindow_Deactivated; parentWindow.KeyDown -= ParentWindow_KeyDown; } }
    }
}

