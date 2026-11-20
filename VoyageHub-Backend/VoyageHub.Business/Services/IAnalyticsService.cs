using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Business.Dtos;

namespace VoyageHub.Business.Services;

public interface IAnalyticsService
{
    Task<AnalyticsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);
}
