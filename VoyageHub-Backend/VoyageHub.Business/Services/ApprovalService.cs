using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Helpers;
using VoyageHub.Data.Entities;
using VoyageHub.Data.Repositories;

namespace VoyageHub.Business.Services;

public class ApprovalService : IApprovalService
{
    private readonly IApprovalStepRepository _approvalStepRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;

    public ApprovalService(
        IApprovalStepRepository approvalStepRepository,
        IAuditService auditService,
        ICurrentUserService currentUserService)
    {
        _approvalStepRepository = approvalStepRepository;
        _auditService = auditService;
        _currentUserService = currentUserService;
    }

    public async Task<ApprovalQueueSummaryDto> GetQueueAsync(Guid? approverId, string? role, CancellationToken cancellationToken = default)
    {
        var callerRole = _currentUserService.UserRole;
        var callerId = _currentUserService.UserId;

        string? effectiveRole = role;
        Guid? effectiveApproverId = approverId;

        if (!string.IsNullOrWhiteSpace(callerRole))
        {
            if (string.Equals(callerRole, "Employee", StringComparison.OrdinalIgnoreCase))
            {
                // Employee cannot fetch approval queue
                return new ApprovalQueueSummaryDto
                {
                    PendingTotal = 0,
                    AvgResponseTimeHours = 0.0,
                    PolicyCompliancePercent = 100,
                    Items = new List<ApprovalQueueItemDto>()
                };
            }
            else if (string.Equals(callerRole, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                effectiveRole = "Manager";
                effectiveApproverId = callerId;
            }
            else if (string.Equals(callerRole, "Finance", StringComparison.OrdinalIgnoreCase))
            {
                effectiveRole = "Finance";
                effectiveApproverId = null;
            }
            else if (string.Equals(callerRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                effectiveRole = role;
                effectiveApproverId = approverId;
            }
        }

        var steps = await _approvalStepRepository.GetPendingStepsAsync(effectiveApproverId, effectiveRole, cancellationToken);
        if (callerId.HasValue && callerId.Value != Guid.Empty)
        {
            steps = steps.Where(s => s.Request != null && s.Request.EmployeeId != callerId.Value).ToList();
        }
        var avgResponseTimeHours = await _approvalStepRepository.GetAverageResponseTimeHoursAsync(effectiveApproverId, effectiveRole, cancellationToken);

        var pendingTotal = steps.Sum(s => s.Request!.EstimatedBudget);

        var items = steps.Select(s =>
        {
            var req = s.Request!;
            var (_, flagReasons) = PolicyEvaluator.Evaluate(
                req.EstimatedBudget,
                req.TravelClass,
                req.Employee?.Department?.Policy,
                req.Employee?.Department?.Name,
                req.PolicyCompliance,
                req.IsInternational);

            return new ApprovalQueueItemDto
            {
                StepId = s.Id,
                RequestId = s.RequestId,
                RequestCode = req.RequestCode,
                EmployeeName = req.Employee?.Name ?? "Employee",
                DepartmentName = req.Employee?.Department?.Name ?? "Sales Dept.",
                Destination = req.Destination,
                Purpose = req.Purpose,
                TravelDates = $"{req.StartDate:dd MMM} – {req.EndDate:dd MMM}",
                Budget = req.EstimatedBudget,
                PolicyStatus = req.PolicyCompliance,
                PolicyFlagReasons = flagReasons,
                Status = s.Status,
                Level = s.Level
            };
        }).ToList();

        var compliancePercent = items.Count > 0
            ? (int)Math.Round((double)items.Count(i => i.PolicyStatus != "Flagged") / items.Count * 100)
            : 100;

        return new ApprovalQueueSummaryDto
        {
            PendingTotal = pendingTotal,
            AvgResponseTimeHours = avgResponseTimeHours,
            PolicyCompliancePercent = compliancePercent,
            Items = items
        };
    }

    public async Task<(bool Success, string Message)> TakeActionAsync(Guid stepId, ApprovalActionDto dto, CancellationToken cancellationToken = default)
    {
        var step = await _approvalStepRepository.GetByIdWithRequestAndApproverAsync(stepId, cancellationToken);
        if (step == null)
        {
            return (false, "Approval step not found.");
        }

        if (step.Request != null && _currentUserService.UserId.HasValue && step.Request.EmployeeId == _currentUserService.UserId.Value)
        {
            return (false, "You cannot approve or reject your own travel request.");
        }

        var isApprove = string.Equals(dto.Action, "Approve", StringComparison.OrdinalIgnoreCase);
        step.Status = isApprove ? "Approved" : "Rejected";
        step.Comment = dto.Comment;
        step.DecidedAt = DateTime.UtcNow;

        if (step.Request != null)
        {
            if (isApprove)
            {
                if (step.Level == "Manager")
                {
                    // Route to Finance (Step 2)
                    var financeStep = await _approvalStepRepository.FindFinanceStepAsync(step.RequestId, cancellationToken);
                    if (financeStep != null)
                    {
                        financeStep.Status = "Pending";
                        step.Request.Status = "Finance Review";
                    }
                    else
                    {
                        step.Request.Status = "Approved";
                    }
                }
                else if (step.Level == "Finance")
                {
                    step.Request.Status = "Approved";
                }
                else
                {
                    step.Request.Status = "Approved";
                }
            }
            else
            {
                step.Request.Status = "Rejected";
                // Cancel any subsequent waiting steps
                var remaining = await _approvalStepRepository.GetRemainingNonApprovedStepsAsync(step.RequestId, step.Id, cancellationToken);
                foreach (var rem in remaining)
                {
                    rem.Status = "Cancelled";
                }
            }
            step.Request.UpdatedAt = DateTime.UtcNow;

            await _auditService.LogAsync(
                entityType: "APPROVAL",
                entityId: step.RequestId,
                action: isApprove ? "Approved" : "Rejected",
                actorId: _currentUserService.UserId ?? step.ApproverId,
                actorName: _currentUserService.UserName ?? step.Approver?.Name ?? "Approver",
                changeDetail: $"{step.Level} {dto.Action} for {step.Request.RequestCode}. {(string.IsNullOrWhiteSpace(dto.Comment) ? "" : "Note: " + dto.Comment)}",
                severity: step.Request.PolicyCompliance == "Flagged" ? "MEDIUM" : "LOW",
                cancellationToken: cancellationToken);
        }

        await _approvalStepRepository.SaveChangesAsync(cancellationToken);

        return (true, $"Request successfully {step.Status.ToLower()}.");
    }
}
