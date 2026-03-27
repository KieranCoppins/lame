using Lame.DomainModel;

namespace Lame.Backend.ChangeLog;

public interface IChangeLog
{
    Task<PaginatedResponse<ChangeLogEntry>> Get(int page, int pageSize);
    Task Create(ChangeLogEntry changeLog);
}