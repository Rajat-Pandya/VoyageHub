using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Business.Dtos;
using VoyageHub.Data.Entities;

namespace VoyageHub.Business.Services;

public interface IExpenseService
{
    Task<ExpenseReviewSummaryDto> GetPendingSummaryAsync(string? category, bool? highPriorityOnly, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message, Expense? Expense)> SubmitExpenseAsync(SubmitExpenseDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> TakeActionAsync(Guid id, ExpenseActionDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> BatchPayoutAsync(BatchPayoutDto? dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message, string? ProofPath)> AttachProofAsync(Guid id, string proofPath, CancellationToken cancellationToken = default);
}
