using System;
using System.IO;
using Xunit;
using _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО;

namespace Sqworgram.FrameworkTests
{
    public class AvatarUrlConverterTests
    {
        [Fact]
        public void AvatarUrlToImageSourceConverter_WithNullInput_ReturnsNull()
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();

            // Act
            var result = converter.Convert(null, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void AvatarUrlToImageSourceConverter_WithEmptyString_ReturnsNull()
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();

            // Act
            var result = converter.Convert("", typeof(object), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void AvatarUrlToImageSourceConverter_WithWhitespace_ReturnsNull()
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();

            // Act
            var result = converter.Convert("   ", typeof(object), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void AvatarUrlToImageSourceConverter_ConvertBackThrowsNotImplemented()
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => 
                converter.ConvertBack(null, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
        }

        [Fact]
        public void AvatarUrlToImageSourceConverter_WithInvalidUrl_ReturnsNull()
        {
            // Arrange
            var converter = new AvatarUrlToImageSourceConverter();
            string invalidUrl = "not-a-valid-url";

            // Act
            var result = converter.Convert(invalidUrl, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            Assert.Null(result);
        }
    }
}
