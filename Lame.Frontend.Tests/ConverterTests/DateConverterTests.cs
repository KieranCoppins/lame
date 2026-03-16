using System.Globalization;
using Lame.Frontend.Helpers.Converters;

namespace Lame.Frontend.Tests.ConverterTests;

public class DateConverterTests
{
    [Fact]
    public void Convert_WithValidDateTime_ReturnsFormattedString()
    {
        // Arrange
        var converter = new DateConverter();
        var date = new DateTime(2000, 9, 28);

        // Act
        var result = converter.Convert(date, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("2000-09-28", result);
    }

    [Fact]
    public void Convert_WithNullValue_ReturnsNull()
    {
        // Arrange
        var converter = new DateConverter();

        // Act
        var result = converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Convert_WithNonDateTimeValue_ReturnsNull()
    {
        // Arrange
        var converter = new DateConverter();

        // Act
        var result = converter.Convert("not a date", typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Null(result);
    }
}