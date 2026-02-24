using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО;
using Xunit;

namespace Sqworgram.UnitTests.ProxyTests
{
    /// <summary>
    /// REAL execution tests - используют реальные proxy классы БЕЗ моков
    /// Обеспечивают покрытие бизнес-логики основного проекта через Coverlet
    /// </summary>
    /// 
    // ===== DatabaseHelper Tests =====
    public class DatabaseHelperRealTests
    {
        [Fact]
        public void RegisterUser_WithValidCredentials_Succeeds()
        {
            // Arrange
            var db = new DatabaseHelper();
            
            // Act
            var result = db.RegisterUserAsync("testuser", "test@example.com", "TestPass123").Result;
            
            // Assert
            Assert.True(result, "Registration should succeed with valid data");
        }

        [Fact]
        public void RegisterUser_WithDuplicateEmail_Fails()
        {
            // Arrange
            var db = new DatabaseHelper();
            db.RegisterUserAsync("user1", "test@example.com", "TestPass123").Wait();
            
            // Act
            var result = db.RegisterUserAsync("user2", "test@example.com", "TestPass456").Result;
            
            // Assert
            Assert.False(result, "Duplicate email registration should fail");
        }

        [Fact]
        public void RegisterUser_WithShortUsername_Fails()
        {
            // Arrange
            var db = new DatabaseHelper();
            
            // Act
            var result = db.RegisterUserAsync("ab", "test@example.com", "TestPass123").Result;
            
            // Assert
            Assert.False(result, "Short username should fail");
        }

        [Fact]
        public void RegisterUser_WithShortPassword_Fails()
        {
            // Arrange
            var db = new DatabaseHelper();
            
            // Act
            var result = db.RegisterUserAsync("testuser", "test@example.com", "Test").Result;
            
            // Assert
            Assert.False(result, "Password shorter than 6 chars should fail");
        }

        [Fact]
        public void AuthenticateUser_WithCorrectCredentials_Succeeds()
        {
            // Arrange
            var db = new DatabaseHelper();
            db.RegisterUserAsync("testuser", "test@example.com", "TestPass123").Wait();
            
            // Act
            var result = db.AuthenticateUserAsync("test@example.com", "TestPass123").Result;
            
            // Assert
            Assert.True(result, "Authentication with correct credentials should succeed");
        }

        [Fact]
        public void AuthenticateUser_WithWrongPassword_Fails()
        {
            // Arrange
            var db = new DatabaseHelper();
            db.RegisterUserAsync("testuser", "test@example.com", "TestPass123").Wait();
            
            // Act
            var result = db.AuthenticateUserAsync("test@example.com", "WrongPass123").Result;
            
            // Assert
            Assert.False(result, "Authentication with wrong password should fail");
        }

        [Fact]
        public void SaveChat_WithValidData_Succeeds()
        {
            // Arrange
            var db = new DatabaseHelper();
            string userId = Guid.NewGuid().ToString();
            
            // Act
            var result = db.SaveChatAsync("My Chat", userId).Result;
            
            // Assert
            Assert.True(result, "Saving chat should succeed");
        }

        [Fact]
        public void GetUserChats_ReturnsUserChats()
        {
            // Arrange
            var db = new DatabaseHelper();
            string userId = Guid.NewGuid().ToString();
            db.SaveChatAsync("Chat1", userId).Wait();
            db.SaveChatAsync("Chat2", userId).Wait();
            db.SaveChatAsync("Chat3", "other-user").Wait();
            
            // Act
            var chats = db.GetUserChatsAsync(userId).Result;
            
            // Assert
            Assert.Equal(2, chats.Count);
            Assert.Contains("Chat1", chats);
            Assert.Contains("Chat2", chats);
        }
    }

    // ===== ValidationHelpers Tests =====
    public class ValidationHelpersRealTests
    {
        [Fact]
        public void IsValidUsername_WithValidUsername_ReturnsTrue()
        {
            // Act & Assert
            Assert.True(ValidationHelpers.IsValidUsername("john_doe"));
            Assert.True(ValidationHelpers.IsValidUsername("user123"));
            Assert.True(ValidationHelpers.IsValidUsername("test_user_name"));
        }

        [Fact]
        public void IsValidUsername_WithInvalidUsername_ReturnsFalse()
        {
            // Act & Assert
            Assert.False(ValidationHelpers.IsValidUsername("ab"));  // too short
            Assert.False(ValidationHelpers.IsValidUsername(""));    // empty
            Assert.False(ValidationHelpers.IsValidUsername("user with spaces"));  // spaces
        }

        [Fact]
        public void IsValidEmail_WithValidEmail_ReturnsTrue()
        {
            // Act & Assert
            Assert.True(ValidationHelpers.IsValidEmail("user@example.com"));
            Assert.True(ValidationHelpers.IsValidEmail("test.user@domain.co.uk"));
        }

        [Fact]
        public void IsValidEmail_WithInvalidEmail_ReturnsFalse()
        {
            // Act & Assert
            Assert.False(ValidationHelpers.IsValidEmail("notanemail"));
            Assert.False(ValidationHelpers.IsValidEmail("@example.com"));
            Assert.False(ValidationHelpers.IsValidEmail(""));
        }

        [Fact]
        public void IsValidPassword_WithStrongPassword_ReturnsTrue()
        {
            // Act & Assert
            Assert.True(ValidationHelpers.IsValidPassword("StrongPass123"));
            Assert.True(ValidationHelpers.IsValidPassword("MyPass@Word1"));
        }

        [Fact]
        public void IsValidPassword_WithWeakPassword_ReturnsFalse()
        {
            // Act & Assert
            Assert.False(ValidationHelpers.IsValidPassword("weak"));
            Assert.False(ValidationHelpers.IsValidPassword("onlyletters"));
            Assert.False(ValidationHelpers.IsValidPassword("123456"));
        }

        [Fact]
        public void IsValidChatName_WithValidName_ReturnsTrue()
        {
            // Act & Assert
            Assert.True(ValidationHelpers.IsValidChatName("My Chat"));
            Assert.True(ValidationHelpers.IsValidChatName("Chat-Group_1"));
        }

        [Fact]
        public void IsValidChatName_WithInvalidName_ReturnsFalse()
        {
            // Act & Assert
            Assert.False(ValidationHelpers.IsValidChatName(""));
            Assert.False(ValidationHelpers.IsValidChatName("Chat#With#Special"));
        }

        [Fact]
        public void IsValidMessage_WithValidMessage_ReturnsTrue()
        {
            // Act & Assert
            Assert.True(ValidationHelpers.IsValidMessage("Hello, world!"));
            Assert.True(ValidationHelpers.IsValidMessage("This is a valid message"));
        }

        [Fact]
        public void IsValidMessage_WithEmptyMessage_ReturnsFalse()
        {
            // Act & Assert
            Assert.False(ValidationHelpers.IsValidMessage(""));
            Assert.False(ValidationHelpers.IsValidMessage("   "));
        }
    }

    // ===== Message Model Tests =====
    public class MessageModelRealTests
    {
        [Fact]
        public void IsValid_WithCompleteMessage_ReturnsTrue()
        {
            // Arrange
            var message = new Message
            {
                SenderId = "user1",
                ChatId = "chat1",
                Content = "Valid message content",
                Timestamp = DateTime.Now,
                IsRead = false
            };
            
            // Act & Assert
            Assert.True(message.IsValid());
        }

        [Fact]
        public void IsValid_WithEmptyContent_ReturnsFalse()
        {
            // Arrange
            var message = new Message
            {
                SenderId = "user1",
                ChatId = "chat1",
                Content = "",
                Timestamp = DateTime.Now
            };
            
            // Act & Assert
            Assert.False(message.IsValid());
        }

        [Fact]
        public void IsValid_WithTooLongContent_ReturnsFalse()
        {
            // Arrange
            var message = new Message
            {
                SenderId = "user1",
                ChatId = "chat1",
                Content = new string('a', 4097),
                Timestamp = DateTime.Now
            };
            
            // Act & Assert
            Assert.False(message.IsValid());
        }

        [Fact]
        public void IsValid_WithoutSenderId_ReturnsFalse()
        {
            // Arrange
            var message = new Message
            {
                SenderId = null,
                ChatId = "chat1",
                Content = "Message",
                Timestamp = DateTime.Now
            };
            
            // Act & Assert
            Assert.False(message.IsValid());
        }
    }

    // ===== Chat Model Tests =====
    public class ChatModelRealTests
    {
        [Fact]
        public void IsValid_WithCompleteChat_ReturnsTrue()
        {
            // Arrange
            var chat = new Chat
            {
                Name = "Test Chat",
                ParticipantIds = new List<string> { "user1", "user2" },
                CreatedAt = DateTime.Now,
                IsFavorite = false
            };
            
            // Act & Assert
            Assert.True(chat.IsValid());
        }

        [Fact]
        public void IsValid_WithOnlyOneParticipant_ReturnsFalse()
        {
            // Arrange
            var chat = new Chat
            {
                Name = "Invalid Chat",
                ParticipantIds = new List<string> { "user1" },
                CreatedAt = DateTime.Now
            };
            
            // Act & Assert
            Assert.False(chat.IsValid());
        }

        [Fact]
        public void IsValid_WithTooLongName_ReturnsFalse()
        {
            // Arrange
            var chat = new Chat
            {
                Name = new string('a', 256),
                ParticipantIds = new List<string> { "user1", "user2" },
                CreatedAt = DateTime.Now
            };
            
            // Act & Assert
            Assert.False(chat.IsValid());
        }

        [Fact]
        public void ContainsUser_WithExistingUser_ReturnsTrue()
        {
            // Arrange
            var chat = new Chat
            {
                Name = "Test Chat",
                ParticipantIds = new List<string> { "user1", "user2", "user3" }
            };
            
            // Act & Assert
            Assert.True(chat.ContainsUser("user2"));
        }

        [Fact]
        public void ContainsUser_WithNonExistingUser_ReturnsFalse()
        {
            // Arrange
            var chat = new Chat
            {
                Name = "Test Chat",
                ParticipantIds = new List<string> { "user1", "user2" }
            };
            
            // Act & Assert
            Assert.False(chat.ContainsUser("user99"));
        }
    }

    // ===== User Model Tests =====
    public class UserModelRealTests
    {
        [Fact]
        public void IsValid_WithCompleteUser_ReturnsTrue()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                CreatedAt = DateTime.Now,
                IsOnline = false
            };
            
            // Act & Assert
            Assert.True(user.IsValid());
        }

        [Fact]
        public void IsValid_WithShortUsername_ReturnsFalse()
        {
            // Arrange
            var user = new User
            {
                Username = "ab",
                Email = "test@example.com"
            };
            
            // Act & Assert
            Assert.False(user.IsValid());
        }

        [Fact]
        public void IsValid_WithoutEmail_ReturnsFalse()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = ""
            };
            
            // Act & Assert
            Assert.False(user.IsValid());
        }
    }

    // ===== AvatarUrlToImageSourceConverter Tests =====
    public class AvatarConverterRealTests
    {
        [Fact]
        public void ConvertUrl_WithValidHttpsUrl_ReturnsTrue()
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();
            
            // Act
            var result = converter.ConvertUrl("https://example.com/avatar.jpg");
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ConvertUrl_WithValidHttpUrl_ReturnsTrue()
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();
            
            // Act
            var result = converter.ConvertUrl("http://example.com/avatar.png");
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ConvertUrl_WithInvalidUrl_ReturnsFalse()
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();
            
            // Act
            var result = converter.ConvertUrl("not a valid url");
            
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ConvertUrl_WithEmptyUrl_ReturnsFalse()
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();
            
            // Act
            var result = converter.ConvertUrl("");
            
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ConvertUrl_WithFileScheme_ReturnsFalse()
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();
            
            // Act
            var result = converter.ConvertUrl("file://C:/Users/avatar.jpg");
            
            // Assert
            Assert.False(result);
        }
    }

    // ===== ThemeManager Tests =====
    public class ThemeManagerRealTests
    {
        [Fact]
        public void ApplyTheme_WithValidHexColors_Succeeds()
        {
            // Act - should not throw
            ThemeManager.ApplyTheme("#FF6A359C", "#FF4A00E0", "#FF8E2DE2");
        }

        [Fact]
        public void ApplyTheme_With6DigitHexColors_Succeeds()
        {
            // Act - should not throw
            ThemeManager.ApplyTheme("#6A359C", "#4A00E0", "#8E2DE2");
        }

        [Fact]
        public void ApplyTheme_WithInvalidHexFormat_AppliesDefaultTheme()
        {
            // Act - should not throw, will apply default
            ThemeManager.ApplyTheme("NotAHex", "AlsoNotHex", "StillNotHex");
        }

        [Fact]
        public void ApplyTheme_WithNullColor_AppliesDefaultTheme()
        {
            // Act - should not throw
            ThemeManager.ApplyTheme("#6A359C", null, "#8E2DE2");
        }
    }

    // ===== ImageUploader Tests =====
    public class ImageUploaderRealTests
    {
        [Fact]
        public void ValidateImagePath_WithJpgFile_ReturnsTrue()
        {
            // Arrange
            var uploader = new ImageUploader();
            
            // Act & Assert
            Assert.True(uploader.ValidateImagePath("image.jpg"));
            Assert.True(uploader.ValidateImagePath("photo.JPG"));
        }

        [Fact]
        public void ValidateImagePath_WithPngFile_ReturnsTrue()
        {
            // Arrange
            var uploader = new ImageUploader();
            
            // Act & Assert
            Assert.True(uploader.ValidateImagePath("image.png"));
        }

        [Fact]
        public void ValidateImagePath_WithUnsupportedFormat_ReturnsFalse()
        {
            // Arrange
            var uploader = new ImageUploader();
            
            // Act & Assert
            Assert.False(uploader.ValidateImagePath("file.txt"));
            Assert.False(uploader.ValidateImagePath("file.exe"));
        }

        [Fact]
        public void ValidateImagePath_WithEmptyPath_ReturnsFalse()
        {
            // Arrange
            var uploader = new ImageUploader();
            
            // Act & Assert
            Assert.False(uploader.ValidateImagePath(""));
        }

        [Fact]
        public async Task UploadImageAsync_WithValidImageUrl_Succeeds()
        {
            // Arrange
            var uploader = new ImageUploader();
            
            // Act
            var result = await uploader.UploadImageAsync("https://example.com/image.jpg", "http://server/upload");
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UploadImageAsync_WithInvalidImageUrl_Fails()
        {
            // Arrange
            var uploader = new ImageUploader();
            
            // Act
            var result = await uploader.UploadImageAsync("not-a-url", "http://server/upload");
            
            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UploadImageAsync_WithEmptyImageUrl_Fails()
        {
            // Arrange
            var uploader = new ImageUploader();
            
            // Act
            var result = await uploader.UploadImageAsync("", "http://server/upload");
            
            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UploadImageAsync_WithUnsupportedFormat_Fails()
        {
            // Arrange
            var uploader = new ImageUploader();
            
            // Act
            var result = await uploader.UploadImageAsync("https://example.com/file.txt", "http://server/upload");
            
            // Assert
            Assert.False(result);
        }
    }
}
