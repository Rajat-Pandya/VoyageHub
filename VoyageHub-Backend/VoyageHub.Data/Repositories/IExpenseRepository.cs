using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Data.Entities;

namespace VoyageHub.Data.Repositories;

public interface IExpenseRepository : IRepository<Expense>
{
    Task<List<Expense>> GetPendingExpensesAsync(string? category, bool? highPriorityOnly, CancellationToken cancellationToken = default);
    Task<List<Expense>> GetExpensesByFilterAsync(string? status, string? category, bool? highPriorityOnly, CancellationToken cancellationToken = default);
    Task<List<Expense>> GetExpensesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalPendingAmountAsync(CancellationToken cancellationToken = default);
    Task<TravelRequest?> FindTravelRequestWithEmployeeAsync(Guid requestId, CancellationToken cancellationToken = default);
    Task<Expense?> GetByIdWithRequestAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Expense>> GetPendingExpensesByIdsAsync(List<Guid>? expenseIds, CancellationToken cancellationToken = default);
}
