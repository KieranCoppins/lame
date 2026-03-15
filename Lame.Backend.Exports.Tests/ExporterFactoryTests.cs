using Lame.Backend.Exports.Exporters;
using Lame.DomainModel;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Backend.Exports.Tests;

public class ExporterFactoryTests
{
    [Theory]
    [InlineData(ExportFormatType.JSON, typeof(JsonExporter))]
    [InlineData(ExportFormatType.XLIFF, typeof(Xliff12Exporter))]
    public void GetExporter_WithSupportedFormat_ReturnsCorrectExporter(ExportFormatType format, Type expectedType)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<JsonExporter>();
        services.AddTransient<Xliff12Exporter>();
        var provider = services.BuildServiceProvider();
        var factory = new ExporterFactory(provider);

        // Act
        var exporter = factory.GetExporter(format);

        // Assert
        Assert.IsType(expectedType, exporter);
    }

    [Fact]
    public void GetExporter_WithUnsupportedFormat_ThrowsNotSupportedException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<JsonExporter>();
        services.AddTransient<Xliff12Exporter>();
        var provider = services.BuildServiceProvider();
        var factory = new ExporterFactory(provider);

        var unsupportedValue = (ExportFormatType)999;

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => factory.GetExporter(unsupportedValue));
    }

    [Fact]
    public void GetExporter_AllExportFormatTypes_AreHandledByFactory()
    {
        // Assert
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        var factory = new ExporterFactory(provider);

        // Act & Assert
        foreach (ExportFormatType value in Enum.GetValues(typeof(ExportFormatType)))
            try
            {
                factory.GetExporter(value);
            }
            catch (InvalidOperationException ex)
            {
                // This exception is expected if the exporter is not registered in the service provider.
            }
            catch (NotSupportedException ex)
            {
                Assert.Fail("ExporterFactory threw an exception for ExportFormatType value: " + value +
                            ". Exception: " + ex);
            }
    }
}