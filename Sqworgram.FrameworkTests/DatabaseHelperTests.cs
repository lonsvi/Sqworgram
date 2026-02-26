using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО;

namespace Sqworgram.FrameworkTests
{
    public class DatabaseHelperTests : IDisposable
    {
        private readonly string dbFileName = "test_helper_1.db";

        public DatabaseHelperTests()
        {
            // Cleanup before test
            CleanupDatabase();
        }

        public void Dispose()
        {
            // Cleanup after test
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
        public void AddFavoriteChat_AddsSuccessfully()
        {
            // Arrange
            var helper = new DatabaseHelper();
            int userId = 1;
            int chatId = 100;

            // Act
            helper.AddFavoriteChat(userId, chatId);
            var result = helper.GetFavoriteChats(userId);

            // Assert
            Assert.NotEmpty(result);
            Assert.Contains(chatId, result);
        }

        [Fact]
        public void GetFavoriteChats_ReturnsEmptyListForNewUser()
        {
            // Arrange
            var helper = new DatabaseHelper();

            // Act
            var result = helper.GetFavoriteChats(999);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void RemoveFavoriteChat_RemovesSuccessfully()
        {
            // Arrange
            var helper = new DatabaseHelper();
            int userId = 2;
            int chatId = 200;
            
            helper.AddFavoriteChat(userId, chatId);

            // Act
            helper.RemoveFavoriteChat(userId, chatId);
            var result = helper.GetFavoriteChats(userId);

            // Assert
            Assert.NotNull(result);
            Assert.DoesNotContain(chatId, result);
        }

        [Fact]
        public void AddFavoriteChat_MultipleChatsSameUser()
        {
            // Arrange
            var helper = new DatabaseHelper();
            int userId = 3;
            int chatId1 = 300;
            int chatId2 = 301;
            int chatId3 = 302;

            // Act
            helper.AddFavoriteChat(userId, chatId1);
            helper.AddFavoriteChat(userId, chatId2);
            helper.AddFavoriteChat(userId, chatId3);
            var result = helper.GetFavoriteChats(userId);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(chatId1, result);
            Assert.Contains(chatId2, result);
            Assert.Contains(chatId3, result);
        }

        [Fact]
        public void AddFavoriteChat_DuplicateIgnored()
        {
            // Arrange
            var helper = new DatabaseHelper();
            int userId = 4;
            int chatId = 400;

            // Act
            helper.AddFavoriteChat(userId, chatId);
            helper.AddFavoriteChat(userId, chatId); // Add same again
            var result = helper.GetFavoriteChats(userId);

            // Assert
            Assert.Single(result); // Should still be 1, not 2
            Assert.Equal(chatId, result[0]);
        }

        [Fact]
        public void GetFavoriteChats_MultipleUsers()
        {
            // Arrange
            var helper = new DatabaseHelper();

            // Act
            helper.AddFavoriteChat(5, 500);
            helper.AddFavoriteChat(5, 501);
            helper.AddFavoriteChat(6, 600);
            helper.AddFavoriteChat(6, 601);

            var user5Chats = helper.GetFavoriteChats(5);
            var user6Chats = helper.GetFavoriteChats(6);

            // Assert
            Assert.Equal(2, user5Chats.Count);
            Assert.Equal(2, user6Chats.Count);
            Assert.All(user5Chats, c => Assert.True(c >= 500 && c <= 501));
            Assert.All(user6Chats, c => Assert.True(c >= 600 && c <= 601));
        }

        [Fact]
        public void RemoveFavoriteChat_NonExistentDoesNotThrow()
        {
            // Arrange
            var helper = new DatabaseHelper();

            // Act & Assert (should not throw)
            helper.RemoveFavoriteChat(999, 999);
            var result = helper.GetFavoriteChats(999);
            Assert.Empty(result);
        }

        [Fact]
        public void AddFavoriteChat_WithZeroIds()
        {
            // Arrange
            var helper = new DatabaseHelper();
            int userId = 0;
            int chatId = 0;

            // Act
            helper.AddFavoriteChat(userId, chatId);
            var result = helper.GetFavoriteChats(userId);

            // Assert
            Assert.NotEmpty(result);
            Assert.Contains(0, result);
        }

        [Fact]
        public void AddFavoriteChat_WithLargeIds()
        {
            // Arrange
            var helper = new DatabaseHelper();
            int userId = int.MaxValue - 1;
            int chatId = int.MaxValue - 2;

            // Act
            helper.AddFavoriteChat(userId, chatId);
            var result = helper.GetFavoriteChats(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal(chatId, result[0]);
        }
    }
}
