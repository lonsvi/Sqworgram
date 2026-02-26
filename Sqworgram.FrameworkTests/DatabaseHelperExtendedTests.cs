using System;
using System.IO;
using Xunit;
using _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО;

namespace Sqworgram.FrameworkTests
{
    public class DatabaseHelperExtendedTests : IDisposable
    {
        private readonly string dbFileName = "test_helper_2.db";

        public DatabaseHelperExtendedTests()
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
        public void RegisterUser_WithDuplicateLogin_ReturnsFalse()
        {
            // Arrange
            var helper = new DatabaseHelper();
            string login = "duplicate";
            string password = "pass1";

            helper.RegisterUser(login, password);

            // Act
            bool result = helper.RegisterUser(login, password);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void LoginUser_WithValidCredentials_ReturnsUserInfo()
        {
            // Arrange
            var helper = new DatabaseHelper();
            string login = "john";
            string password = "john123";

            helper.RegisterUser(login, password);

            // Act
            var result = helper.LoginUser(login, password);

            // Assert
            Assert.NotNull(result.UserId);
            Assert.NotNull(result.Login);
            Assert.Equal(login, result.Login);
        }

        [Fact]
        public void LoginUser_WithInvalidPassword_ReturnsNullUserId()
        {
            // Arrange
            var helper = new DatabaseHelper();
            helper.RegisterUser("user1", "correct");

            // Act
            var result = helper.LoginUser("user1", "wrong");

            // Assert
            Assert.Null(result.UserId);
        }

        [Fact]
        public void GetUserById_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var helper = new DatabaseHelper();

            // Act
            var user = helper.GetUserById(999);

            // Assert
            Assert.Null(user);
        }

        [Fact]
        public void UpdateAvatarUrl_UpdatesSuccessfully()
        {
            // Arrange
            var helper = new DatabaseHelper();
            helper.RegisterUser("avataruser", "pass");
            var loginResult = helper.LoginUser("avataruser", "pass");
            string newAvatarUrl = "https://example.com/avatar.jpg";

            // Act
            helper.UpdateAvatarUrl(loginResult.UserId.Value, newAvatarUrl);
            var updatedUser = helper.GetUserById(loginResult.UserId.Value);

            // Assert
            Assert.Equal(newAvatarUrl, updatedUser.AvatarUrl);
        }

        [Fact]
        public void GetOrCreateChat_CreatesChatBetweenUsers()
        {
            // Arrange
            var helper = new DatabaseHelper();

            helper.RegisterUser("user1", "pass1");
            var user1 = helper.LoginUser("user1", "pass1");
            Assert.NotNull(user1.UserId);

            helper.RegisterUser("user2", "pass2");
            var user2 = helper.LoginUser("user2", "pass2");
            Assert.NotNull(user2.UserId);

            // Act
            int chatId = helper.GetOrCreateChat(user1.UserId.Value, user2.UserId.Value);

            // Assert
            Assert.True(chatId > 0);
        }

        [Fact]
        public void GetOrCreateChat_ReturnsSameChatIdForExistingChat()
        {
            // Arrange
            var helper = new DatabaseHelper();
            helper.RegisterUser("userA", "passA");
            helper.RegisterUser("userB", "passB");
            var userA = helper.LoginUser("userA", "passA");
            var userB = helper.LoginUser("userB", "passB");

            // Act
            int chatId1 = helper.GetOrCreateChat(userA.UserId.Value, userB.UserId.Value);
            int chatId2 = helper.GetOrCreateChat(userA.UserId.Value, userB.UserId.Value);

            // Assert
            Assert.Equal(chatId1, chatId2);
        }

        [Fact]
        public void SaveMessageAndGetId_SavesMessage()
        {
            // Arrange
            var helper = new DatabaseHelper();
            helper.RegisterUser("sender", "senderpass");
            var sender = helper.LoginUser("sender", "senderpass");
            helper.RegisterUser("receiver", "recpass");
            var receiver = helper.LoginUser("receiver", "recpass");
            
            int chatId = helper.GetOrCreateChat(sender.UserId.Value, receiver.UserId.Value);
            string messageText = "Hello!";
            DateTime timestamp = DateTime.Now;

            // Act
            int messageId = helper.SaveMessageAndGetId(chatId, sender.UserId.Value, messageText, null, timestamp);

            // Assert
            Assert.True(messageId > 0);
        }

        
    }
}
