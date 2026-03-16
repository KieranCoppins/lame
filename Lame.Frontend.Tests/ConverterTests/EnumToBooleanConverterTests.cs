using System.Globalization;
using System.Windows.Data;
using Lame.Frontend.Helpers.Converters;

namespace Lame.Frontend.Tests.ConverterTests;

public class EnumToBooleanConverterTests
{
    [Fact]
    public void Convert_ReturnsTrue_WhenValueAndParameterAreEqualEnumNames()
    {
        // Arrange
        var converter = new EnumToBooleanConverter();

        // Act
        var result = converter.Convert(DayOfWeek.Monday, typeof(bool), "Monday", CultureInfo.InvariantCulture);

        // Assert
        Assert.True((bool)result!);
    }

    [Fact]
    public void Convert_ReturnsFalse_WhenValueAndParameterAreDifferentEnumNames()
    {
        // Arrange
        var converter = new EnumToBooleanConverter();

        // Act
        var result = converter.Convert(DayOfWeek.Monday, typeof(bool), "Tuesday", CultureInfo.InvariantCulture);

        // Assert
        Assert.False((bool)result!);
    }

    [Fact]
    public void Convert_ReturnsFalse_WhenValueIsNull()
    {
        // Arrange
        var converter = new EnumToBooleanConverter();

        // Act
        var result = converter.Convert(null, typeof(bool), "Monday", CultureInfo.InvariantCulture);

        // Assert
        Assert.False((bool)result!);
    }

    [Fact]
    public void Convert_ReturnsFalse_WhenParameterIsNull()
    {
        // Arrange
        var converter = new EnumToBooleanConverter();

        // Act
        var result = converter.Convert(DayOfWeek.Monday, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.False((bool)result!);
    }

    [Fact]
    public void ConvertBack_ReturnsEnumValue_WhenValueIsTrueAndParameterIsValid()
    {
        // Arrange
        var converter = new EnumToBooleanConverter();

        // Act
        var result = converter.ConvertBack(true, typeof(DayOfWeek), "Monday", CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(DayOfWeek.Monday, result);
    }

    [Fact]
    public void ConvertBack_ReturnsBindingDoNothing_WhenValueIsFalse()
    {
        // Arrange
        var converter = new EnumToBooleanConverter();

        // Act
        var result = converter.ConvertBack(false, typeof(DayOfWeek), "Monday", CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void ConvertBack_ReturnsBindingDoNothing_WhenParameterIsNull()
    {
        // Arrange
        var converter = new EnumToBooleanConverter();

        // Act
        var result = converter.ConvertBack(true, typeof(DayOfWeek), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void ConvertBack_ThrowsArgumentException_WhenParameterIsInvalidEnumName()
    {
        // Arrange
        var converter = new EnumToBooleanConverter();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            converter.ConvertBack(true, typeof(DayOfWeek), "NotAValidDay", CultureInfo.InvariantCulture));
    }
}