using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Data.Entities;

namespace VoyageHub.Data.Repositories;

public interface IApprovalStepRepository : IRepository<ApprovalStep>
{
    Task<List<ApprovalStep>> GetPendingStepsAsync(Guid? approverId, string? role, CancellationToken cancellationToken = default);
    Task<double> GetAverageResponseTimeHoursAsync(Guid? approverId, string? role, CancellationToken cancellationToken = default);
    Task<ApprovalStep?> GetByIdWithRequestAndApproverAsync(Guid stepId, CancellationToken cancellationToken = default);
    Task<ApprovalStep?> FindFinanceStepAsync(Guid requestId, CancellationToken cancellationToken = default);
    Task<List<ApprovalStep>> GetRemainingNonApprovedStepsAsync(Guid requestId, Guid currentStepId, CancellationToken cancellationToken = default);
}
