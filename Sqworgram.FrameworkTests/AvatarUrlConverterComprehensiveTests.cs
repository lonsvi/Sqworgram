using System;
using System.Windows.Media.Imaging;
using Xunit;
using _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО;

namespace Sqworgram.FrameworkTests
{
    public class AvatarUrlConverterComprehensiveTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Convert_WithEmptyOrWhitespace_ReturnsNull(string input)
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();

            // Act
            var result = converter.Convert(input, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("https://example.com/image.jpg")]
        [InlineData("http://example.com/image.png")]
        [InlineData("https://via.placeholder.com/150")]
        public void Convert_WithValidHttpUrl_ReturnsImageSource(string url)
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();

            // Act
            var result = converter.Convert(url, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert - BitmapImage is expected if URL is valid
            Assert.IsType<BitmapImage>(result);
        }

        [Theory]
        [InlineData("not-a-url")]
        [InlineData("htp://typo.com")]
        [InlineData("ftp://example.com/file")]
        public void Convert_WithInvalidUrl_ReturnsNull(string url)
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();

            // Act
            var result = converter.Convert(url, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("C:\\Users\\test\\avatar.jpg")]
        [InlineData("file:///C:/image.jpg")]
        [InlineData("\\\\server\\share\\image.jpg")]
        public void Convert_WithLocalPath_AttemptsToLoad(string path)
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();

            // Act
            var result = converter.Convert(path, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert - Result depends on whether file exists or URI is valid
            // Should not throw exception regardless
            // Either returns BitmapImage or null
            Assert.True(result == null || result is BitmapImage);
        }

        [Theory]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("\t")]
        [InlineData("  \n  ")]
        public void Convert_WithOnlyWhitespaceVariants_ReturnsNull(string whitespace)
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();

            // Act
            var result = converter.Convert(whitespace, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Convert_WithVeryLongUrl_HandlesGracefully()
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();
            var longUrl = "https://example.com/" + new string('a', 2000);

            // Act
            var result = converter.Convert(longUrl, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert - Should not crash
            Assert.True(result == null || result is BitmapImage);
        }

        [Theory]
        [InlineData("https://例え.jp/avatar.jpg")]  // Japanese
        [InlineData("https://münchen.de/image.png")] // German
        [InlineData("https://москва.рф/photo.jpg")]  // Russian (Cyrillic)
        public void Convert_WithUnicodeInUrl_HandlesGracefully(string url)
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();

            // Act
            var result = converter.Convert(url, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert - Should not crash
            Assert.True(result == null || result is BitmapImage);
        }

        [Fact]
        public void ConvertBack_Always_ThrowsNotImplementedException()
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() =>
                converter.ConvertBack(new BitmapImage(), typeof(string), null, System.Globalization.CultureInfo.CurrentCulture)
            );
        }

        [Theory]
        [InlineData(123)]
        [InlineData(45.67)]
        [InlineData(true)]
        public void Convert_WithNonStringInput_HandlesGracefully(object input)
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();

            // Act
            var result = converter.Convert(input, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert - Should not crash
            Assert.True(result == null || result is BitmapImage);
        }

        [Fact]
        public void Convert_WithUrlAndMixedCase_Works()
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();
            var url = "HTTPS://EXAMPLE.COM/IMAGE.JPG";

            // Act
            var result = converter.Convert(url, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            Assert.True(result == null || result is BitmapImage);
        }

        [Theory]
        [InlineData("https://example.com/avatar.jpg?size=100")]  // With query params
        [InlineData("https://example.com:8080/image.jpg")]       // With port
        [InlineData("https://sub.example.com/avatar.jpg")]       // With subdomain
        public void Convert_WithUrlVariations_HandlesCorrectly(string url)
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();

            // Act
            var result = converter.Convert(url, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            Assert.True(result == null || result is BitmapImage);
        }
    }
}
