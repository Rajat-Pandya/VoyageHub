using System;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Business.Dtos;

namespace VoyageHub.Business.Services;

public interface IApprovalService
{
    Task<ApprovalQueueSummaryDto> GetQueueAsync(Guid? approverId, string? role, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> TakeActionAsync(Guid stepId, ApprovalActionDto dto, CancellationToken cancellationToken = default);
}
