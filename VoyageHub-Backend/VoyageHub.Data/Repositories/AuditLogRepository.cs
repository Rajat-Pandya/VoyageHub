using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VoyageHub.Data.Entities;

namespace VoyageHub.Data.Repositories;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<AuditLog>> GetLogsAsync(string? severity, string? entityType, int limit = 50, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(a => a.Actor)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(severity) && severity.ToUpper() != "ALL")
        {
            var s = severity.Trim().ToUpper();
            query = query.Where(a => a.Severity.ToUpper() == s);
        }

        if (!string.IsNullOrWhiteSpace(entityType) && entityType.ToUpper() != "ALL")
        {
            var e = entityType.Trim().ToUpper();
            query = query.Where(a => a.EntityType.ToUpper() == e);
        }

        return await query.OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
