using Lame.Backend.EntityFramework.Models;
using Lame.DomainModel;

namespace Lame.Backend.EntityFramework;

public static class QueryHelpers
{
    public static IQueryable<TranslationEntity> LatestVersionPerLanguage(
        this IQueryable<TranslationEntity> query)
    {
        return query.Where(t => !query.Any(t2 =>
            t2.AssetId == t.AssetId &&
            t2.Language == t.Language &&
            (
                t2.MajorVersion > t.MajorVersion ||
                (t2.MajorVersion == t.MajorVersion && t2.MinorVersion > t.MinorVersion)
            )));
    }

    public static IQueryable<TranslationEntity> TargetTranslationPerLanguage(
        this IQueryable<AssetEntity> query)
    {
        return query
            .SelectMany(asset => asset.TargetedTranslations)
            .Select(target => target.Translation);
    }

    // TODO make this more generic so I dont have to keep redefining it for each entity type
    public static IQueryable<AssetEntity> SearchBy(
        this IQueryable<AssetEntity> query,
        string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        searchTerm = searchTerm.ToLower();

        return query
            .Where(a =>
                a.InternalName.ToLower().Contains(searchTerm) ||
                a.Tags.Any(t => t.Name.ToLower().Contains(searchTerm)))
            .OrderBy(a =>
                a.InternalName.ToLower() == searchTerm ? 0 :
                a.InternalName.ToLower().StartsWith(searchTerm) ? 1 :
                a.InternalName.ToLower().Contains(searchTerm) ? 2 :
                3)
            .ThenBy(a => a.InternalName);
    }

    public static IQueryable<AssetDto> AsDto(
        this IQueryable<AssetEntity> query)
    {
        return query.Select(entity => new AssetDto
        {
            Id = entity.Id,
            AssetType = entity.AssetType,
            ContextNotes = entity.ContextNotes,
            InternalName = entity.InternalName,
            LastUpdatedAt = entity.LastUpdatedAt,
            CreatedAt = entity.CreatedAt,
            Status = entity.Status,
            NumTranslations = entity.Translations
                // TODO we have a helper for this, but it's built on IQueryable
                // Filter only languages that are up to date with the english translation
                .Where(t =>
                    !entity.Translations.Any(english =>
                        english.AssetId == t.AssetId &&
                        english.Language == "en" &&
                        english.MajorVersion > t.MajorVersion))
                .Select(t => t.Language)
                .Distinct()
                .Count()
        });
    }

    public static IQueryable<TranslationDto> AsDto(
        this IQueryable<TranslationEntity> query)
    {
        return query.Select(translation => new TranslationDto
        {
            Id = translation.Id,
            AssetId = translation.AssetId,
            CreatedAt = translation.CreatedAt,
            Language = translation.Language,
            Content = translation.Content,
            MajorVersion = translation.MajorVersion,
            MinorVersion = translation.MinorVersion,
            Status = query.Any(english =>
                english.AssetId == translation.AssetId &&
                english.Language == "en" &&
                english.MajorVersion > translation.MajorVersion)
                ? TranslationStatus.Outdated
                : TranslationStatus.UpToDate
        });
    }

    public static IQueryable<TranslationEntity> WithUptoDateOnly(this IQueryable<TranslationEntity> query)
    {
        return query.Where(t =>
            // Translations are considered upto to date if major versions match, we do not care about minor versions
            !query.Any(english =>
                english.AssetId == t.AssetId &&
                english.Language == "en" &&
                english.MajorVersion > t.MajorVersion));
    }
}