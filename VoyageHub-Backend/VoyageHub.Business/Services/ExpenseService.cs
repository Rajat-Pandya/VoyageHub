using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Business.Dtos;
using VoyageHub.Data.Entities;
using VoyageHub.Data.Repositories;

namespace VoyageHub.Business.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;

    public ExpenseService(
        IExpenseRepository expenseRepository,
        IAuditService auditService,
        ICurrentUserService currentUserService)
    {
        _expenseRepository = expenseRepository;
        _auditService = auditService;
        _currentUserService = currentUserService;
    }

    public async Task<ExpenseReviewSummaryDto> GetPendingSummaryAsync(string? category, bool? highPriorityOnly, CancellationToken cancellationToken = default)
    {
        var expenses = await _expenseRepository.GetPendingExpensesAsync(category, highPriorityOnly, cancellationToken);
        var total = await _expenseRepository.GetTotalPendingAmountAsync(cancellationToken);

        return new ExpenseReviewSummaryDto
        {
            TotalPendingAmount = total,
            PendingClaims = expenses.Select(MapToExpenseItemDto).ToList()
        };
    }

    public async Task<ExpenseReviewSummaryDto> GetExpensesSummaryAsync(string? status, string? category, bool? highPriorityOnly, CancellationToken cancellationToken = default)
    {
        var expenses = await _expenseRepository.GetExpensesByFilterAsync(status, category, highPriorityOnly, cancellationToken);
        var totalPending = await _expenseRepository.GetTotalPendingAmountAsync(cancellationToken);

        return new ExpenseReviewSummaryDto
        {
            TotalPendingAmount = totalPending,
            PendingClaims = expenses.Select(MapToExpenseItemDto).ToList()
        };
    }

    public async Task<List<ExpenseItemDto>> GetMyExpensesAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return new List<ExpenseItemDto>();
        }

        var expenses = await _expenseRepository.GetExpensesByUserIdAsync(_currentUserService.UserId.Value, cancellationToken);
        return expenses.Select(MapToExpenseItemDto).ToList();
    }

    public async Task<List<TripReimbursementReportDto>> GetMyReimbursementReportsAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return new List<TripReimbursementReportDto>();
        }

        var trips = await _expenseRepository.GetUserTripsForReimbursementAsync(_currentUserService.UserId.Value, cancellationToken);
        return trips.Select(MapToTripReportDto).ToList();
    }

    private static (List<ItinerarySegment> UnclaimedSegments, List<Stay> UnclaimedStays, List<Expense> ActiveExpenses) PartitionTripCosts(TravelRequest req)
    {
        var activeExpenses = req.Expenses?
            .Where(x => !string.Equals(x.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
            .ToList() ?? new List<Expense>();

        var unclaimedSegments = req.ItinerarySegments?
            .Where(s => !activeExpenses.Any(x =>
                (string.Equals(x.Category, "Airfare", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(x.Category, "Transport", StringComparison.OrdinalIgnoreCase)) &&
                x.Amount == s.Cost))
            .ToList() ?? new List<ItinerarySegment>();

        var unclaimedStays = req.Stays?
            .Where(st => !activeExpenses.Any(x =>
                (string.Equals(x.Category, "Hotel", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(x.Category, "Lodging", StringComparison.OrdinalIgnoreCase)) &&
                x.Amount == st.Cost))
            .ToList() ?? new List<Stay>();

        return (unclaimedSegments, unclaimedStays, activeExpenses);
    }

    private static TripReimbursementReportDto MapToTripReportDto(TravelRequest req)
    {
        var (unclaimedSegments, unclaimedStays, activeExpenses) = PartitionTripCosts(req);
        var corporatePaid = unclaimedSegments.Sum(s => s.Cost) + unclaimedStays.Sum(st => st.Cost);
        var claimedTotal = activeExpenses.Sum(e => e.Amount);
        var pendingAmount = activeExpenses.Where(e => string.Equals(e.Status, "Pending", StringComparison.OrdinalIgnoreCase)).Sum(e => e.Amount);
        var reimbursedAmount = activeExpenses.Where(e => string.Equals(e.Status, "Reimbursed", StringComparison.OrdinalIgnoreCase)).Sum(e => e.Amount);

        var lineItems = new List<ReimbursementLineItemDto>();
        foreach (var seg in unclaimedSegments)
        {
            lineItems.Add(new ReimbursementLineItemDto
            {
                Kind = "Itinerary",
                Category = seg.Mode,
                Description = $"{seg.Origin} → {seg.Destination}" + (string.IsNullOrWhiteSpace(seg.Carrier) ? "" : $" ({seg.Carrier})"),
                Amount = seg.Cost,
                Status = "Corporate-Paid",
                Date = seg.DepartureTime
            });
        }
        foreach (var stay in unclaimedStays)
        {
            lineItems.Add(new ReimbursementLineItemDto
            {
                Kind = "Itinerary",
                Category = "Hotel",
                Description = $"{stay.HotelName} ({stay.Nights} Nights)",
                Amount = stay.Cost,
                Status = "Corporate-Paid",
                Date = stay.CheckIn
            });
        }
        foreach (var exp in (req.Expenses ?? new List<Expense>()).OrderByDescending(e => e.SubmittedAt))
        {
            lineItems.Add(new ReimbursementLineItemDto
            {
                Kind = "Expense",
                Category = exp.Category,
                Description = $"{exp.Category} Claim",
                Amount = exp.Amount,
                Status = exp.Status,
                Date = exp.SubmittedAt,
                ProofPath = exp.ProofPath
            });
        }

        string reportStatus;
        if (req.Expenses == null || req.Expenses.Count == 0)
        {
            reportStatus = "No Claims";
        }
        else if (pendingAmount > 0 && reimbursedAmount > 0)
        {
            reportStatus = "Partially Reimbursed";
        }
        else if (pendingAmount > 0)
        {
            reportStatus = "Pending";
        }
        else if (reimbursedAmount > 0)
        {
            reportStatus = "Reimbursed";
        }
        else
        {
            reportStatus = "Rejected";
        }

        return new TripReimbursementReportDto
        {
            RequestId = req.Id,
            RequestCode = req.RequestCode,
            Destination = req.Destination,
            Purpose = req.Purpose,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            TripStatus = req.Status,
            ApprovedBudget = req.EstimatedBudget,
            TotalTripSpend = corporatePaid + claimedTotal,
            CorporatePaidAmount = corporatePaid,
            ClaimedTotal = claimedTotal,
            PendingAmount = pendingAmount,
            ReimbursedAmount = reimbursedAmount,
            ReportStatus = reportStatus,
            LineItems = lineItems
        };
    }

    private static ExpenseItemDto MapToExpenseItemDto(Expense e)
    {
        var req = e.Request;
        var approvedBudget = req?.EstimatedBudget ?? 0m;
        decimal actualSpend;
        decimal claimedTotal;
        decimal corporatePaid;
        if (req == null)
        {
            actualSpend = e.Amount;
            claimedTotal = string.Equals(e.Status, "Rejected", StringComparison.OrdinalIgnoreCase) ? 0m : e.Amount;
            corporatePaid = 0m;
        }
        else
        {
            var (unclaimedSegments, unclaimedStays, activeExpenses) = PartitionTripCosts(req);
            claimedTotal = activeExpenses.Sum(x => x.Amount);
            corporatePaid = unclaimedSegments.Sum(s => s.Cost) + unclaimedStays.Sum(st => st.Cost);
            actualSpend = corporatePaid + claimedTotal;
        }

        return new ExpenseItemDto
        {
            Id = e.Id,
            RequestId = e.RequestId,
            RequestCode = req?.RequestCode ?? "#TR-2048",
            EmployeeName = req?.Employee?.Name ?? "Employee",
            DepartmentName = req?.Employee?.Department?.Name ?? "Sales",
            TripPurpose = req?.Purpose ?? "Trip Expense",
            Category = e.Category,
            Amount = e.Amount,
            ProofPath = e.ProofPath,
            Status = e.Status,
            SubmittedAt = e.SubmittedAt,
            ApprovedBudget = approvedBudget,
            ActualSpend = actualSpend,
            TripClaimedTotal = claimedTotal,
            CorporatePaidAmount = corporatePaid,
            IsOverBudget = approvedBudget > 0 && actualSpend > approvedBudget
        };
    }

    public async Task<(bool Success, string Message, Expense? Expense)> SubmitExpenseAsync(SubmitExpenseDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
        {
            return (false, "Invalid expense data.", null);
        }

        if (dto.Amount <= 0)
        {
            return (false, "Expense amount must be greater than zero.", null);
        }

        var request = await _expenseRepository.FindTravelRequestWithEmployeeAsync(dto.RequestId, cancellationToken);
        if (request == null)
        {
            return (false, "Travel request not found.", null);
        }

        if (!IsOwnerOrAdmin(request.EmployeeId))
        {
            return (false, "You can only submit expenses for your own travel requests.", null);
        }

        if (!string.Equals(request.Status, "Approved", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Expenses can only be submitted for Approved travel requests.", null);
        }

        var expense = new Expense
        {
            RequestId = dto.RequestId,
            Category = string.IsNullOrWhiteSpace(dto.Category) ? "Airfare" : dto.Category,
            Amount = dto.Amount,
            ProofPath = string.IsNullOrWhiteSpace(dto.ProofPath) ? null : dto.ProofPath.Trim(),
            Status = "Pending",
            SubmittedAt = DateTime.UtcNow
        };

        await _expenseRepository.AddAsync(expense, cancellationToken);

        await _auditService.LogAsync(
            entityType: "EXPENSE",
            entityId: expense.Id,
            action: "Submit Expense",
            actorId: _currentUserService.UserId ?? request.EmployeeId,
            actorName: _currentUserService.UserName ?? request.Employee?.Name ?? "Employee",
            changeDetail: $"Submitted ₹{expense.Amount:N0} {expense.Category} claim for {request.RequestCode}",
            severity: "LOW",
            cancellationToken: cancellationToken);

        await _expenseRepository.SaveChangesAsync(cancellationToken);

        return (true, string.Empty, expense);
    }

    public async Task<(bool Success, string Message)> TakeActionAsync(Guid id, ExpenseActionDto dto, CancellationToken cancellationToken = default)
    {
        var expense = await _expenseRepository.GetByIdWithRequestAsync(id, cancellationToken);
        if (expense == null)
        {
            return (false, "Expense claim not found.");
        }

        var isApprove = string.Equals(dto.Action, "Approve", StringComparison.OrdinalIgnoreCase);
        expense.Status = isApprove ? "Reimbursed" : "Rejected";

        await _auditService.LogAsync(
            entityType: "EXPENSE",
            entityId: expense.Id,
            action: isApprove ? "Reimbursed Claim" : "Rejected Claim",
            actorId: _currentUserService.UserId,
            actorName: _currentUserService.UserName ?? "Finance Officer",
            changeDetail: $"Processed ₹{expense.Amount:N0} {expense.Category} claim for {expense.Request?.RequestCode} ({expense.Status})",
            severity: "LOW",
            cancellationToken: cancellationToken);

        await _expenseRepository.SaveChangesAsync(cancellationToken);

        return (true, $"Expense claim {expense.Status.ToLower()}.");
    }

    public async Task<(bool Success, string Message)> BatchPayoutAsync(BatchPayoutDto? dto, CancellationToken cancellationToken = default)
    {
        var toPayout = await _expenseRepository.GetPendingExpensesByIdsAsync(dto?.ExpenseIds, cancellationToken);

        foreach (var exp in toPayout)
        {
            exp.Status = "Reimbursed";
        }

        await _auditService.LogAsync(
            entityType: "EXPENSE",
            entityId: null,
            action: "Batch Payout",
            actorId: _currentUserService.UserId,
            actorName: _currentUserService.UserName ?? "Finance Officer",
            changeDetail: $"Processed batch payout for {toPayout.Count} claims totaling ₹{toPayout.Sum(e => e.Amount):N0}",
            severity: "MEDIUM",
            cancellationToken: cancellationToken);

        await _expenseRepository.SaveChangesAsync(cancellationToken);

        return (true, $"Successfully processed payout for {toPayout.Count} claims.");
    }

    public async Task<(bool Success, string Message, string? ProofPath)> AttachProofAsync(Guid id, string proofPath, CancellationToken cancellationToken = default)
    {
        var expense = await _expenseRepository.GetByIdWithRequestAsync(id, cancellationToken);
        if (expense == null)
        {
            return (false, "Expense claim not found.", null);
        }

        if (expense.Request != null && !IsOwnerOrAdmin(expense.Request.EmployeeId))
        {
            return (false, "You can only attach proof to your own expense claims.", null);
        }

        expense.ProofPath = proofPath;
        await _expenseRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            entityType: "EXPENSE",
            entityId: expense.Id,
            action: "Attach Proof",
            actorId: _currentUserService.UserId,
            actorName: _currentUserService.UserName ?? "Employee",
            changeDetail: $"Attached receipt proof '{proofPath}' to expense claim",
            severity: "LOW",
            cancellationToken: cancellationToken);

        return (true, "Proof document attached successfully.", proofPath);
    }

    private bool IsOwnerOrAdmin(Guid requestOwnerId)
    {
        var callerId = _currentUserService.UserId;
        if (!callerId.HasValue) return true;
        if (string.Equals(_currentUserService.UserRole, "Admin", StringComparison.OrdinalIgnoreCase)) return true;
        return callerId.Value == requestOwnerId;
    }
}
