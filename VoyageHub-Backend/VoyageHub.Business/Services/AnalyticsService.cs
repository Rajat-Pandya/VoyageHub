using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VoyageHub.Business.Dtos;
using VoyageHub.Data;

namespace VoyageHub.Business.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly AppDbContext _context;

    public AnalyticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AnalyticsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        // 1. Department Spend
        var departments = await _context.Departments
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var deptSpendList = new List<DepartmentSpendDto>();

        foreach (var d in departments)
        {
            var trips = await _context.TravelRequests
                .AsNoTracking()
                .CountAsync(r => r.Employee!.DepartmentId == d.Id && r.Status != "Draft" && r.Status != "Cancelled", cancellationToken);

            var reimbursedExpenses = await _context.Expenses
                .AsNoTracking()
                .Where(e => (e.Status == "Reimbursed" || e.Status == "Approved") && e.Request!.Employee!.DepartmentId == d.Id)
                .SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0m;

            var approvedBudgets = await _context.TravelRequests
                .AsNoTracking()
                .Where(r => (r.Status == "Approved" || r.Status == "Completed") && r.Employee!.DepartmentId == d.Id)
                .SumAsync(r => (decimal?)r.EstimatedBudget, cancellationToken) ?? 0m;

            var total = reimbursedExpenses > 0 ? reimbursedExpenses : approvedBudgets;
            var avg = trips > 0 ? Math.Round(total / trips, 2) : 0m;

            deptSpendList.Add(new DepartmentSpendDto
            {
                Department = d.Name,
                Trips = trips,
                AvgCost = avg,
                Total = total
            });
        }

        // 2. Monthly Spend (last 6 months)
        var now = DateTime.UtcNow;
        var monthlySpendList = new List<MonthlySpendDto>();

        for (int i = 5; i >= 0; i--)
        {
            var monthDate = now.AddMonths(-i);
            var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);
            var monthLabel = monthStart.ToString("MMM");

            var monthExpenses = await _context.Expenses
                .AsNoTracking()
                .Where(e => (e.Status == "Reimbursed" || e.Status == "Approved") && e.SubmittedAt >= monthStart && e.SubmittedAt < monthEnd)
                .SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0m;

            var monthRequests = await _context.TravelRequests
                .AsNoTracking()
                .Where(r => (r.Status == "Approved" || r.Status == "Completed") && r.CreatedAt >= monthStart && r.CreatedAt < monthEnd)
                .SumAsync(r => (decimal?)r.EstimatedBudget, cancellationToken) ?? 0m;

            var amount = monthExpenses > 0 ? monthExpenses : monthRequests;

            monthlySpendList.Add(new MonthlySpendDto
            {
                Month = monthLabel,
                Amount = amount
            });
        }

        // 3. Category Spend
        var categoryColors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Airfare", "#4f46e5" },
            { "Flight", "#4f46e5" },
            { "Hotel", "#f59e0b" },
            { "Stay", "#f59e0b" },
            { "Meals", "#10b981" },
            { "Food", "#10b981" },
            { "Transport", "#ec4899" },
            { "Car", "#ec4899" },
            { "Train", "#8b5cf6" },
            { "Other", "#64748b" }
        };

        var expensesByCategory = await _context.Expenses
            .AsNoTracking()
            .Where(e => e.Status == "Reimbursed" || e.Status == "Approved" || e.Status == "Pending")
            .GroupBy(e => e.Category)
            .Select(g => new
            {
                Category = g.Key,
                Amount = g.Sum(e => e.Amount)
            })
            .ToListAsync(cancellationToken);

        var totalCatAmount = expensesByCategory.Sum(c => c.Amount);
        var categorySpendList = new List<CategorySpendDto>();

        foreach (var c in expensesByCategory)
        {
            var pct = totalCatAmount > 0 ? (int)Math.Round((double)c.Amount / (double)totalCatAmount * 100) : 0;
            var color = categoryColors.TryGetValue(c.Category, out var hex) ? hex : "#6366f1";

            categorySpendList.Add(new CategorySpendDto
            {
                Category = c.Category,
                Amount = c.Amount,
                Percentage = pct,
                Color = color
            });
        }

        if (categorySpendList.Count == 0)
        {
            var flightsCost = await _context.ItinerarySegments
                .AsNoTracking()
                .Where(s => s.Request!.Status == "Approved" || s.Request.Status == "Completed" || s.Request.Status == "In Review")
                .SumAsync(s => (decimal?)s.Cost, cancellationToken) ?? 0m;

            var staysCost = await _context.Stays
                .AsNoTracking()
                .Where(s => s.Request!.Status == "Approved" || s.Request.Status == "Completed" || s.Request.Status == "In Review")
                .SumAsync(s => (decimal?)s.Cost, cancellationToken) ?? 0m;

            var combined = flightsCost + staysCost;
            if (combined > 0)
            {
                categorySpendList.Add(new CategorySpendDto
                {
                    Category = "Airfare",
                    Amount = flightsCost,
                    Percentage = (int)Math.Round((double)flightsCost / (double)combined * 100),
                    Color = "#4f46e5"
                });
                categorySpendList.Add(new CategorySpendDto
                {
                    Category = "Hotel",
                    Amount = staysCost,
                    Percentage = (int)Math.Round((double)staysCost / (double)combined * 100),
                    Color = "#f59e0b"
                });
            }
        }

        // 4. Total Spent
        var totalSpent = deptSpendList.Sum(d => d.Total);

        // 5. Vs Last Period Percentage
        double vsLastPeriodPercent = 0.0;
        if (monthlySpendList.Count >= 2)
        {
            var currentMonthAmount = monthlySpendList[^1].Amount;
            var prevMonthAmount = monthlySpendList[^2].Amount;
            if (prevMonthAmount > 0)
            {
                vsLastPeriodPercent = Math.Round((double)((currentMonthAmount - prevMonthAmount) / prevMonthAmount) * 100, 1);
            }
        }

        return new AnalyticsOverviewDto
        {
            TotalSpent = totalSpent,
            VsLastPeriodPercent = vsLastPeriodPercent,
            MonthlySpend = monthlySpendList,
            DepartmentSpend = deptSpendList,
            CategorySpend = categorySpendList
        };
    }

    public async Task<List<EmployeeSpendReportDto>> GetEmployeeSpendReportAsync(CancellationToken cancellationToken = default)
    {
        var users = await _context.Users
            .AsNoTracking()
            .Include(u => u.Department)
            .ToListAsync(cancellationToken);

        var reports = new List<EmployeeSpendReportDto>();

        foreach (var u in users)
        {
            var requests = await _context.TravelRequests
                .AsNoTracking()
                .Where(r => r.EmployeeId == u.Id && r.Status != "Cancelled")
                .ToListAsync(cancellationToken);

            if (requests.Count == 0) continue;

            var trips = requests.Count(r => r.Status != "Draft");
            var approvedBudget = requests
                .Where(r => r.Status == "Approved" || r.Status == "Completed")
                .Sum(r => r.EstimatedBudget);

            var reqIds = requests.Select(r => r.Id).ToList();
            var reimbursedExpenses = await _context.Expenses
                .AsNoTracking()
                .Where(e => reqIds.Contains(e.RequestId) && (e.Status == "Reimbursed" || e.Status == "Approved"))
                .SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0m;

            reports.Add(new EmployeeSpendReportDto
            {
                EmployeeId = u.Id,
                EmployeeName = u.Name,
                DepartmentName = u.Department?.Name ?? "General",
                TripsCount = trips,
                TotalApprovedBudget = approvedBudget,
                TotalReimbursedExpenses = reimbursedExpenses
            });
        }

        return reports.OrderByDescending(r => r.TotalApprovedBudget).ToList();
    }

    public async Task<CostSummaryReportDto> GetCostSummaryReportAsync(CancellationToken cancellationToken = default)
    {
        var totalTrips = await _context.TravelRequests
            .AsNoTracking()
            .CountAsync(r => r.Status != "Draft" && r.Status != "Cancelled", cancellationToken);

        var totalCommitted = await _context.TravelRequests
            .AsNoTracking()
            .Where(r => r.Status == "Approved" || r.Status == "Completed")
            .SumAsync(r => (decimal?)r.EstimatedBudget, cancellationToken) ?? 0m;

        var totalReimbursed = await _context.Expenses
            .AsNoTracking()
            .Where(e => e.Status == "Reimbursed")
            .SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0m;

        var totalPending = await _context.Expenses
            .AsNoTracking()
            .Where(e => e.Status == "Pending")
            .SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0m;

        var avgCost = totalTrips > 0 ? Math.Round(totalCommitted / totalTrips, 2) : 0m;

        return new CostSummaryReportDto
        {
            TotalTrips = totalTrips,
            TotalCommittedBudget = totalCommitted,
            TotalReimbursed = totalReimbursed,
            TotalPendingReimbursement = totalPending,
            AverageCostPerTrip = avgCost
        };
    }
}
