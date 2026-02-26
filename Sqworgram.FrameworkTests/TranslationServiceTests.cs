using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО;

namespace Sqworgram.FrameworkTests
{
    public class TranslationServiceTests
    {
        [Fact]
        public async Task TranslateTextAsync_WithValidInput_ReturnsNonNull()
        {
            // Arrange
            var service = new TranslationService();

            // Act
            var result = await service.TranslateTextAsync("hello", "en", "es");

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task TranslateTextAsync_WithEmptyText_ReturnsError()
        {
            // Arrange
            var service = new TranslationService();

            // Act
            var result = await service.TranslateTextAsync("", "en", "es");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task TranslateTextAsync_WithSameLanguages_Returns()
        {
            // Arrange
            var service = new TranslationService();

            // Act
            var result = await service.TranslateTextAsync("test", "en", "en");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task TranslateTextAsync_WithSpecialCharacters_ReturnsResult()
        {
            // Arrange
            var service = new TranslationService();
            var textWithSpecialChars = "Hello @#$%";

            // Act
            var result = await service.TranslateTextAsync(textWithSpecialChars, "en", "fr");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task TranslateTextAsync_WithVeryLongText_Returns()
        {
            // Arrange
            var service = new TranslationService();
            var longText = new string('a', 500);

            // Act
            var result = await service.TranslateTextAsync(longText, "en", "de");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task TranslateTextAsync_WithUnicodeText_ReturnsResult()
        {
            // Arrange
            var service = new TranslationService();
            var unicodeText = "مرحبا";

            // Act
            var result = await service.TranslateTextAsync(unicodeText, "ar", "en");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task TranslateTextAsync_WithCyrillicText_ReturnsResult()
        {
            // Arrange
            var service = new TranslationService();

            // Act
            var result = await service.TranslateTextAsync("привет", "ru", "en");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task TranslateTextAsync_WithHtmlContent_ReturnsResult()
        {
            // Arrange
            var service = new TranslationService();
            var htmlText = "<p>Hello</p>";

            // Act
            var result = await service.TranslateTextAsync(htmlText, "en", "es");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task TranslateTextAsync_WithNumbers_ReturnsResult()
        {
            // Arrange
            var service = new TranslationService();

            // Act
            var result = await service.TranslateTextAsync("12345", "en", "fr");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task TranslateTextAsync_WithWhitespace_ReturnsResult()
        {
            // Arrange
            var service = new TranslationService();
            var textWithSpaces = "   hello   world   ";

            // Act
            var result = await service.TranslateTextAsync(textWithSpaces, "en", "de");

            // Assert
            Assert.NotNull(result);
        }
    }
}
