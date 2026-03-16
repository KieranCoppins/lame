using System.Globalization;
using Lame.Frontend.Helpers.Converters;

namespace Lame.Frontend.Tests.ConverterTests;

public class PercentageConverterTests
{
    [Fact]
    public void Convert_ReturnsCorrectPercentage_WhenValuesAreValid()
    {
        // Arrange
        var converter = new PercentageConverter();
        object[] values = { 25, 100 };

        // Act
        var result = converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(25, (double)result);
    }

    [Fact]
    public void Convert_ReturnsZero_WhenValuesLengthIsLessThanTwo()
    {
        // Arrange
        var converter = new PercentageConverter();
        object[] values = { 50 };

        // Act
        var result = converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Convert_ReturnsZero_WhenFirstValueIsNotParsable()
    {
        // Arrange
        var converter = new PercentageConverter();
        object[] values = { "notANumber", 100 };

        // Act
        var result = converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Convert_ReturnsZero_WhenSecondValueIsNotParsable()
    {
        // Arrange
        var converter = new PercentageConverter();
        object[] values = { 50, "notANumber" };

        // Act
        var result = converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Convert_ReturnsZero_WhenTotalIsZero()
    {
        // Arrange
        var converter = new PercentageConverter();
        object[] values = { 50, 0 };

        // Act
        var result = converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Convert_ReturnsZero_WhenTotalIsNegative()
    {
        // Arrange
        var converter = new PercentageConverter();
        object[] values = { 50, -10 };

        // Act
        var result = converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(0, result);
    }
}