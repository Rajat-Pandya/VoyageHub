using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Services;

namespace VoyageHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TravelRequestsController : ControllerBase
{
    private readonly ITravelRequestService _travelRequestService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<TravelRequestsController> _logger;

    public TravelRequestsController(
        ITravelRequestService travelRequestService,
        ICurrentUserService currentUserService,
        ILogger<TravelRequestsController> logger)
    {
        _travelRequestService = travelRequestService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Employee,Manager,Finance,Admin")]
    public async Task<ActionResult<List<TravelRequestListDto>>> GetRequests(
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] Guid? employeeId,
        [FromQuery] int? limit)
    {
        if (User.IsInRole("Employee"))
        {
            var callerId = _currentUserService.UserId;
            if (employeeId.HasValue && callerId.HasValue && employeeId.Value != callerId.Value)
            {
                return Forbid();
            }
        }

        var dtoList = await _travelRequestService.GetRequestsAsync(status, search, employeeId, limit);
        return Ok(dtoList);
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Employee,Manager,Finance,Admin")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats([FromQuery] Guid? employeeId)
    {
        if (User.IsInRole("Employee"))
        {
            var callerId = _currentUserService.UserId;
            if (employeeId.HasValue && callerId.HasValue && employeeId.Value != callerId.Value)
            {
                return Forbid();
            }
        }

        var stats = await _travelRequestService.GetStatsAsync(employeeId);
        return Ok(stats);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Employee,Manager,Finance,Admin")]
    public async Task<ActionResult<TravelRequestDetailDto>> GetDetail(Guid id)
    {
        var detail = await _travelRequestService.GetDetailAsync(id);
        if (detail == null) return NotFound(new { message = "Travel request not found." });

        if (User.IsInRole("Employee"))
        {
            var callerId = _currentUserService.UserId;
            if (callerId.HasValue && detail.EmployeeId != callerId.Value)
            {
                return Forbid();
            }
        }

        return Ok(detail);
    }

    [HttpPost]
    [Authorize(Roles = "Employee,Manager")]
    public async Task<ActionResult<TravelRequestDetailDto>> Create([FromBody] CreateTravelRequestDto dto, [FromQuery] Guid? employeeId)
    {
        if (dto == null) return BadRequest(new { message = "Invalid request data." });

        if (User.IsInRole("Employee"))
        {
            employeeId = _currentUserService.UserId;
        }

        var result = await _travelRequestService.CreateRequestAsync(dto, employeeId);
        if (!result.Success) return BadRequest(new { message = result.Message });

        return CreatedAtAction(nameof(GetDetail), new { id = result.Request!.Id }, result.Request);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Employee,Manager,Admin")]
    public async Task<IActionResult> CancelRequest(Guid id)
    {
        var result = await _travelRequestService.CancelRequestAsync(id);
        if (!result.Success) return NotFound(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    [HttpPost("{id}/submit")]
    [Authorize(Roles = "Employee,Manager,Admin")]
    public async Task<IActionResult> SubmitDraft(Guid id)
    {
        var result = await _travelRequestService.SubmitDraftAsync(id);
        return StatusCode(result.StatusCode, result.Response);
    }

    [HttpPost("{id}/segments")]
    [Authorize(Roles = "Employee,Manager,Admin")]
    public async Task<IActionResult> AddSegment(Guid id, [FromBody] AddSegmentDto dto)
    {
        var result = await _travelRequestService.AddSegmentAsync(id, dto);
        if (!result.Success) return NotFound(new { message = result.Message });
        return Ok(result.Segment);
    }

    [HttpPost("{id}/stays")]
    [Authorize(Roles = "Employee,Manager,Admin")]
    public async Task<IActionResult> AddStay(Guid id, [FromBody] AddStayDto dto)
    {
        var result = await _travelRequestService.AddStayAsync(id, dto);
        if (!result.Success) return NotFound(new { message = result.Message });
        return Ok(result.Stay);
    }

    [HttpPost("{id}/close")]
    [Authorize(Roles = "Employee,Manager,Finance,Admin")]
    public async Task<IActionResult> CloseRequest(Guid id)
    {
        var result = await _travelRequestService.CloseRequestAsync(id);
        if (!result.Success)
        {
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { message = result.Message });
            }
            return BadRequest(new { message = result.Message });
        }
        return Ok(new { success = true, message = result.Message });
    }
}
