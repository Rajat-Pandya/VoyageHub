using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Data.Entities;

namespace VoyageHub.Data.Repositories;

public interface ISupportTicketRepository : IRepository<SupportTicket>
{
    Task<List<SupportTicket>> GetTicketsAsync(string? status, Guid? userId, string? search, CancellationToken cancellationToken = default);
    Task<SupportTicket?> GetByIdWithUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TravelRequest?> FindTravelRequestByCodeAsync(string requestCode, CancellationToken cancellationToken = default);
    Task<User?> FindUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
