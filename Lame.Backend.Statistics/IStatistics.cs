using Lame.DomainModel;

namespace Lame.Backend.Statistics;

public interface IStatistics
{
    public Task<ProjectStatistics> GetProjectStatistics();
}