namespace Lame.DomainModel;

public enum ExportTagFilterType
{
    /// <summary>
    ///     Exports assets with any of the tags provided in the filter
    /// </summary>
    Any,

    /// <summary>
    ///     Exports assets with all of the tags provided in the filter
    /// </summary>
    All,

    /// <summary>
    ///     Exports assets with only the tags provided in the filter, and no other tags
    /// </summary>
    Only
}