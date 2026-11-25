using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VoyageHub.Data.Entities;

namespace VoyageHub.Data.Repositories;

public class ApprovalStepRepository : Repository<ApprovalStep>, IApprovalStepRepository
{
    public ApprovalStepRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<ApprovalStep>> GetPendingStepsAsync(Guid? approverId, string? role, CancellationToken cancellationToken = default)
    {
        var stepsQuery = DbSet
            .Include(s => s.Request)
                .ThenInclude(r => r!.Employee)
                    .ThenInclude(e => e!.Department)
                        .ThenInclude(d => d!.Policy)
            .Where(s => s.Status == "Pending" && s.Request != null);

        stepsQuery = ApplyRoleFilter(stepsQuery, role);

        return await stepsQuery.OrderByDescending(s => s.Request!.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<double> GetAverageResponseTimeHoursAsync(Guid? approverId, string? role, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(s => s.DecidedAt != null && s.Request != null && (s.Status == "Approved" || s.Status == "Rejected"));

        query = ApplyRoleFilter(query, role);

        if (approverId.HasValue && approverId.Value != Guid.Empty)
        {
            query = query.Where(s => s.ApproverId == approverId.Value);
        }

        var durations = await query
            .Select(s => new { DecidedAt = s.DecidedAt!.Value, CreatedAt = s.Request!.CreatedAt })
            .ToListAsync(cancellationToken);

        if (durations.Count == 0)
        {
            return 0.0;
        }

        var avgHours = durations
            .Select(d => Math.Max(0.0, (d.DecidedAt - d.CreatedAt).TotalHours))
            .Average();

        return Math.Round(avgHours, 1);
    }

    private static IQueryable<ApprovalStep> ApplyRoleFilter(IQueryable<ApprovalStep> query, string? role)
    {
        if (string.IsNullOrWhiteSpace(role)) return query;

        var r = role.Trim().ToLower();
        if (r == "manager") return query.Where(s => s.Level == "Manager");
        if (r == "finance") return query.Where(s => s.Level == "Finance");
        return query;
    }

    public async Task<ApprovalStep?> GetByIdWithRequestAndApproverAsync(Guid stepId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Request)
            .Include(s => s.Approver)
            .FirstOrDefaultAsync(s => s.Id == stepId, cancellationToken);
    }

    public async Task<ApprovalStep?> FindFinanceStepAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(s => s.RequestId == requestId && s.Level == "Finance" && s.Status != "Approved", cancellationToken);
    }

    public async Task<List<ApprovalStep>> GetRemainingNonApprovedStepsAsync(Guid requestId, Guid currentStepId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.RequestId == requestId && s.Id != currentStepId && s.Status != "Approved")
            .ToListAsync(cancellationToken);
    }
}
