namespace Lame.DomainModel;

public class ChangeLogEntry
{
    /// <summary>
    ///     The id of this change log entry
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The id of the resource that was modified
    /// </summary>
    public Guid? ResourceId { get; set; }

    /// <summary>
    ///     What resource was changed
    /// </summary>
    public ResourceType? ResourceType { get; set; }

    /// <summary>
    ///     What happened to this resource
    /// </summary>
    public ResourceAction ResourceAction { get; set; }

    /// <summary>
    ///     When did this change occur
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    ///     The message to display with the change
    /// </summary>
    public string Message { get; set; }
}