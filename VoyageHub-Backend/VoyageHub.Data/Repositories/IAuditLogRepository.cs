using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Data.Entities;

namespace VoyageHub.Data.Repositories;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<List<AuditLog>> GetLogsAsync(string? severity, string? entityType, int limit = 50, CancellationToken cancellationToken = default);
}
