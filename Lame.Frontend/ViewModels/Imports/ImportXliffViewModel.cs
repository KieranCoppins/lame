using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Lame.DomainModel;
using Lame.Frontend.Services;

namespace Lame.Frontend.ViewModels.Imports;

public class ImportXliffViewModel : BaseViewModel, IImportViewModel
{
    private readonly ISystemIO _systemIo;

    public ImportXliffViewModel(ISystemIO systemIO)
    {
        _systemIo = systemIO;
    }

    public async Task<List<ImportData>> GetImportData(string fileName)
    {
        var data = await _systemIo.ReadAllBytesAsync(fileName);

        using var stream = new MemoryStream(data);

        var xliffDoc = XDocument.Load(stream);

        var ns = xliffDoc.Root?.Name.Namespace ?? XNamespace.None;

        // Determine the source and target languages
        var xliffElement = xliffDoc.Element(ns + "xliff") ??
                           throw new Exception("Invalid XLIFF file: missing <xliff> root element");

        var fileElement = xliffElement.Element(ns + "file") ??
                          throw new Exception("Invalid XLIFF file: missing <file> element");

        var sourceLanguage = fileElement.Attribute("source-language")?.Value;
        if (sourceLanguage != "en") throw new Exception("Invalid XLIFF file: source language must be english");

        var targetLanguage = fileElement.Attribute("target-language")?.Value;
        if (string.IsNullOrEmpty(targetLanguage))
            throw new Exception("Invalid XLIFF file: target-language must be specified");

        var xliffTransUnits = xliffDoc.Descendants(ns + "trans-unit");

        var importDataList = new List<ImportData>();

        foreach (var transUnit in xliffTransUnits)
        {
            if (!Guid.TryParse(transUnit.Attribute("id")?.Value, out var transUnitId))
                // TODO provide some kind of warning that there was a missing translation unit guid id
                continue;

            var internalName = transUnit.Attribute("resname")?.Value;
            if (string.IsNullOrWhiteSpace(internalName)) continue;

            var source = transUnit.Element(ns + "source")?.Value;
            var target = transUnit.Element(ns + "target")?.Value;
            var notes = transUnit.Element(ns + "note")?.Value;

            var transData = new ImportData
            {
                Asset = new Asset
                {
                    Id = transUnitId,
                    ContextNotes = notes,
                    InternalName = internalName,
                    AssetType = AssetType.Text
                },

                // Ignore setting version here as we don't know
                SourceTranslation = new Translation
                {
                    AssetId = transUnitId,
                    Id = Guid.NewGuid(),
                    Content = source,
                    Language = sourceLanguage
                },
                TargetTranslation = new Translation
                {
                    AssetId = transUnitId,
                    Id = Guid.NewGuid(),
                    Content = target,
                    Language = targetLanguage
                }
            };

            importDataList.Add(transData);
        }

        return importDataList;
    }
}