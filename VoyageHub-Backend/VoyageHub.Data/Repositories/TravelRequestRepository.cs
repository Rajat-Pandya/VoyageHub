using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VoyageHub.Data.Entities;

namespace VoyageHub.Data.Repositories;

public class TravelRequestRepository : Repository<TravelRequest>, ITravelRequestRepository
{
    public TravelRequestRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<TravelRequest>> GetRequestsAsync(
        string? status,
        string? search,
        Guid? employeeId,
        int? limit,
        Guid? managerId = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(r => r.Employee)
                .ThenInclude(e => e!.Department)
            .Include(r => r.ApprovalSteps)
                .ThenInclude(s => s.Approver)
            .AsNoTracking();

        if (employeeId.HasValue && employeeId != Guid.Empty)
        {
            query = query.Where(r => r.EmployeeId == employeeId.Value);
        }
        else if (managerId.HasValue && managerId != Guid.Empty)
        {
            query = query.Where(r => r.EmployeeId == managerId.Value || (r.Employee != null && r.Employee.ManagerId == managerId.Value));
        }

        if (!string.IsNullOrWhiteSpace(status) && status.ToLower() != "all" && status.ToLower() != "all statuses")
        {
            var s = status.Trim().ToLower();
            if (s == "pending" || s == "manager review" || s == "in review")
            {
                query = query.Where(r => r.Status == "In Review" || r.Status == "Manager Review" || r.Status == "Pending");
            }
            else
            {
                query = query.Where(r => r.Status.ToLower().Contains(s));
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(r => r.Destination.ToLower().Contains(term) ||
                                     r.Purpose.ToLower().Contains(term) ||
                                     r.RequestCode.ToLower().Contains(term));
        }

        query = query.OrderByDescending(r => r.CreatedAt);

        if (limit.HasValue && limit.Value > 0)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<(int Drafts, int Pending, int Upcoming, decimal Reimbursement)> GetStatsAsync(
        Guid? employeeId,
        Guid? managerId = null,
        CancellationToken cancellationToken = default)
    {
        var reqQuery = DbSet.AsQueryable();
        if (employeeId.HasValue && employeeId != Guid.Empty)
        {
            reqQuery = reqQuery.Where(r => r.EmployeeId == employeeId.Value);
        }
        else if (managerId.HasValue && managerId != Guid.Empty)
        {
            reqQuery = reqQuery.Where(r => r.EmployeeId == managerId.Value || (r.Employee != null && r.Employee.ManagerId == managerId.Value));
        }

        var drafts = await reqQuery.CountAsync(r => r.Status == "Draft", cancellationToken);
        var pending = await reqQuery.CountAsync(r => r.Status == "In Review" || r.Status == "Manager Review" || r.Status == "Pending", cancellationToken);
        var upcoming = await reqQuery.CountAsync(r => r.Status == "Approved" && r.StartDate >= DateTime.UtcNow.Date, cancellationToken);

        var expenseQuery = Context.Expenses.AsQueryable();
        if (employeeId.HasValue && employeeId != Guid.Empty)
        {
            expenseQuery = expenseQuery.Where(e => e.Request != null && e.Request.EmployeeId == employeeId.Value);
        }
        else if (managerId.HasValue && managerId != Guid.Empty)
        {
            expenseQuery = expenseQuery.Where(e => e.Request != null && (e.Request.EmployeeId == managerId.Value || (e.Request.Employee != null && e.Request.Employee.ManagerId == managerId.Value)));
        }
        var reimbursement = await expenseQuery.Where(e => e.Status == "Pending").SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0m;

        return (drafts, pending, upcoming, reimbursement);
    }

    public async Task<TravelRequest?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Employee)
                .ThenInclude(e => e!.Department)
                    .ThenInclude(d => d!.Policy)
            .Include(r => r.ItinerarySegments)
            .Include(r => r.Stays)
            .Include(r => r.Expenses)
            .Include(r => r.ApprovalSteps)
                .ThenInclude(s => s.Approver)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<User?> GetEmployeeWithDepartmentAndManagerAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .Include(u => u.Department)
                .ThenInclude(d => d!.Policy)
            .Include(u => u.Manager)
            .FirstOrDefaultAsync(u => u.Id == employeeId, cancellationToken);
    }

    public async Task<Guid> GetFirstEmployeeIdAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Users.Where(u => u.Role == "Employee").Select(u => u.Id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Guid?> GetFirstManagerIdAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Users.Where(u => u.Role == "Manager").Select(u => (Guid?)u.Id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Guid> GetFirstFinanceIdAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Users.Where(u => u.Role == "Finance").Select(u => u.Id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(cancellationToken);
    }

    public async Task<TravelRequest?> GetDraftForSubmissionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Employee)
                .ThenInclude(e => e!.Department)
                    .ThenInclude(d => d!.Policy)
            .Include(r => r.ApprovalSteps)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public void RemoveApprovalSteps(IEnumerable<ApprovalStep> steps)
    {
        Context.ApprovalSteps.RemoveRange(steps);
    }

    public async Task AddApprovalStepAsync(ApprovalStep step, CancellationToken cancellationToken = default)
    {
        await Context.ApprovalSteps.AddAsync(step, cancellationToken);
    }

    public async Task AddSegmentAsync(ItinerarySegment segment, CancellationToken cancellationToken = default)
    {
        await Context.ItinerarySegments.AddAsync(segment, cancellationToken);
    }

    public async Task AddStayAsync(Stay stay, CancellationToken cancellationToken = default)
    {
        await Context.Stays.AddAsync(stay, cancellationToken);
    }
}

