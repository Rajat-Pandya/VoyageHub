using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Data.Entities;

namespace VoyageHub.Data.Repositories;

public interface ITravelRequestRepository : IRepository<TravelRequest>
{
    Task<List<TravelRequest>> GetRequestsAsync(string? status, string? search, Guid? employeeId, int? limit, Guid? managerId = null, CancellationToken cancellationToken = default);
    Task<(int Drafts, int Pending, int Upcoming, decimal Reimbursement)> GetStatsAsync(Guid? employeeId, Guid? managerId = null, CancellationToken cancellationToken = default);
    Task<TravelRequest?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetEmployeeWithDepartmentAndManagerAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<Guid> GetFirstEmployeeIdAsync(CancellationToken cancellationToken = default);
    Task<Guid?> GetFirstManagerIdAsync(CancellationToken cancellationToken = default);
    Task<Guid> GetFirstFinanceIdAsync(CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task<TravelRequest?> GetDraftForSubmissionAsync(Guid id, CancellationToken cancellationToken = default);
    void RemoveApprovalSteps(IEnumerable<ApprovalStep> steps);
    Task AddApprovalStepAsync(ApprovalStep step, CancellationToken cancellationToken = default);
    Task AddSegmentAsync(ItinerarySegment segment, CancellationToken cancellationToken = default);
    Task AddStayAsync(Stay stay, CancellationToken cancellationToken = default);
}

