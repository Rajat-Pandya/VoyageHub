using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Business.Dtos;

namespace VoyageHub.Business.Services;

public interface ISupportService
{
    Task<List<SupportTicketDto>> GetTicketsAsync(string? status, Guid? userId, string? search, CancellationToken cancellationToken = default);
    Task<SupportTicketDto?> GetTicketByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message, SupportTicketDto? Ticket)> CreateTicketAsync(CreateSupportTicketDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> UpdateTicketStatusAsync(Guid id, UpdateTicketStatusDto dto, CancellationToken cancellationToken = default);
}
