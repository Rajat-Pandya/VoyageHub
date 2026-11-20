using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Business.Dtos;
using VoyageHub.Data.Entities;
using VoyageHub.Data.Repositories;

namespace VoyageHub.Business.Services;

public class SupportService : ISupportService
{
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly IAuditService _auditService;

    public SupportService(ISupportTicketRepository supportTicketRepository, IAuditService auditService)
    {
        _supportTicketRepository = supportTicketRepository;
        _auditService = auditService;
    }

    public async Task<List<SupportTicketDto>> GetTicketsAsync(string? status, Guid? userId, string? search, CancellationToken cancellationToken = default)
    {
        var tickets = await _supportTicketRepository.GetTicketsAsync(status, userId, search, cancellationToken);

        return tickets.Select(t => new SupportTicketDto
        {
            Id = t.Id,
            TicketCode = t.TicketCode,
            Category = t.Category,
            Priority = t.Priority,
            RequestCode = t.RequestCode,
            RequestId = t.RequestId,
            Subject = t.Subject,
            Description = t.Description,
            CallbackNumber = t.CallbackNumber,
            Status = t.Status,
            AssignedAgent = t.AssignedAgent,
            ResolutionNotes = t.ResolutionNotes,
            UserId = t.UserId,
            UserName = t.User != null ? t.User.Name : null,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        }).ToList();
    }

    public async Task<SupportTicketDto?> GetTicketByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var t = await _supportTicketRepository.GetByIdWithUserAsync(id, cancellationToken);
        if (t == null) return null;

        return new SupportTicketDto
        {
            Id = t.Id,
            TicketCode = t.TicketCode,
            Category = t.Category,
            Priority = t.Priority,
            RequestCode = t.RequestCode,
            RequestId = t.RequestId,
            Subject = t.Subject,
            Description = t.Description,
            CallbackNumber = t.CallbackNumber,
            Status = t.Status,
            AssignedAgent = t.AssignedAgent,
            ResolutionNotes = t.ResolutionNotes,
            UserId = t.UserId,
            UserName = t.User?.Name,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        };
    }

    public async Task<(bool Success, string Message, SupportTicketDto? Ticket)> CreateTicketAsync(CreateSupportTicketDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Subject) || string.IsNullOrWhiteSpace(dto.Description))
        {
            return (false, "Subject and description are required.", null);
        }

        // Generate unique ticket code
        var randomNum = Random.Shared.Next(1000, 9999);
        var ticketCode = $"#TICK-{randomNum}";

        // Find matching travel request if provided
        Guid? matchedRequestId = null;
        if (!string.IsNullOrWhiteSpace(dto.RequestCode) && dto.RequestCode != "None")
        {
            var tr = await _supportTicketRepository.FindTravelRequestByCodeAsync(dto.RequestCode, cancellationToken);
            if (tr != null)
            {
                matchedRequestId = tr.Id;
            }
        }

        // Find user if provided
        User? user = null;
        if (dto.UserId.HasValue && dto.UserId.Value != Guid.Empty)
        {
            user = await _supportTicketRepository.FindUserByIdAsync(dto.UserId.Value, cancellationToken);
        }

        var ticket = new SupportTicket
        {
            Id = Guid.NewGuid(),
            TicketCode = ticketCode,
            Category = dto.Category,
            Priority = dto.Priority,
            RequestCode = dto.RequestCode != "None" ? dto.RequestCode : null,
            RequestId = matchedRequestId,
            Subject = dto.Subject.Trim(),
            Description = dto.Description.Trim(),
            CallbackNumber = dto.CallbackNumber?.Trim(),
            Status = "Open",
            AssignedAgent = "Meera Sen (Corporate Travel Desk)",
            UserId = user?.Id ?? dto.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _supportTicketRepository.AddAsync(ticket, cancellationToken);

        // Audit log
        var severity = dto.Priority.Equals("Urgent", StringComparison.OrdinalIgnoreCase) ? "HIGH" :
                       (dto.Priority.Equals("High", StringComparison.OrdinalIgnoreCase) ? "MEDIUM" : "LOW");

        await _auditService.LogAsync(
            entityType: "SUPPORT",
            entityId: ticket.Id,
            action: "Create Support Ticket",
            actorId: user?.Id,
            actorName: user?.Name ?? "Employee",
            changeDetail: $"Opened support ticket {ticketCode}: '{ticket.Subject}' ({ticket.Category}, Priority: {ticket.Priority})",
            severity: severity,
            cancellationToken: cancellationToken);

        await _supportTicketRepository.SaveChangesAsync(cancellationToken);

        var resultDto = new SupportTicketDto
        {
            Id = ticket.Id,
            TicketCode = ticket.TicketCode,
            Category = ticket.Category,
            Priority = ticket.Priority,
            RequestCode = ticket.RequestCode,
            RequestId = ticket.RequestId,
            Subject = ticket.Subject,
            Description = ticket.Description,
            CallbackNumber = ticket.CallbackNumber,
            Status = ticket.Status,
            AssignedAgent = ticket.AssignedAgent,
            ResolutionNotes = ticket.ResolutionNotes,
            UserId = ticket.UserId,
            UserName = user?.Name,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt
        };

        return (true, string.Empty, resultDto);
    }

    public async Task<(bool Success, string Message)> UpdateTicketStatusAsync(Guid id, UpdateTicketStatusDto dto, CancellationToken cancellationToken = default)
    {
        var ticket = await _supportTicketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket == null)
        {
            return (false, $"Ticket with ID {id} not found.");
        }

        ticket.Status = dto.Status;
        ticket.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.ResolutionNotes))
        {
            ticket.ResolutionNotes = dto.ResolutionNotes;
        }

        if (!string.IsNullOrWhiteSpace(dto.AssignedAgent))
        {
            ticket.AssignedAgent = dto.AssignedAgent;
        }

        await _auditService.LogAsync(
            entityType: "SUPPORT",
            entityId: ticket.Id,
            action: "Update Support Ticket Status",
            actorId: null,
            actorName: ticket.AssignedAgent ?? "Travel Desk Agent",
            changeDetail: $"Support ticket {ticket.TicketCode} status changed to '{ticket.Status}'",
            severity: "LOW",
            cancellationToken: cancellationToken);

        await _supportTicketRepository.SaveChangesAsync(cancellationToken);

        return (true, $"Ticket {ticket.TicketCode} status updated to {ticket.Status}.");
    }
}
