using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Business.Dtos;
using VoyageHub.Data.Entities;
using VoyageHub.Data.Repositories;

namespace VoyageHub.Business.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<List<AuditLogDto>> GetLogsAsync(string? severity, string? entityType, int limit = 50, CancellationToken cancellationToken = default)
    {
        var logs = await _auditLogRepository.GetLogsAsync(severity, entityType, limit, cancellationToken);

        return logs.Select(a => new AuditLogDto
        {
            Id = a.Id,
            Timestamp = a.Timestamp,
            EntityType = a.EntityType,
            Action = a.Action,
            Actor = a.ActorName,
            ActorAvatar = a.Actor != null ? a.Actor.AvatarUrl : null,
            ChangeDetail = a.ChangeDetail,
            Severity = a.Severity
        }).ToList();
    }

    public async Task LogAsync(
        string entityType,
        Guid? entityId,
        string action,
        Guid? actorId,
        string actorName,
        string changeDetail,
        string severity = "LOW",
        CancellationToken cancellationToken = default)
    {
        var log = new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            ActorId = actorId,
            ActorName = actorName,
            ChangeDetail = changeDetail,
            Severity = severity,
            Timestamp = DateTime.UtcNow
        };

        await _auditLogRepository.AddAsync(log, cancellationToken);
    }
}
