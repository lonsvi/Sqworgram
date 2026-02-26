using System;
using System.IO;
using Xunit;
using _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО;

namespace Sqworgram.FrameworkTests
{
    public class DatabaseHelperFullCoverageTests : IDisposable
    {
        private readonly string dbFileName = "test_helper_3.db";

        public DatabaseHelperFullCoverageTests()
        {
            CleanupDatabase();
        }

        public void Dispose()
        {
            CleanupDatabase();
        }

        private void CleanupDatabase()
        {
            if (File.Exists(dbFileName))
            {
                try
                {
                    // Force garbage collection multiple times to close SQLite handles
                    for (int i = 0; i < 3; i++)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        System.Threading.Thread.Sleep(50);
                    }
                    
                    // Wait longer for file to be released
                    System.Threading.Thread.Sleep(200);
                    
                    File.Delete(dbFileName);
                }
                catch (Exception)
                {
                    // If still locked, try with longer wait
                    try
                    {
                        System.Threading.Thread.Sleep(1000);
                        File.Delete(dbFileName);
                    }
                    catch { /* Ignore */ }
                }
            }
        }

        [Fact]
        public void GetUserById_WithInvalidUserId_ReturnsNull()
        {
            // Arrange
            var db = new DatabaseHelper();

            // Act
            var user = db.GetUserById(999);

            // Assert
            Assert.Null(user);
        }

        [Fact]
        public void GetUsers_WithMultipleUsers_ReturnsAllExcludingRequested()
        {
            // Arrange
            var db = new DatabaseHelper();
            db.RegisterUser("user1", "pass1");
            db.RegisterUser("user2", "pass2");
            db.RegisterUser("user3", "pass3");
            
            var loginResult = db.LoginUser("user1", "pass1");
            var user1Id = loginResult.UserId.Value;

            // Act
            var users = db.GetUsers(user1Id);

            // Assert
            Assert.NotNull(users);
            Assert.True(users.Count >= 2, "Should return at least 2 other users");
        }

        [Fact]
        public void UpdateAvatarUrl_WithValidUserId_UpdatesSuccessfully()
        {
            // Arrange
            var db = new DatabaseHelper();
            db.RegisterUser("testuser", "password");
            var loginResult = db.LoginUser("testuser", "password");
            var userId = loginResult.UserId.Value;
            var newAvatarUrl = "https://example.com/avatar.jpg";

            // Act
            db.UpdateAvatarUrl(userId, newAvatarUrl);
            var updatedUser = db.GetUserById(userId);

            // Assert
            Assert.NotNull(updatedUser);
            Assert.Equal(newAvatarUrl, updatedUser.AvatarUrl);
        }

        [Fact]
        public void GetOrCreateChat_BetweenTwoUsers_ReturnsChatId()
        {
            // Arrange
            var db = new DatabaseHelper();
            db.RegisterUser("user1", "pass1");
            db.RegisterUser("user2", "pass2");
            var user1 = db.LoginUser("user1", "pass1");
            var user2 = db.LoginUser("user2", "pass2");

            // Act
            var chatId = db.GetOrCreateChat(user1.UserId.Value, user2.UserId.Value);

            // Assert
            Assert.True(chatId > 0);
        }

        [Fact]
        public void GetOrCreateChat_SameUsersTwice_ReturnsSameChatId()
        {
            // Arrange
            var db = new DatabaseHelper();
            db.RegisterUser("user1", "pass1");
            db.RegisterUser("user2", "pass2");
            var user1 = db.LoginUser("user1", "pass1");
            var user2 = db.LoginUser("user2", "pass2");

            // Act
            var chatId1 = db.GetOrCreateChat(user1.UserId.Value, user2.UserId.Value);
            var chatId2 = db.GetOrCreateChat(user1.UserId.Value, user2.UserId.Value);

            // Assert
            Assert.Equal(chatId1, chatId2);
        }

        [Fact]
        public void SaveMessageAndGetId_CreatesMessage_ReturnsMsgId()
        {
            // Arrange
            var db = new DatabaseHelper();
            db.RegisterUser("user1", "pass1");
            db.RegisterUser("user2", "pass2");
            var user1 = db.LoginUser("user1", "pass1");
            var user2 = db.LoginUser("user2", "pass2");
            var chatId = db.GetOrCreateChat(user1.UserId.Value, user2.UserId.Value);

            // Act
            var msgId = db.SaveMessageAndGetId(chatId, user1.UserId.Value, "Hello", null, DateTime.Now);

            // Assert
            Assert.True(msgId > 0);
        }

        [Fact]
        public void GetMessages_AfterSavingMessages_ReturnsMessages()
        {
            // Arrange
            var db = new DatabaseHelper();
            db.RegisterUser("user1", "pass1");
            db.RegisterUser("user2", "pass2");
            var user1 = db.LoginUser("user1", "pass1");
            var user2 = db.LoginUser("user2", "pass2");
            var chatId = db.GetOrCreateChat(user1.UserId.Value, user2.UserId.Value);
            db.SaveMessageAndGetId(chatId, user1.UserId.Value, "Message 1", null, DateTime.Now);
            db.SaveMessageAndGetId(chatId, user2.UserId.Value, "Message 2", null, DateTime.Now);

            // Act
            var messages = db.GetMessages(chatId);

            // Assert
            Assert.NotNull(messages);
            Assert.True(messages.Count >= 2);
        }

        [Fact]
        public void GetMessagesAfter_WithTimestamp_ReturnsRecentMessages()
        {
            // Arrange
            var db = new DatabaseHelper();
            db.RegisterUser("user1", "pass1");
            db.RegisterUser("user2", "pass2");
            var user1 = db.LoginUser("user1", "pass1");
            var user2 = db.LoginUser("user2", "pass2");
            var chatId = db.GetOrCreateChat(user1.UserId.Value, user2.UserId.Value);
            
            var oldTime = DateTime.Now.AddMinutes(-10);
            db.SaveMessageAndGetId(chatId, user1.UserId.Value, "Old Message", null, oldTime);
            
            var recentTime = DateTime.Now;
            db.SaveMessageAndGetId(chatId, user2.UserId.Value, "Recent Message", null, recentTime);

            // Act
            var messages = db.GetMessagesAfter(chatId, recentTime.AddSeconds(-1));

            // Assert
            Assert.NotNull(messages);
            Assert.True(messages.Count >= 1);
        }

        [Fact]
        public void SetUserOnline_WithValidUserId_SetsOnlineStatus()
        {
            // Arrange
            var db = new DatabaseHelper();
            db.RegisterUser("testuser", "password");
            var loginResult = db.LoginUser("testuser", "password");
            var userId = loginResult.UserId.Value;

            // Act
            db.SetUserOnline(userId, true);
            var status = db.GetUserStatus(userId);

            // Assert
            Assert.True(status.IsOnline);
        }

        [Fact]
        public void SetUserOnline_WithFalse_SetsOfflineStatus()
        {
            // Arrange
            var db = new DatabaseHelper();
            db.RegisterUser("testuser", "password");
            var loginResult = db.LoginUser("testuser", "password");
            var userId = loginResult.UserId.Value;
            db.SetUserOnline(userId, true);

            // Act
            db.SetUserOnline(userId, false);
            var status = db.GetUserStatus(userId);

            // Assert
            Assert.False(status.IsOnline);
        }

        [Fact]
        public void SetUserTyping_WithTrue_SetsTypingStatus()
        {
            // Arrange
            var db = new DatabaseHelper();
            db.RegisterUser("testuser", "password");
            var loginResult = db.LoginUser("testuser", "password");
            var userId = loginResult.UserId.Value;

            // Act
            db.SetUserTyping(userId, true);
            var status = db.GetUserStatus(userId);

            // Assert
            Assert.True(status.IsTyping);
        }

        [Fact]
        public void SetUserTyping_WithFalse_ClearsTypingStatus()
        {
            // Arrange
            var db = new DatabaseHelper();
            db.RegisterUser("testuser", "password");
            var loginResult = db.LoginUser("testuser", "password");
            var userId = loginResult.UserId.Value;
            db.SetUserTyping(userId, true);

            // Act
            db.SetUserTyping(userId, false);
            var status = db.GetUserStatus(userId);

            // Assert
            Assert.False(status.IsTyping);
        }

        [Fact]
        public void AddFavoriteChat_WithValidIds_AddsSuccessfully()
        {
            // Arrange
            var db = new DatabaseHelper();
            db.RegisterUser("user1", "pass1");
            db.RegisterUser("user2", "pass2");
            var user1 = db.LoginUser("user1", "pass1");
            var user2 = db.LoginUser("user2", "pass2");
            var chatId = db.GetOrCreateChat(user1.UserId.Value, user2.UserId.Value);

            // Act
            db.AddFavoriteChat(user1.UserId.Value, chatId);
            var favorites = db.GetFavoriteChats(user1.UserId.Value);

            // Assert
            Assert.NotNull(favorites);
            Assert.Contains(chatId, favorites);
        }

        [Fact]
        public void RemoveFavoriteChat_WithValidIds_RemovesSuccessfully()
        {
            // Arrange
            var db = new DatabaseHelper();
            db.RegisterUser("user1", "pass1");
            db.RegisterUser("user2", "pass2");
            var user1 = db.LoginUser("user1", "pass1");
            var user2 = db.LoginUser("user2", "pass2");
            var chatId = db.GetOrCreateChat(user1.UserId.Value, user2.UserId.Value);
            db.AddFavoriteChat(user1.UserId.Value, chatId);

            // Act
            db.RemoveFavoriteChat(user1.UserId.Value, chatId);
            var favorites = db.GetFavoriteChats(user1.UserId.Value);

            // Assert
            Assert.NotNull(favorites);
            Assert.DoesNotContain(chatId, favorites);
        }

        [Fact]
        public void GetFavoriteChats_WithMultipleFavorites_ReturnsAll()
        {
            // Arrange
            var db = new DatabaseHelper();
            db.RegisterUser("user1", "pass1");
            db.RegisterUser("user2", "pass2");
            db.RegisterUser("user3", "pass3");
            var user1 = db.LoginUser("user1", "pass1");
            var user2 = db.LoginUser("user2", "pass2");
            var user3 = db.LoginUser("user3", "pass3");
            
            var chat1 = db.GetOrCreateChat(user1.UserId.Value, user2.UserId.Value);
            var chat2 = db.GetOrCreateChat(user1.UserId.Value, user3.UserId.Value);
            
            db.AddFavoriteChat(user1.UserId.Value, chat1);
            db.AddFavoriteChat(user1.UserId.Value, chat2);

            // Act
            var favorites = db.GetFavoriteChats(user1.UserId.Value);

            // Assert
            Assert.NotNull(favorites);
            Assert.True(favorites.Count >= 2);
            Assert.Contains(chat1, favorites);
            Assert.Contains(chat2, favorites);
        }
    }
}
