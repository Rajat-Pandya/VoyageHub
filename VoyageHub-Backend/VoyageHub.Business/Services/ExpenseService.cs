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

    private static ExpenseItemDto MapToExpenseItemDto(Expense e)
    {
        var req = e.Request;
        var approvedBudget = req?.EstimatedBudget ?? 0m;
        var actualSpend = req == null
            ? e.Amount
            : (req.ItinerarySegments?.Sum(s => s.Cost) ?? 0m)
              + (req.Stays?.Sum(st => st.Cost) ?? 0m)
              + (req.Expenses?.Where(x => !string.Equals(x.Status, "Rejected", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Amount) ?? 0m);

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
