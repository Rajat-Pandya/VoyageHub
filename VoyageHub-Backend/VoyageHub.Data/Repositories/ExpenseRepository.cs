using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VoyageHub.Data.Entities;

namespace VoyageHub.Data.Repositories;

public class ExpenseRepository : Repository<Expense>, IExpenseRepository
{
    public ExpenseRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Expense>> GetPendingExpensesAsync(string? category, bool? highPriorityOnly, CancellationToken cancellationToken = default)
    {
        return await GetExpensesByFilterAsync("Pending", category, highPriorityOnly, cancellationToken);
    }

    public async Task<List<Expense>> GetExpensesByFilterAsync(string? status, string? category, bool? highPriorityOnly, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(e => e.Request)
                .ThenInclude(r => r!.Employee)
                    .ThenInclude(u => u!.Department)
            .Include(e => e.Request)
                .ThenInclude(r => r!.ItinerarySegments)
            .Include(e => e.Request)
                .ThenInclude(r => r!.Stays)
            .Include(e => e.Request)
                .ThenInclude(r => r!.Expenses)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
        {
            var normalizedStatus = status.Trim();
            query = query.Where(e => e.Status == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(category) && category.ToLower() != "all categories" && category.ToLower() != "all")
        {
            var cat = category.Trim().ToLower();
            query = query.Where(e => e.Category.ToLower() == cat);
        }

        if (highPriorityOnly == true)
        {
            query = query.Where(e => e.Amount >= 10000.00m);
        }

        return await query.OrderByDescending(e => e.SubmittedAt).ThenByDescending(e => e.Amount).ToListAsync(cancellationToken);
    }

    public async Task<List<Expense>> GetExpensesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTrackingWithIdentityResolution()
            .Include(e => e.Request)
                .ThenInclude(r => r!.Employee)
                    .ThenInclude(u => u!.Department)
            .Include(e => e.Request)
                .ThenInclude(r => r!.ItinerarySegments)
            .Include(e => e.Request)
                .ThenInclude(r => r!.Stays)
            .Include(e => e.Request)
                .ThenInclude(r => r!.Expenses)
            .Where(e => e.Request != null && e.Request.EmployeeId == userId)
            .OrderByDescending(e => e.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TravelRequest>> GetUserTripsForReimbursementAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.TravelRequests
            .AsNoTracking()
            .Include(r => r.ItinerarySegments)
            .Include(r => r.Stays)
            .Include(r => r.Expenses)
            .Where(r => r.EmployeeId == userId &&
                        (r.Status == "Approved" || r.Status == "Completed" || r.Expenses.Count > 0))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalPendingAmountAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(e => e.Status == "Pending").SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0m;
    }

    public async Task<TravelRequest?> FindTravelRequestWithEmployeeAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        return await Context.TravelRequests.Include(r => r.Employee).FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);
    }

    public async Task<Expense?> GetByIdWithRequestAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Request)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<List<Expense>> GetPendingExpensesByIdsAsync(List<Guid>? expenseIds, CancellationToken cancellationToken = default)
    {
        if (expenseIds != null && expenseIds.Count > 0)
        {
            return await DbSet.Where(e => expenseIds.Contains(e.Id) && e.Status == "Pending").ToListAsync(cancellationToken);
        }

        return await DbSet.Where(e => e.Status == "Pending").ToListAsync(cancellationToken);
    }
}
