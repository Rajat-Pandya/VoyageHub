using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Services;

namespace VoyageHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Manager,Finance,Admin")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<AnalyticsOverviewDto>> GetOverview(CancellationToken cancellationToken)
    {
        var overview = await _analyticsService.GetOverviewAsync(cancellationToken);
        return Ok(overview);
    }

    [HttpGet("employee-reports")]
    public async Task<ActionResult<List<EmployeeSpendReportDto>>> GetEmployeeReports(CancellationToken cancellationToken)
    {
        var reports = await _analyticsService.GetEmployeeSpendReportAsync(cancellationToken);
        return Ok(reports);
    }

    [HttpGet("cost-reports")]
    public async Task<ActionResult<CostSummaryReportDto>> GetCostReports(CancellationToken cancellationToken)
    {
        var report = await _analyticsService.GetCostSummaryReportAsync(cancellationToken);
        return Ok(report);
    }
}
