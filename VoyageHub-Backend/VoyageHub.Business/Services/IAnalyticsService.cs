using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Business.Dtos;

namespace VoyageHub.Business.Services;

public interface IAnalyticsService
{
    Task<AnalyticsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<List<EmployeeSpendReportDto>> GetEmployeeSpendReportAsync(CancellationToken cancellationToken = default);
    Task<CostSummaryReportDto> GetCostSummaryReportAsync(CancellationToken cancellationToken = default);
}
