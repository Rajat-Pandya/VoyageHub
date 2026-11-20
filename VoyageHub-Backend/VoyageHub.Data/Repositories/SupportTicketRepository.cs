using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VoyageHub.Data.Entities;

namespace VoyageHub.Data.Repositories;

public class SupportTicketRepository : Repository<SupportTicket>, ISupportTicketRepository
{
    public SupportTicketRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<SupportTicket>> GetTicketsAsync(string? status, Guid? userId, string? search, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(t => t.User)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var s = status.ToLower();
            query = query.Where(t => t.Status.ToLower() == s);
        }

        if (userId.HasValue && userId.Value != Guid.Empty)
        {
            query = query.Where(t => t.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(t => t.TicketCode.ToLower().Contains(term) ||
                                     t.Subject.ToLower().Contains(term) ||
                                     t.Category.ToLower().Contains(term) ||
                                     (t.RequestCode != null && t.RequestCode.ToLower().Contains(term)));
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<SupportTicket?> GetByIdWithUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<TravelRequest?> FindTravelRequestByCodeAsync(string requestCode, CancellationToken cancellationToken = default)
    {
        var trimmed = requestCode.Trim();
        return await Context.TravelRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RequestCode == trimmed, cancellationToken);
    }

    public async Task<User?> FindUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.Users.FindAsync(new object[] { userId }, cancellationToken);
    }
}
