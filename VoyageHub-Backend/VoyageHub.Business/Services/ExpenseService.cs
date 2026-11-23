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

        var claims = expenses.Select(e => new ExpenseItemDto
        {
            Id = e.Id,
            RequestId = e.RequestId,
            RequestCode = e.Request?.RequestCode ?? "#TR-2048",
            EmployeeName = e.Request?.Employee?.Name ?? "Employee",
            DepartmentName = e.Request?.Employee?.Department?.Name ?? "Sales",
            TripPurpose = e.Request?.Purpose ?? "Trip Expense",
            Category = e.Category,
            Amount = e.Amount,
            ProofPath = e.ProofPath,
            Status = e.Status,
            SubmittedAt = e.SubmittedAt
        }).ToList();

        return new ExpenseReviewSummaryDto
        {
            TotalPendingAmount = total,
            PendingClaims = claims
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

        var expense = new Expense
        {
            RequestId = dto.RequestId,
            Category = string.IsNullOrWhiteSpace(dto.Category) ? "Airfare" : dto.Category,
            Amount = dto.Amount,
            ProofPath = dto.ProofPath ?? $"receipt_{dto.Category.ToLower()}_{RandomNumberGenerator.GetInt32(100, 999)}.pdf",
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
}
