using System.Text;
using Lame.Backend.Exports.Exporters;
using Lame.Backend.Exports.Models;

namespace Lame.Backend.Exports.Tests.ExporterTests;

public class Xliff12ExporterTests
{
    [Fact]
    public void Export_WithSingleRecord_ProducesValidXliff()
    {
        // Arrange
        var exporter = new Xliff12Exporter();
        var records = new List<AssetExportData>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SourceTranslation = new TranslationExportData { Content = "Hello" },
                TargetTranslation = new TranslationExportData { Content = "Bonjour" },
                Context = "Greeting",
                InternalName = "asset1"
            }
        };

        // Act
        var result = exporter.Export(records, "en", "fr");

        // Assert
        var xml = Encoding.UTF8.GetString(result);

        Assert.Contains("<source>Hello</source>", xml);
        Assert.Contains("<target>Bonjour</target>", xml);
        Assert.Contains("<note from=\"context\">Greeting</note>", xml);
        Assert.Contains("<note from=\"internal-name\">asset1</note>", xml);
        Assert.Contains("source-language=\"en\"", xml);
        Assert.Contains("target-language=\"fr\"", xml);
    }

    [Fact]
    public void Export_WithMultipleRecords_ProducesMultipleTransUnits()
    {
        // Arrange
        var exporter = new Xliff12Exporter();
        var records = new List<AssetExportData>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SourceTranslation = new TranslationExportData { Content = "One" },
                TargetTranslation = new TranslationExportData { Content = "Un" },
                Context = "Number",
                InternalName = "asset1"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SourceTranslation = new TranslationExportData { Content = "Two" },
                TargetTranslation = new TranslationExportData { Content = "Deux" },
                Context = "Number",
                InternalName = "asset2"
            }
        };

        // Act
        var result = exporter.Export(records, "en", "fr");

        // Assert
        var xml = Encoding.UTF8.GetString(result);

        Assert.Contains("<source>One</source>", xml);
        Assert.Contains("<target>Un</target>", xml);
        Assert.Contains("<source>Two</source>", xml);
        Assert.Contains("<target>Deux</target>", xml);
        Assert.Equal(2, xml.Split("<trans-unit").Length - 1);
    }

    [Fact]
    public void Export_WithNullSourceOrTargetTranslation_ProducesEmptyElements()
    {
        // Arrange
        var exporter = new Xliff12Exporter();
        var records = new List<AssetExportData>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SourceTranslation = null,
                TargetTranslation = null,
                Context = "NoTrans",
                InternalName = "asset3"
            }
        };

        // Act
        var result = exporter.Export(records, "en", "fr");

        // Assert
        var xml = Encoding.UTF8.GetString(result);

        Assert.Contains("<source></source>", xml);
        Assert.Contains("<target></target>", xml);
    }

    [Fact]
    public void Export_WithSpecialCharacters_EscapesXmlCorrectly()
    {
        // Arrange
        var exporter = new Xliff12Exporter();
        var records = new List<AssetExportData>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SourceTranslation = new TranslationExportData { Content = "<tag>&\"'" },
                TargetTranslation = new TranslationExportData { Content = "<target>&\"'" },
                Context = "<context>&\"'",
                InternalName = "<internal>&\"'"
            }
        };

        // Act
        var result = exporter.Export(records, "en", "fr");

        // Assert
        var xml = Encoding.UTF8.GetString(result);

        Assert.Contains("&lt;tag&gt;&amp;&quot;&apos;", xml);
        Assert.Contains("&lt;target&gt;&amp;&quot;&apos;", xml);
        Assert.Contains("&lt;context&gt;&amp;&quot;&apos;", xml);
        Assert.Contains("&lt;internal&gt;&amp;&quot;&apos;", xml);
    }

    [Fact]
    public void Export_WithEmptyRecords_ReturnsXliffWithNoTransUnits()
    {
        // Arrange
        var exporter = new Xliff12Exporter();
        var records = new List<AssetExportData>();

        // Act
        var result = exporter.Export(records, "en", "fr");

        // Assert
        var xml = Encoding.UTF8.GetString(result);

        Assert.Contains("<body>", xml);
        Assert.DoesNotContain("<trans-unit", xml);
        Assert.Contains("</xliff>", xml);
    }
}