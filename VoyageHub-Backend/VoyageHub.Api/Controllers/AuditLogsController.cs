using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Services;

namespace VoyageHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditLogsController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuditLogDto>>> GetLogs(
        [FromQuery] string? severity,
        [FromQuery] string? entityType,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var logs = await _auditService.GetLogsAsync(severity, entityType, limit, cancellationToken);
        return Ok(logs);
    }
}
