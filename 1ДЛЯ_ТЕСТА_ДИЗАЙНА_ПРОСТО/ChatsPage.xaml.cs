using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО
{
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
        private readonly HashSet<int> displayedMessageIds = new HashSet<int>();
        private string selectedAttachmentPath = null;
        private readonly ImageUploader imageUploader;
        private bool isWindowActive = false;

        public ChatsPage(int userId, string userLogin, string avatarUrl = null)
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            currentUserId = userId;
            currentUserLogin = userLogin;
            currentAvatarUrl = avatarUrl;
            translationService = new TranslationService();
            imageUploader = new ImageUploader();

            statusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            statusTimer.Tick += StatusTimer_Tick;

            Loaded += ChatsPage_Loaded;
            Unloaded += Page_Unloaded;
        }

        private async void ChatsPage_Loaded(object sender, RoutedEventArgs e)
        {
            var currentUser = await Task.Run(() => dbHelper.GetUserById(currentUserId));
            if (currentUser != null)
            {
                currentAvatarUrl = currentUser.AvatarUrl;
            }

            await LoadChatsAsync(true);
            statusTimer.Start();
            await Task.Run(() => dbHelper.SetUserOnline(currentUserId, true));

            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Activated += ParentWindow_Activated;
                parentWindow.Deactivated += ParentWindow_Deactivated;
                parentWindow.KeyDown += ParentWindow_KeyDown;
            }
        }

        private void ParentWindow_Activated(object sender, EventArgs e)
        {
            isWindowActive = true;
        }

        private void ParentWindow_Deactivated(object sender, EventArgs e)
        {
            isWindowActive = false;
        }

        private void ParentWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (isWindowActive && !MessageTextBox.IsFocused && !SearchTextBox.IsFocused && currentChatId != -1)
            {
                if (e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                    e.Key == Key.LeftAlt || e.Key == Key.RightAlt || e.Key == Key.System || e.Key == Key.Enter ||
                    e.Key == Key.Tab || e.Key == Key.Escape || e.Key == Key.CapsLock)
                {
                    return;
                }

                MessageTextBox.Focus();

                string inputChar = GetCharFromKey(e.Key, Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));
                if (!string.IsNullOrEmpty(inputChar))
                {
                    MessageTextBox.AppendText(inputChar);
                }
                else if (e.Key == Key.Space)
                {
                    MessageTextBox.AppendText(" ");
                }
                else if (e.Key == Key.Back)
                {
                    if (!string.IsNullOrEmpty(MessageTextBox.Text))
                    {
                        MessageTextBox.Text = MessageTextBox.Text.Substring(0, MessageTextBox.Text.Length - 1);
                    }
                }

                MessageTextBox.CaretIndex = MessageTextBox.Text.Length;
                e.Handled = true;
            }
        }

        private string GetCharFromKey(Key key, bool isShiftPressed)
        {
            try
            {
                if (key >= Key.A && key <= Key.Z)
                {
                    string letter = key.ToString();
                    return isShiftPressed ? letter : letter.ToLower();
                }
                else if (key >= Key.D0 && key <= Key.D9)
                {
                    if (isShiftPressed)
                    {
                        switch (key)
                        {
                            case Key.D1: return "!";
                            case Key.D2: return "@";
                            case Key.D3: return "#";
                            case Key.D4: return "$";
                            case Key.D5: return "%";
                            case Key.D6: return "^";
                            case Key.D7: return "&";
                            case Key.D8: return "*";
                            case Key.D9: return "(";
                            case Key.D0: return ")";
                            default: return key.ToString().Substring(1);
                        }
                    }
                    return key.ToString().Substring(1);
                }
                else if (key == Key.OemPeriod) return isShiftPressed ? ">" : ".";
                else if (key == Key.OemComma) return isShiftPressed ? "<" : ",";
                else if (key == Key.OemMinus) return isShiftPressed ? "_" : "-";
                else if (key == Key.OemPlus) return isShiftPressed ? "+" : "=";
                else if (key == Key.OemQuestion) return isShiftPressed ? "?" : "/";
                else if (key == Key.OemSemicolon) return isShiftPressed ? ":" : ";";
                else if (key == Key.OemQuotes) return isShiftPressed ? "\"" : "'";
            }
            catch
            {
                return null;
            }
            return null;
        }

        private async void StatusTimer_Tick(object sender, EventArgs e)
        {
            await LoadChatsAsync(false);
            if (currentChatId != -1 && otherUserId != -1)
            {
                var (isOnline, _, isTyping) = await Task.Run(() => dbHelper.GetUserStatus(otherUserId));
                Dispatcher.Invoke(() =>
                {
                    TypingIndicator.Visibility = isTyping ? Visibility.Visible : Visibility.Collapsed;
                });
                await CheckNewMessagesAsync();
            }
        }

        private async Task LoadChatsAsync(bool fullReload)
        {
            var users = await Task.Run(() => dbHelper.GetUsers(currentUserId));
            var favoriteChats = await Task.Run(() => dbHelper.GetFavoriteChats(currentUserId).ToHashSet());
            string searchText = Dispatcher.Invoke(() => SearchTextBox.Text.Trim().ToLower());

            var chatItems = new List<object>();

            foreach (var user in users.OrderBy(u => u.Login))
            {
                if (string.IsNullOrEmpty(searchText) || user.Login.ToLower().Contains(searchText))
                {
                    int chatId = chatMapping.ContainsKey(user.Login)
                        ? chatMapping[user.Login]
                        : await Task.Run(() => dbHelper.GetOrCreateChat(currentUserId, user.Id));

                    if (!isFavoriteMode || (isFavoriteMode && favoriteChats.Contains(chatId)))
                    {
                        chatMapping[user.Login] = chatId;
                        chatItems.Add(new
                        {
                            Login = user.Login,
                            AvatarUrl = user.AvatarUrl,
                            IsOnline = user.IsOnline,
                            LastSeen = user.LastSeen,
                            IsTyping = user.IsTyping
                        });
                    }
                }
                else if (chatMapping.ContainsKey(user.Login))
                {
                    chatMapping.Remove(user.Login);
                }
            }

            Dispatcher.Invoke(() =>
            {
                if (fullReload)
                {
                    ChatListBox.Items.Clear();
                    foreach (var item in chatItems)
                    {
                        ChatListBox.Items.Add(item);
                    }
                }
                else
                {
                    for (int i = 0; i < ChatListBox.Items.Count; i++)
                    {
                        var existingItem = (dynamic)ChatListBox.Items[i];
                        var updatedItem = chatItems.FirstOrDefault(x => ((dynamic)x).Login == existingItem.Login);
                        if (updatedItem != null)
                        {
                            ChatListBox.Items[i] = updatedItem;
                        }
                    }

                    for (int i = ChatListBox.Items.Count - 1; i >= 0; i--)
                    {
                        var existingItem = (dynamic)ChatListBox.Items[i];
                        if (!chatItems.Any(x => ((dynamic)x).Login == existingItem.Login))
                        {
                            ChatListBox.Items.RemoveAt(i);
                        }
                    }

                    foreach (var item in chatItems)
                    {
                        if (!ChatListBox.Items.Cast<dynamic>().Any(x => x.Login == ((dynamic)item).Login))
                        {
                            ChatListBox.Items.Add(item);
                        }
                    }
                }

                if (ChatListBox.Items.Count > 0)
                {
                    if (selectedChatLogin != null)
                    {
                        ChatListBox.SelectedItem = ChatListBox.Items.Cast<dynamic>()
                            .FirstOrDefault(x => x.Login == selectedChatLogin);
                    }
                    else if (ChatListBox.SelectedItem == null)
                    {
                        ChatListBox.SelectedIndex = 0;
                    }
                }
            });
        }

        private async void ChatListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatListBox.SelectedItem == null) return;

            string newSelectedChat = ((dynamic)ChatListBox.SelectedItem).Login;
            if (newSelectedChat == selectedChatLogin || !chatMapping.ContainsKey(newSelectedChat)) return;

            selectedChatLogin = newSelectedChat;
            currentChatId = chatMapping[selectedChatLogin];

            var users = await Task.Run(() => dbHelper.GetUsers(currentUserId));
            var selectedUser = users.FirstOrDefault(u => u.Login == selectedChatLogin);

            if (selectedUser.Login == null)
            {
                MessageBox.Show($"Пользователь {selectedChatLogin} не найден в базе данных.");
                return;
            }

            otherUserId = selectedUser.Id;
            await OpenChatAsync(selectedChatLogin);
            TypingIndicator.DataContext = new { SelectedChatLogin = selectedChatLogin };
        }

        private async Task OpenChatAsync(string chatName)
        {
            Dispatcher.Invoke(() =>
            {
                ChatPlaceholder.Visibility = Visibility.Collapsed;
                ChatArea.Visibility = Visibility.Visible;
                MessagesPanel.Children.Clear();
                displayedMessageIds.Clear();
                ChatHeader.Text = chatName;
            });

            var messages = await Task.Run(() => dbHelper.GetMessages(currentChatId));
            foreach (var message in messages.OrderBy(m => m.Timestamp))
            {
                bool isSentByMe = message.SenderId == currentUserId;
                Dispatcher.Invoke(() => AddMessageToUI(message.MessageText, message.AttachmentUrl, isSentByMe, message.Timestamp));
                displayedMessageIds.Add(message.Id);
            }

            lastMessageCheck = messages.Any() ? messages.Max(m => m.Timestamp) : DateTime.MinValue;
            Dispatcher.Invoke(() => ChatScrollViewer.ScrollToEnd());
        }

        private async Task CheckNewMessagesAsync()
        {
            if (currentChatId == -1) return;

            var newMessages = await Task.Run(() => dbHelper.GetMessagesAfter(currentChatId, lastMessageCheck));
            if (newMessages.Any())
            {
                foreach (var message in newMessages)
                {
                    if (!displayedMessageIds.Contains(message.Id))
                    {
                        bool isSentByMe = message.SenderId == currentUserId;
                        Dispatcher.Invoke(() => AddMessageToUI(message.MessageText, message.AttachmentUrl, isSentByMe, message.Timestamp));
                        displayedMessageIds.Add(message.Id);
                    }
                }
                lastMessageCheck = newMessages.Max(m => m.Timestamp);
                Dispatcher.Invoke(() => ChatScrollViewer.ScrollToEnd());
            }
        }

        private void AddMessageToUI(string message, string attachmentUrl, bool isSentByMe, DateTime timestamp)
        {
            var messageContainer = new StackPanel
            {
                Margin = new Thickness(5),
                HorizontalAlignment = isSentByMe ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                MaxWidth = 450
            };

            var contentPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = isSentByMe ? HorizontalAlignment.Right : HorizontalAlignment.Left
            };

            var messageBorder = new Border
            {
                CornerRadius = new CornerRadius(10),
                Background = isSentByMe ? new SolidColorBrush(Color.FromRgb(85, 85, 85)) : new SolidColorBrush(Color.FromRgb(68, 68, 68)),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 2, 5, 2)
            };

            var innerPanel = new StackPanel();

            bool isMessageImageUrl = !string.IsNullOrEmpty(message) &&
                                     (message.StartsWith("http://") || message.StartsWith("https://")) &&
                                     (message.EndsWith(".png") || message.EndsWith(".jpg") || message.EndsWith(".jpeg"));

            if (!string.IsNullOrEmpty(message) && !isMessageImageUrl)
            {
                var messageBlock = new TextBlock
                {
                    Text = message,
                    Foreground = Brushes.White,
                    TextWrapping = TextWrapping.Wrap,
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 14
                };
                innerPanel.Children.Add(messageBlock);
            }

            string imageUrlToDisplay = null;
            if (!string.IsNullOrEmpty(attachmentUrl))
            {
                imageUrlToDisplay = attachmentUrl;
            }
            else if (isMessageImageUrl)
            {
                imageUrlToDisplay = message;
            }

            if (!string.IsNullOrEmpty(imageUrlToDisplay))
            {
                if (imageUrlToDisplay.EndsWith(".png") || imageUrlToDisplay.EndsWith(".jpg") || imageUrlToDisplay.EndsWith(".jpeg"))
                {
                    try
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = new Uri(imageUrlToDisplay, UriKind.Absolute);
                        bitmapImage.EndInit();

                        var image = new Image
                        {
                            Source = bitmapImage,
                            MaxWidth = 200,
                            MaxHeight = 200,
                            Margin = new Thickness(0, 5, 0, 0),
                            Cursor = Cursors.Hand // Подсказка, что изображение кликабельно
                        };

                        // Обработчик клика для открытия изображения
                        image.MouseLeftButtonDown += (s, e) => OpenImage(imageUrlToDisplay);

                        // Добавление контекстного меню для скачивания
                        var contextMenu = new ContextMenu();
                        var downloadItem = new MenuItem { Header = "Скачать" };
                        downloadItem.Click += (s, e) => DownloadFile(imageUrlToDisplay);
                        contextMenu.Items.Add(downloadItem);
                        image.ContextMenu = contextMenu;

                        innerPanel.Children.Add(image);
                    }
                    catch (Exception ex)
                    {
                        var errorBlock = new TextBlock
                        {
                            Text = $"Ошибка загрузки изображения: {ex.Message}",
                            Foreground = Brushes.Red,
                            TextWrapping = TextWrapping.Wrap,
                            FontFamily = new FontFamily("Segoe UI"),
                            FontSize = 12,
                            Margin = new Thickness(0, 5, 0, 0)
                        };
                        innerPanel.Children.Add(errorBlock);
                    }
                }
                else
                {
                    var fileLink = new TextBlock
                    {
                        Text = $"Файл: {System.IO.Path.GetFileName(imageUrlToDisplay)}",
                        Foreground = Brushes.LightBlue,
                        TextWrapping = TextWrapping.Wrap,
                        FontFamily = new FontFamily("Segoe UI"),
                        FontSize = 14,
                        Margin = new Thickness(0, 5, 0, 0),
                        Cursor = Cursors.Hand
                    };
                    fileLink.MouseLeftButtonDown += (s, e) => DownloadFile(imageUrlToDisplay);
                    innerPanel.Children.Add(fileLink);
                }
            }

            messageBorder.Child = innerPanel;

            var translateButton = new Button
            {
                Content = "Перевести",
                Style = (Style)FindResource("TranslateButtonStyle"),
                Tag = message,
                Visibility = !string.IsNullOrEmpty(message) && !isMessageImageUrl ? Visibility.Visible : Visibility.Collapsed
            };
            translateButton.Click += TranslateButton_Click;

            if (isSentByMe)
            {
                contentPanel.Children.Add(translateButton);
                contentPanel.Children.Add(messageBorder);
            }
            else
            {
                contentPanel.Children.Add(messageBorder);
                contentPanel.Children.Add(translateButton);
            }

            var timestampBlock = new TextBlock
            {
                Text = timestamp.ToString("HH:mm"),
                Foreground = Brushes.Gray,
                FontSize = 10,
                HorizontalAlignment = isSentByMe ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 5),
                FontFamily = new FontFamily("Segoe UI")
            };

            messageContainer.Children.Add(contentPanel);
            messageContainer.Children.Add(timestampBlock);
            MessagesPanel.Children.Add(messageContainer);
        }

        private async void TranslateButton_Click(object sender, RoutedEventArgs e)
        {
            var translateButton = sender as Button;
            if (translateButton == null) return;

            string messageText = translateButton.Tag as string;
            if (string.IsNullOrEmpty(messageText))
            {
                MessageBox.Show("Сообщение пустое, нечего переводить.");
                return;
            }

            string direction = (TranslationDirectionComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "EN → RU";
            string sourceLanguage = direction == "EN → RU" ? "en" : "ru";
            string targetLanguage = direction == "EN → RU" ? "ru" : "en";

            string translatedText = await translationService.TranslateTextAsync(messageText, sourceLanguage, targetLanguage);
            if (translatedText.Contains("INVALID SOURCE LANGUAGE"))
            {
                MessageBox.Show($"Ошибка: Неверный исходный язык ({sourceLanguage}). Попробуйте другой язык.");
                return;
            }

            var buttonParent = translateButton.Parent as StackPanel;
            var messageContainer = buttonParent?.Parent as StackPanel;
            if (messageContainer != null)
            {
                var translatedBlock = new TextBlock
                {
                    Text = $"[Перевод]: {translatedText}",
                    Foreground = Brushes.LightGray,
                    TextWrapping = TextWrapping.Wrap,
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 12,
                    Margin = new Thickness(10, 0, 0, 5)
                };
                Dispatcher.Invoke(() =>
                {
                    messageContainer.Children.Add(translatedBlock);
                    ChatScrollViewer.ScrollToEnd();
                });
            }
        }

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
            }
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (currentChatId == -1) return;

            string newMessage = MessageTextBox.Text;
            string attachmentUrl = null;
            DateTime timestamp = DateTime.Now;

            bool isMessageImageUrl = !string.IsNullOrEmpty(newMessage) &&
                                     (newMessage.StartsWith("http://") || newMessage.StartsWith("https://")) &&
                                     (newMessage.EndsWith(".png") || newMessage.EndsWith(".jpg") || newMessage.EndsWith(".jpeg"));

            if (isMessageImageUrl)
            {
                attachmentUrl = newMessage;
                newMessage = null;
            }
            else if (!string.IsNullOrEmpty(selectedAttachmentPath))
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

                if (newMessage == $"Прикреплён файл: {System.IO.Path.GetFileName(selectedAttachmentPath)}")
                {
                    newMessage = null;
                }
            }

            if (string.IsNullOrEmpty(newMessage) && string.IsNullOrEmpty(attachmentUrl)) return;

            int messageId = await Task.Run(() => dbHelper.SaveMessageAndGetId(currentChatId, currentUserId, newMessage, attachmentUrl, timestamp));

            Dispatcher.Invoke(() =>
            {
                AddMessageToUI(newMessage, attachmentUrl, true, timestamp);
                displayedMessageIds.Add(messageId);
                ChatScrollViewer.ScrollToEnd();
                MessageTextBox.Clear();
                selectedAttachmentPath = null;
            });

            lastMessageCheck = timestamp;
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    MessageTextBox.AppendText(Environment.NewLine);
                    MessageTextBox.CaretIndex = MessageTextBox.Text.Length;
                }
                else
                {
                    SendMessage_Click(sender, e);
                }
                e.Handled = true;
            }
        }

        private void DownloadFile(string fileUrl)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = System.IO.Path.GetFileName(fileUrl),
                Filter = "All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (var client = new System.Net.WebClient())
                {
                    try
                    {
                        client.DownloadFile(fileUrl, saveFileDialog.FileName);
                        MessageBox.Show("Файл успешно сохранён!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при скачивании файла: {ex.Message}");
                    }
                }
            }
        }

        private void OpenImage(string imageUrl)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = imageUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть изображение: {ex.Message}");
            }
        }

        private void MessageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = MessageTextBox.Text;
            Task.Run(() => dbHelper.SetUserTyping(currentUserId, !string.IsNullOrEmpty(text)));
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox.Text == "Поиск")
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Поиск";
                textBox.Foreground = Brushes.Gray;
            }
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox.Text != "Поиск")
            {
                await LoadChatsAsync(true);
            }
        }

        private void SettingsButton_Click(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new SettingsPage(currentUserId, currentUserLogin, currentAvatarUrl));
        }

        private async void ChatListBox_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ChatListBox.SelectedItem != null)
            {
                string selectedChat = ((dynamic)ChatListBox.SelectedItem).Login;
                int chatId = chatMapping[selectedChat];
                var favoriteChats = await Task.Run(() => dbHelper.GetFavoriteChats(currentUserId).ToHashSet());

                var contextMenu = new ContextMenu();
                var favoriteItem = new MenuItem
                {
                    Header = favoriteChats.Contains(chatId) ? "Убрать из избранного" : "Добавить в избранное"
                };
                favoriteItem.Click += async (s, args) =>
                {
                    if (favoriteChats.Contains(chatId))
                    {
                        await Task.Run(() => dbHelper.RemoveFavoriteChat(currentUserId, chatId));
                    }
                    else
                    {
                        await Task.Run(() => dbHelper.AddFavoriteChat(currentUserId, chatId));
                    }
                    await LoadChatsAsync(true);
                };
                contextMenu.Items.Add(favoriteItem);
                contextMenu.IsOpen = true;
            }
        }

        private async void ToggleFavoriteMode_Click(object sender, RoutedEventArgs e)
        {
            isFavoriteMode = !isFavoriteMode;
            FavoriteButton.Content = isFavoriteMode ? "Все чаты" : "Избранное";
            await LoadChatsAsync(true);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            statusTimer.Stop();
            Task.Run(() =>
            {
                dbHelper.SetUserOnline(currentUserId, false);
                dbHelper.SetUserTyping(currentUserId, false);
            });

            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Activated -= ParentWindow_Activated;
                parentWindow.Deactivated -= ParentWindow_Deactivated;
                parentWindow.KeyDown -= ParentWindow_KeyDown;
            }
        }
    }
}