using System.Globalization;
using Lame.Frontend.Helpers.Converters;

namespace Lame.Frontend.Tests.ConverterTests;

public class LanguageCodeToEnglishConverterTests
{
    [Fact]
    public void Convert_ReturnsEnglishName_WhenGivenValidAlpha2Code()
    {
        // Arrange
        var converter = new LanguageCodeToEnglishConverter();
        var code = "fr";

        // Act
        var result = converter.Convert(code, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("French", result);
    }

    [Fact]
    public void Convert_ReturnsNull_WhenValueIsNull()
    {
        // Arrange
        var converter = new LanguageCodeToEnglishConverter();

        // Act
        var result = converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Convert_ThrowsInvalidCastException_WhenValueIsNotString()
    {
        // Arrange
        var converter = new LanguageCodeToEnglishConverter();

        // Act & Assert
        Assert.Throws<InvalidCastException>(() =>
            converter.Convert(123, typeof(string), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ConvertBack_ReturnsAlpha2Code_WhenGivenValidEnglishName()
    {
        // Arrange
        var converter = new LanguageCodeToEnglishConverter();
        var englishName = "French";

        // Act
        var result = converter.ConvertBack(englishName, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("fr", result);
    }

    [Fact]
    public void ConvertBack_ReturnsNull_WhenValueIsNotString()
    {
        // Arrange
        var converter = new LanguageCodeToEnglishConverter();

        // Act
        var result = converter.ConvertBack(123, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Null(result);
    }
}