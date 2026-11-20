using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Services;

namespace VoyageHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupportController : ControllerBase
{
    private readonly ISupportService _supportService;
    private readonly ILogger<SupportController> _logger;

    public SupportController(ISupportService supportService, ILogger<SupportController> logger)
    {
        _supportService = supportService;
        _logger = logger;
    }

    [HttpGet("tickets")]
    public async Task<ActionResult<List<SupportTicketDto>>> GetTickets(
        [FromQuery] string? status,
        [FromQuery] Guid? userId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        try
        {
            var tickets = await _supportService.GetTicketsAsync(status, userId, search, cancellationToken);
            return Ok(tickets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching support tickets");
            return StatusCode(500, new { message = $"Database error: {ex.Message}" });
        }
    }

    [HttpGet("tickets/{id}")]
    public async Task<ActionResult<SupportTicketDto>> GetTicketById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _supportService.GetTicketByIdAsync(id, cancellationToken);
            if (ticket == null)
            {
                return NotFound(new { message = $"Ticket with ID {id} not found." });
            }

            return Ok(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching ticket {TicketId}", id);
            return StatusCode(500, new { message = $"Database error: {ex.Message}" });
        }
    }

    [HttpPost("tickets")]
    public async Task<ActionResult<SupportTicketDto>> CreateTicket([FromBody] CreateSupportTicketDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var (success, message, ticket) = await _supportService.CreateTicketAsync(dto, cancellationToken);
            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating support ticket");
            return StatusCode(500, new { message = $"Database error: {ex.Message}" });
        }
    }

    [HttpPost("tickets/{id}/status")]
    public async Task<ActionResult> UpdateTicketStatus(Guid id, [FromBody] UpdateTicketStatusDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var (success, message) = await _supportService.UpdateTicketStatusAsync(id, dto, cancellationToken);
            if (!success)
            {
                return NotFound(new { message });
            }

            return Ok(new { success = true, message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ticket {TicketId}", id);
            return StatusCode(500, new { message = $"Database error: {ex.Message}" });
        }
    }
}
