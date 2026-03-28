using Lame.Backend.EntityFramework.Models;
using Lame.DomainModel;

namespace Lame.Backend.EntityFramework.Tests.EntityBuilders;

public class ChangeLogEntityBuilder
{
    private readonly ChangeLogEntity _changeLogEntry;

    public ChangeLogEntityBuilder()
    {
        _changeLogEntry = new ChangeLogEntity
        {
            Id = Guid.NewGuid(),
            ResourceId = null,
            Date = DateTime.UtcNow,
            Message = "Default change log entry message",
            ResourceAction = ResourceAction.Updated,
            ResourceType = ResourceType.Asset
        };
    }

    public ChangeLogEntity Build()
    {
        return _changeLogEntry;
    }

    public ChangeLogEntityBuilder WithResourceId(Guid resourceId)
    {
        _changeLogEntry.ResourceId = resourceId;
        return this;
    }

    public ChangeLogEntityBuilder WithId(Guid id)
    {
        _changeLogEntry.Id = id;
        return this;
    }

    public ChangeLogEntityBuilder WithDate(DateTime date)
    {
        _changeLogEntry.Date = date;
        return this;
    }

    public ChangeLogEntityBuilder WithMessage(string msg)
    {
        _changeLogEntry.Message = msg;
        return this;
    }

    public ChangeLogEntityBuilder WithResourceAction(ResourceAction resourceAction)
    {
        _changeLogEntry.ResourceAction = resourceAction;
        return this;
    }

    public ChangeLogEntityBuilder WithResourceType(ResourceType resourceType)
    {
        _changeLogEntry.ResourceType = resourceType;
        return this;
    }
}