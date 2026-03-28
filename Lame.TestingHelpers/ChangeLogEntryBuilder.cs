using Lame.DomainModel;

namespace Lame.TestingHelpers;

public class ChangeLogEntryBuilder
{
    private readonly ChangeLogEntry _changeLogEntry;

    public ChangeLogEntryBuilder()
    {
        _changeLogEntry = new ChangeLogEntry
        {
            Id = Guid.NewGuid(),
            ResourceId = null,
            Date = DateTime.UtcNow,
            Message = "Default change log entry message",
            ResourceAction = ResourceAction.Updated,
            ResourceType = ResourceType.Asset
        };
    }

    public ChangeLogEntry Build()
    {
        return _changeLogEntry;
    }

    public ChangeLogEntryBuilder WithResourceId(Guid resourceId)
    {
        _changeLogEntry.ResourceId = resourceId;
        return this;
    }

    public ChangeLogEntryBuilder WithId(Guid id)
    {
        _changeLogEntry.Id = id;
        return this;
    }

    public ChangeLogEntryBuilder WithDate(DateTime date)
    {
        _changeLogEntry.Date = date;
        return this;
    }

    public ChangeLogEntryBuilder WithMessage(string msg)
    {
        _changeLogEntry.Message = msg;
        return this;
    }

    public ChangeLogEntryBuilder WithResourceAction(ResourceAction resourceAction)
    {
        _changeLogEntry.ResourceAction = resourceAction;
        return this;
    }

    public ChangeLogEntryBuilder WithResourceType(ResourceType resourceType)
    {
        _changeLogEntry.ResourceType = resourceType;
        return this;
    }
}