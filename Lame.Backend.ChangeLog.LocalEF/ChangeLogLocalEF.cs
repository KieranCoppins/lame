using Lame.Backend.EntityFramework;
using Lame.Backend.EntityFramework.Models;
using Lame.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Backend.ChangeLog.LocalEF;

public class ChangeLogLocalEF : IChangeLog
{
    private readonly IServiceProvider _serviceProvider;
    private IChangeLog _changeLogImplementation;

    public ChangeLogLocalEF(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<PaginatedResponse<ChangeLogEntry>> Get(int page, int pageSize)
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var paginatedResponse = new PaginatedResponse<ChangeLogEntry>
            {
                Page = page
            };

            if (pageSize <= 0)
            {
                paginatedResponse.TotalPages = 0;
                paginatedResponse.Items = [];
                return paginatedResponse;
            }

            var logQuery = context.ChangeLogEntries
                .AsNoTracking()
                .OrderByDescending(c => c.Date);

            var totalLogs = await logQuery.CountAsync();

            var logEntries = await logQuery
                .Skip(page * pageSize)
                .Take(pageSize)
                .Select(c => (ChangeLogEntry)c)
                .ToListAsync();

            paginatedResponse.TotalPages = (int)Math.Ceiling((double)totalLogs / pageSize);
            paginatedResponse.Items = logEntries;

            return paginatedResponse;
        });
    }

    public Task<PaginatedResponse<ChangeLogEntry>> Get(List<Guid> resourceIds, int page, int pageSize)
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var paginatedResponse = new PaginatedResponse<ChangeLogEntry>
            {
                Page = page
            };

            if (pageSize <= 0)
            {
                paginatedResponse.TotalPages = 0;
                paginatedResponse.Items = [];
                return paginatedResponse;
            }

            var logQuery = context.ChangeLogEntries
                .AsNoTracking()
                .Where(c => c.ResourceId.HasValue && resourceIds.Contains(c.ResourceId.Value))
                .OrderByDescending(c => c.Date);

            var totalLogs = await logQuery.CountAsync();

            var logEntries = await logQuery
                .Skip(page * pageSize)
                .Take(pageSize)
                .Select(c => (ChangeLogEntry)c)
                .ToListAsync();

            paginatedResponse.TotalPages = (int)Math.Ceiling((double)totalLogs / pageSize);
            paginatedResponse.Items = logEntries;

            return paginatedResponse;
        });
    }

    public Task Create(ChangeLogEntry changeLog)
    {
        return Task.Run(() =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var entity = new ChangeLogEntity
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow,
                Message = changeLog.Message,
                ResourceAction = changeLog.ResourceAction,
                ResourceId = changeLog.ResourceId,
                ResourceType = changeLog.ResourceType
            };

            context.ChangeLogEntries.Add(entity);

            return context.SaveChangesAsync();
        });
    }
}