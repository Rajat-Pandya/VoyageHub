using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Services;

namespace VoyageHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TravelRequestsController : ControllerBase
{
    private readonly ITravelRequestService _travelRequestService;
    private readonly ILogger<TravelRequestsController> _logger;

    public TravelRequestsController(ITravelRequestService travelRequestService, ILogger<TravelRequestsController> logger)
    {
        _travelRequestService = travelRequestService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<TravelRequestListDto>>> GetRequests(
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] Guid? employeeId,
        [FromQuery] int? limit)
    {
        var dtoList = await _travelRequestService.GetRequestsAsync(status, search, employeeId, limit);
        return Ok(dtoList);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats([FromQuery] Guid? employeeId)
    {
        var stats = await _travelRequestService.GetStatsAsync(employeeId);
        return Ok(stats);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TravelRequestDetailDto>> GetDetail(Guid id)
    {
        var detail = await _travelRequestService.GetDetailAsync(id);
        if (detail == null) return NotFound(new { message = "Travel request not found." });
        return Ok(detail);
    }

    [HttpPost]
    public async Task<ActionResult<TravelRequestDetailDto>> Create([FromBody] CreateTravelRequestDto dto, [FromQuery] Guid? employeeId)
    {
        if (dto == null) return BadRequest("Invalid request data.");

        var result = await _travelRequestService.CreateRequestAsync(dto, employeeId);
        if (!result.Success) return BadRequest(result.Message);

        return CreatedAtAction(nameof(GetDetail), new { id = result.Request!.Id }, result.Request);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelRequest(Guid id)
    {
        var result = await _travelRequestService.CancelRequestAsync(id);
        if (!result.Success) return NotFound();
        return Ok(new { message = result.Message });
    }

    [HttpPost("{id}/submit")]
    public async Task<IActionResult> SubmitDraft(Guid id)
    {
        var result = await _travelRequestService.SubmitDraftAsync(id);
        return StatusCode(result.StatusCode, result.Response);
    }

    [HttpPost("{id}/segments")]
    public async Task<IActionResult> AddSegment(Guid id, [FromBody] AddSegmentDto dto)
    {
        var result = await _travelRequestService.AddSegmentAsync(id, dto);
        if (!result.Success) return NotFound(result.Message);
        return Ok(result.Segment);
    }

    [HttpPost("{id}/stays")]
    public async Task<IActionResult> AddStay(Guid id, [FromBody] AddStayDto dto)
    {
        var result = await _travelRequestService.AddStayAsync(id, dto);
        if (!result.Success) return NotFound(result.Message);
        return Ok(result.Stay);
    }

    [HttpPost("{id}/close")]
    public async Task<IActionResult> CloseRequest(Guid id)
    {
        var result = await _travelRequestService.CloseRequestAsync(id);
        if (!result.Success) return NotFound(result.Message);
        return Ok(new { success = true, message = result.Message });
    }
}
