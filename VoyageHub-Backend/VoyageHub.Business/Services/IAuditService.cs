using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Business.Dtos;

namespace VoyageHub.Business.Services;

public interface IAuditService
{
    Task<List<AuditLogDto>> GetLogsAsync(string? severity, string? entityType, int limit = 50, CancellationToken cancellationToken = default);
    Task LogAsync(string entityType, Guid? entityId, string action, Guid? actorId, string actorName, string changeDetail, string severity = "LOW", CancellationToken cancellationToken = default);
}
