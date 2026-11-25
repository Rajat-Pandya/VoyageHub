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

        if (!string.IsNullOrWhiteSpace(role))
        {
            var r = role.Trim().ToLower();
            if (r == "manager")
            {
                stepsQuery = stepsQuery.Where(s => s.Level == "Manager");
            }
            else if (r == "finance")
            {
                stepsQuery = stepsQuery.Where(s => s.Level == "Finance");
            }
        }

        return await stepsQuery.OrderByDescending(s => s.Request!.CreatedAt).ToListAsync(cancellationToken);
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
