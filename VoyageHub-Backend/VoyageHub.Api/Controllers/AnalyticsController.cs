using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Services;

namespace VoyageHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
}
