using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Services;

namespace VoyageHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApprovalsController : ControllerBase
{
    private readonly IApprovalService _approvalService;

    public ApprovalsController(IApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    [HttpGet("queue")]
    public async Task<ActionResult<ApprovalQueueSummaryDto>> GetQueue(
        [FromQuery] Guid? approverId,
        [FromQuery] string? role,
        CancellationToken cancellationToken)
    {
        var summary = await _approvalService.GetQueueAsync(approverId, role, cancellationToken);
        return Ok(summary);
    }

    [HttpPost("{stepId}/action")]
    public async Task<IActionResult> TakeAction(Guid stepId, [FromBody] ApprovalActionDto dto, CancellationToken cancellationToken)
    {
        var (success, message) = await _approvalService.TakeActionAsync(stepId, dto, cancellationToken);
        if (!success)
        {
            return NotFound(new { message });
        }

        return Ok(new { success = true, message });
    }
}
