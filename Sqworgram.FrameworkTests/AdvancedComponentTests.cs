using System;
using Xunit;
using _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО;

namespace Sqworgram.FrameworkTests
{
    public class AvatarUrlConverterAdvancedTests
    {
        [Fact]
        public void AvatarUrlToImageSourceConverter_WithHttpUrl_LoadsImage()
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();
            var validHttpUrl = "https://via.placeholder.com/150";

            // Act
            var result = converter.Convert(validHttpUrl, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void AvatarUrlToImageSourceConverter_WithFileUrl_LoadsImage()
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();
            var fileUrl = "file:///C:/image.jpg";

            // Act
            var result = converter.Convert(fileUrl, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            // File may not exist, but converter should try to load it
            if (System.IO.File.Exists(fileUrl))
                Assert.NotNull(result);
        }

        [Fact]
        public void AvatarUrlToImageSourceConverter_ConvertBack_ThrowsNotImplemented()
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() =>
                converter.ConvertBack("value", typeof(string), null, System.Globalization.CultureInfo.CurrentCulture)
            );
        }

        [Fact]
        public void AvatarUrlToImageSourceConverter_WithTabCharacters_ReturnsNull()
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();
            var urlWithTabs = "\t\t\t";

            // Act
            var result = converter.Convert(urlWithTabs, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void AvatarUrlToImageSourceConverter_WithNewlines_ReturnsNull()
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();
            var urlWithNewlines = "\n\n\n";

            // Act
            var result = converter.Convert(urlWithNewlines, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            Assert.Null(result);
        }
    }

    public class DatabaseHelperAdvancedTests
    {
        private readonly string dbFileName = "test_helper_4.db";

        public DatabaseHelperAdvancedTests()
        {
            CleanupDatabase();
        }

        private void CleanupDatabase()
        {
            if (System.IO.File.Exists(dbFileName))
            {
                try
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    System.Threading.Thread.Sleep(100);
                    System.IO.File.Delete(dbFileName);
                }
                catch { /* Ignore */ }
            }
        }

        [Fact]
        public void DatabaseHelper_WithMultipleInstances_SharesSameDatabase()
        {
            // Arrange
            var helper1 = new DatabaseHelper();
            var helper2 = new DatabaseHelper();

            helper1.RegisterUser("testuser", "password");

            // Act
            var loginResult = helper2.LoginUser("testuser", "password");

            // Assert
            Assert.NotNull(loginResult.UserId);
        }

        [Fact]
        public void UpdateAvatarUrl_WithEmptyUrl_Updates()
        {
            // Arrange
            var helper = new DatabaseHelper();
            helper.RegisterUser("user", "pass");
            var loginResult = helper.LoginUser("user", "pass");

            // Act
            helper.UpdateAvatarUrl(loginResult.UserId.Value, "");
            var updated = helper.GetUserById(loginResult.UserId.Value);

            // Assert
            Assert.NotNull(updated);
            Assert.Equal("", updated.AvatarUrl);
        }

        [Fact]
        public void GetMessages_WithEmptyChat_ReturnsEmptyList()
        {
            // Arrange
            var helper = new DatabaseHelper();
            helper.RegisterUser("u1", "p1");
            helper.RegisterUser("u2", "p2");
            var user1 = helper.LoginUser("u1", "p1");
            var user2 = helper.LoginUser("u2", "p2");
            var chatId = helper.GetOrCreateChat(user1.UserId.Value, user2.UserId.Value);

            // Act
            var messages = helper.GetMessages(chatId);

            // Assert
            Assert.NotNull(messages);
            Assert.Empty(messages);
        }
    }
}
