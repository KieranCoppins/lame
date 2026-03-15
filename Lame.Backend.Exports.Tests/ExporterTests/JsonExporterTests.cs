using System.Text;
using Lame.Backend.Exports.Exporters;
using Lame.Backend.Exports.Models;

namespace Lame.Backend.Exports.Tests.ExporterTests;

public class JsonExporterTests
{
    [Fact]
    public void Export_WithSingleRecord_ReturnsExpectedJson()
    {
        // Arrange
        var exporter = new JsonExporter();
        var records = new List<AssetExportData>
        {
            new()
            {
                InternalName = "asset1",
                TargetTranslation = new TranslationExportData { Content = "Hello" }
            }
        };

        // Act
        var result = exporter.Export(records, "en", "fr");

        // Assert
        var json = Encoding.UTF8.GetString(result);

        Assert.Contains("\"InternalName\": \"asset1\"", json);
        Assert.Contains("\"Content\": \"Hello\"", json);
    }

    [Fact]
    public void Export_WithMultipleRecords_ReturnsJsonArrayWithAllRecords()
    {
        // Arrange
        var exporter = new JsonExporter();
        var records = new List<AssetExportData>
        {
            new()
            {
                InternalName = "asset1",
                TargetTranslation = new TranslationExportData { Content = "Hello" }
            },
            new()
            {
                InternalName = "asset2",
                TargetTranslation = new TranslationExportData { Content = "Bonjour" }
            }
        };

        // Act
        var result = exporter.Export(records, "en", "fr");

        // Assert
        var json = Encoding.UTF8.GetString(result);

        Assert.Contains("\"InternalName\": \"asset1\"", json);
        Assert.Contains("\"Content\": \"Hello\"", json);
        Assert.Contains("\"InternalName\": \"asset2\"", json);
        Assert.Contains("\"Content\": \"Bonjour\"", json);
    }

    [Fact]
    public void Export_WithNullTargetTranslation_SerializesContentAsNull()
    {
        // Arrange
        var exporter = new JsonExporter();
        var records = new List<AssetExportData>
        {
            new()
            {
                InternalName = "asset1",
                TargetTranslation = null
            }
        };

        // Act
        var result = exporter.Export(records, "en", "fr");

        // Assert
        var json = Encoding.UTF8.GetString(result);

        Assert.Contains("\"InternalName\": \"asset1\"", json);
        Assert.Contains("\"Content\": null", json);
    }

    [Fact]
    public void Export_WithEmptyRecords_ReturnsEmptyJsonArray()
    {
        // Arrange
        var exporter = new JsonExporter();
        var records = new List<AssetExportData>();

        // Act
        var result = exporter.Export(records, "en", "fr");

        // Assert
        var json = Encoding.UTF8.GetString(result);

        Assert.Equal("[]", json.Trim());
    }

    [Fact]
    public void Export_WithNullRecords_ThrowsArgumentNullException()
    {
        // Arrange
        var exporter = new JsonExporter();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exporter.Export(null, "en", "fr"));
    }
}