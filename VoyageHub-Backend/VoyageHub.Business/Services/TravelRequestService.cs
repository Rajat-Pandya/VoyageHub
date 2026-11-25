using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Helpers;
using VoyageHub.Data.Entities;
using VoyageHub.Data.Repositories;

namespace VoyageHub.Business.Services;

public class TravelRequestService : ITravelRequestService
{
    private readonly ITravelRequestRepository _travelRequestRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;

    public TravelRequestService(
        ITravelRequestRepository travelRequestRepository,
        IAuditService auditService,
        ICurrentUserService currentUserService)
    {
        _travelRequestRepository = travelRequestRepository;
        _auditService = auditService;
        _currentUserService = currentUserService;
    }

    public async Task<List<TravelRequestListDto>> GetRequestsAsync(
        string? status,
        string? search,
        Guid? employeeId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var callerId = _currentUserService.UserId;
        var callerRole = _currentUserService.UserRole;

        Guid? targetEmployeeId = employeeId;
        Guid? targetManagerId = null;

        if (string.Equals(callerRole, "Employee", StringComparison.OrdinalIgnoreCase))
        {
            if (callerId.HasValue)
            {
                targetEmployeeId = callerId.Value;
            }
        }
        else if (string.Equals(callerRole, "Manager", StringComparison.OrdinalIgnoreCase))
        {
            if (callerId.HasValue)
            {
                targetManagerId = callerId.Value;
            }
        }

        var results = await _travelRequestRepository.GetRequestsAsync(status, search, targetEmployeeId, limit, targetManagerId, cancellationToken);

        var dtoList = results.Select(r =>
        {
            var activeApprover = r.ApprovalSteps
                .Where(s => s.Approver != null)
                .Select(s => s.Approver!.Name)
                .FirstOrDefault() ?? "Priya Nair";

            return new TravelRequestListDto
            {
                Id = r.Id,
                RequestCode = r.RequestCode,
                Destination = r.Destination,
                IsInternational = r.IsInternational,
                Purpose = r.Purpose,
                DatesFormatted = $"{r.StartDate:dd MMM} – {r.EndDate:dd MMM}",
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                SubmittedDate = r.CreatedAt,
                EstimatedBudget = r.EstimatedBudget,
                ApproverName = activeApprover,
                Status = r.Status,
                PolicyCompliance = r.PolicyCompliance,
                TravelClass = r.TravelClass,
                EmployeeName = r.Employee?.Name ?? "Rohan Sharma",
                DepartmentName = r.Employee?.Department?.Name ?? "Sales & Business Dev (Mumbai HQ)"
            };
        }).ToList();

        return dtoList;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(Guid? employeeId, CancellationToken cancellationToken = default)
    {
        var callerId = _currentUserService.UserId;
        var callerRole = _currentUserService.UserRole;

        Guid? targetEmployeeId = employeeId;
        Guid? targetManagerId = null;

        if (string.Equals(callerRole, "Employee", StringComparison.OrdinalIgnoreCase))
        {
            if (callerId.HasValue)
            {
                targetEmployeeId = callerId.Value;
            }
        }
        else if (string.Equals(callerRole, "Manager", StringComparison.OrdinalIgnoreCase))
        {
            if (callerId.HasValue)
            {
                targetManagerId = callerId.Value;
            }
        }

        var stats = await _travelRequestRepository.GetStatsAsync(targetEmployeeId, targetManagerId, cancellationToken);

        return new DashboardStatsDto
        {
            DraftsCount = stats.Drafts,
            PendingCount = stats.Pending,
            UpcomingTripsCount = stats.Upcoming,
            TotalReimbursement = stats.Reimbursement
        };
    }

    public async Task<TravelRequestDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var r = await _travelRequestRepository.GetDetailAsync(id, cancellationToken);
        if (r == null) return null;

        var callerId = _currentUserService.UserId;
        var callerRole = _currentUserService.UserRole;

        if (string.Equals(callerRole, "Employee", StringComparison.OrdinalIgnoreCase))
        {
            if (callerId.HasValue && r.EmployeeId != callerId.Value)
            {
                return null;
            }
        }
        else if (string.Equals(callerRole, "Manager", StringComparison.OrdinalIgnoreCase))
        {
            if (callerId.HasValue && r.EmployeeId != callerId.Value && r.Employee?.ManagerId != callerId.Value)
            {
                return null;
            }
        }

        var durationDays = Math.Max(1, (r.EndDate - r.StartDate).Days);
        var durationNights = Math.Max(0, durationDays - 1);

        return new TravelRequestDetailDto
        {
            Id = r.Id,
            RequestCode = r.RequestCode,
            EmployeeId = r.EmployeeId,
            EmployeeName = r.Employee?.Name ?? "Employee",
            EmployeeEmail = r.Employee?.Email ?? "",
            DepartmentName = r.Employee?.Department?.Name ?? "General",
            Destination = r.Destination,
            IsInternational = r.IsInternational,
            Purpose = r.Purpose,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            TripDurationDays = durationDays,
            TripDurationNights = durationNights,
            EstimatedBudget = r.EstimatedBudget,
            TravelClass = r.TravelClass,
            Justification = r.Justification,
            Status = r.Status,
            PolicyCompliance = r.PolicyCompliance,
            PolicyFlagReasons = PolicyEvaluator.Evaluate(
                r.EstimatedBudget,
                r.TravelClass,
                r.Employee?.Department?.Policy,
                r.Employee?.Department?.Name,
                r.PolicyCompliance,
                r.IsInternational).Reasons,
            SubmittedDate = r.CreatedAt,
            PrimaryContact = "VoyageHub India Corporate Desk (+91-80-4000-8800)",
            Segments = r.ItinerarySegments.Select(s => new ItinerarySegmentDto
            {
                Id = s.Id,
                Mode = s.Mode,
                Carrier = s.Carrier,
                Origin = s.Origin,
                Destination = s.Destination,
                DepartureTime = s.DepartureTime,
                ArrivalTime = s.ArrivalTime,
                ConfirmationNumber = s.ConfirmationNumber,
                Cost = s.Cost
            }).ToList(),
            Stays = r.Stays.Select(st => new StayDto
            {
                Id = st.Id,
                HotelName = st.HotelName,
                CheckIn = st.CheckIn,
                CheckOut = st.CheckOut,
                Nights = st.Nights,
                ConfirmationNumber = st.ConfirmationNumber,
                Cost = st.Cost
            }).ToList(),
            Expenses = r.Expenses.Select(e => new ExpenseItemDto
            {
                Id = e.Id,
                RequestId = e.RequestId,
                RequestCode = r.RequestCode,
                EmployeeName = r.Employee?.Name ?? "Employee",
                DepartmentName = r.Employee?.Department?.Name ?? "General",
                TripPurpose = r.Purpose,
                Category = e.Category,
                Amount = e.Amount,
                ProofPath = e.ProofPath,
                Status = e.Status,
                SubmittedAt = e.SubmittedAt
            }).ToList(),
            ApprovalHistory = r.ApprovalSteps.Select(a => new ApprovalStepItemDto
            {
                Id = a.Id,
                Level = a.Level,
                ApproverName = a.Approver?.Name ?? "Approver",
                Status = a.Status,
                Comment = a.Comment,
                DecidedAt = a.DecidedAt
            }).ToList()
        };
    }

    public async Task<(bool Success, string Message, TravelRequestDetailDto? Request)> CreateRequestAsync(
        CreateTravelRequestDto dto,
        Guid? employeeId,
        CancellationToken cancellationToken = default)
    {
        if (dto == null) return (false, "Invalid request data.", null);

        if (dto.StartDate > dto.EndDate)
        {
            return (false, "Travel start date cannot be after end date.", null);
        }

        if (dto.EstimatedBudget <= 0)
        {
            return (false, "Estimated budget must be greater than zero.", null);
        }

        if (string.IsNullOrWhiteSpace(dto.Destination))
        {
            return (false, "Destination is required.", null);
        }

        if (string.IsNullOrWhiteSpace(dto.Purpose))
        {
            return (false, "Purpose is required.", null);
        }

        var callerId = _currentUserService.UserId;
        var callerRole = _currentUserService.UserRole;

        var empId = string.Equals(callerRole, "Employee", StringComparison.OrdinalIgnoreCase) && callerId.HasValue
            ? callerId.Value
            : (employeeId.HasValue && employeeId != Guid.Empty
                ? employeeId.Value
                : (callerId ?? await _travelRequestRepository.GetFirstEmployeeIdAsync(cancellationToken)));

        var employee = await _travelRequestRepository.GetEmployeeWithDepartmentAndManagerAsync(empId, cancellationToken);
        if (employee == null) return (false, "Employee account not found.", null);

        var count = await _travelRequestRepository.GetCountAsync(cancellationToken);
        var code = $"#TR-{2050 + count + 1}";

        var (isFlagged, _) = PolicyEvaluator.Evaluate(
            dto.EstimatedBudget,
            dto.TravelClass,
            employee.Department?.Policy,
            employee.Department?.Name,
            null,
            dto.IsInternational);

        var status = dto.SaveAsDraft ? "Draft" : "In Review";

        var request = new TravelRequest
        {
            RequestCode = code,
            EmployeeId = empId,
            Destination = dto.Destination.Trim(),
            IsInternational = dto.IsInternational,
            Purpose = dto.Purpose.Trim(),
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            EstimatedBudget = dto.EstimatedBudget,
            TravelClass = string.IsNullOrWhiteSpace(dto.TravelClass) ? "Economy (Standard)" : dto.TravelClass,
            Justification = dto.Justification,
            Status = status,
            PolicyCompliance = isFlagged ? "Flagged" : "Standard",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (dto.Segments != null)
        {
            foreach (var s in dto.Segments)
            {
                request.ItinerarySegments.Add(new ItinerarySegment
                {
                    Mode = s.Mode ?? "Flight",
                    Carrier = s.Carrier ?? "LUFTHANSA LH123",
                    Origin = s.Origin,
                    Destination = s.Destination,
                    DepartureTime = s.DepartureTime,
                    ArrivalTime = s.ArrivalTime,
                    ConfirmationNumber = s.ConfirmationNumber ?? $"#AJK{RandomNumberGenerator.GetInt32(10, 99)}L",
                    Cost = s.Cost
                });
            }
        }

        if (dto.Stays != null)
        {
            foreach (var st in dto.Stays)
            {
                request.Stays.Add(new Stay
                {
                    HotelName = st.HotelName,
                    CheckIn = st.CheckIn,
                    CheckOut = st.CheckOut,
                    Nights = st.Nights > 0 ? st.Nights : 1,
                    ConfirmationNumber = st.ConfirmationNumber ?? $"#HY-{RandomNumberGenerator.GetInt32(100000, 999999)}",
                    Cost = st.Cost
                });
            }
        }

        if (!dto.SaveAsDraft)
        {
            var steps = await BuildWorkflowStepsAsync(request.Id, employee, cancellationToken);
            if (steps.All(s => s.Level == "Finance"))
            {
                request.Status = "Finance Review";
            }
            foreach (var step in steps)
            {
                request.ApprovalSteps.Add(step);
            }
        }

        await _travelRequestRepository.AddAsync(request, cancellationToken);

        await _auditService.LogAsync(
            entityType: "REQUEST",
            entityId: request.Id,
            action: dto.SaveAsDraft ? "Create Draft" : "Submit Request",
            actorId: _currentUserService.UserId ?? empId,
            actorName: _currentUserService.UserName ?? employee.Name,
            changeDetail: $"Created {request.RequestCode} for {request.Destination} (₹{request.EstimatedBudget:N0})",
            severity: isFlagged ? "MEDIUM" : "LOW",
            cancellationToken: cancellationToken);

        await _travelRequestRepository.SaveChangesAsync(cancellationToken);

        var resultDto = new TravelRequestDetailDto
        {
            Id = request.Id,
            RequestCode = request.RequestCode,
            Destination = request.Destination,
            Purpose = request.Purpose,
            Status = request.Status,
            EstimatedBudget = request.EstimatedBudget
        };

        return (true, string.Empty, resultDto);
    }

    public async Task<(bool Success, string Message)> CancelRequestAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var r = await _travelRequestRepository.GetByIdAsync(id, cancellationToken);
        if (r == null) return (false, "Not found");

        r.Status = "Cancelled";
        r.UpdatedAt = DateTime.UtcNow;

        await _auditService.LogAsync(
            entityType: "REQUEST",
            entityId: r.Id,
            action: "Cancelled",
            actorId: _currentUserService.UserId ?? r.EmployeeId,
            actorName: _currentUserService.UserName ?? "User",
            changeDetail: $"Cancelled trip {r.RequestCode}",
            severity: "LOW",
            cancellationToken: cancellationToken);

        await _travelRequestRepository.SaveChangesAsync(cancellationToken);
        return (true, "Request cancelled successfully.");
    }

    public async Task<(int StatusCode, object Response)> SubmitDraftAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var r = await _travelRequestRepository.GetDraftForSubmissionAsync(id, cancellationToken);

        if (r == null) return (404, "Travel request not found.");
        if (r.Status != "Draft") return (400, "Only Draft requests can be submitted.");

        var (isFlagged, _) = PolicyEvaluator.Evaluate(
            r.EstimatedBudget,
            r.TravelClass,
            r.Employee?.Department?.Policy,
            r.Employee?.Department?.Name,
            null,
            r.IsInternational);
        r.PolicyCompliance = isFlagged ? "Flagged" : "Standard";
        r.Status = "Manager Review";
        r.UpdatedAt = DateTime.UtcNow;

        if (r.ApprovalSteps.Any())
        {
            _travelRequestRepository.RemoveApprovalSteps(r.ApprovalSteps.ToList());
            r.ApprovalSteps.Clear();
        }

        var workflowSteps = await BuildWorkflowStepsAsync(r.Id, r.Employee, cancellationToken);
        var skippedManager = workflowSteps.All(s => s.Level == "Finance");
        r.Status = skippedManager ? "Finance Review" : "Manager Review";

        foreach (var step in workflowSteps)
        {
            await _travelRequestRepository.AddApprovalStepAsync(step, cancellationToken);
        }

        await _auditService.LogAsync(
            entityType: "REQUEST",
            entityId: r.Id,
            action: "Submit Request",
            actorId: r.EmployeeId,
            actorName: r.Employee?.Name ?? "Employee",
            changeDetail: $"Submitted {r.RequestCode} for {(skippedManager ? "Finance" : "Manager")} review (₹{r.EstimatedBudget:N0})",
            severity: isFlagged ? "MEDIUM" : "LOW",
            cancellationToken: cancellationToken);

        await _travelRequestRepository.SaveChangesAsync(cancellationToken);
        return (200, new { success = true, message = $"Request submitted for {(skippedManager ? "Finance" : "Manager")} review." });
    }

    public async Task<(bool Success, string Message, ItinerarySegment? Segment)> AddSegmentAsync(
        Guid id,
        AddSegmentDto dto,
        CancellationToken cancellationToken = default)
    {
        var r = await _travelRequestRepository.GetByIdAsync(id, cancellationToken);
        if (r == null) return (false, "Travel request not found.", null);
        if (!IsOwnerOrAdmin(r.EmployeeId)) return (false, "You can only modify your own travel requests.", null);

        var segment = new ItinerarySegment
        {
            RequestId = id,
            Mode = string.IsNullOrWhiteSpace(dto.Mode) ? "Flight" : dto.Mode,
            Carrier = dto.Carrier ?? "LUFTHANSA LH123",
            Origin = dto.Origin,
            Destination = dto.Destination,
            DepartureTime = dto.DepartureTime == default ? DateTime.UtcNow : dto.DepartureTime,
            ArrivalTime = dto.ArrivalTime,
            ConfirmationNumber = dto.ConfirmationNumber ?? $"#AJK{RandomNumberGenerator.GetInt32(10, 99)}L",
            Cost = dto.Cost
        };

        await _travelRequestRepository.AddSegmentAsync(segment, cancellationToken);
        r.UpdatedAt = DateTime.UtcNow;
        await _travelRequestRepository.SaveChangesAsync(cancellationToken);

        return (true, string.Empty, segment);
    }

    public async Task<(bool Success, string Message, Stay? Stay)> AddStayAsync(
        Guid id,
        AddStayDto dto,
        CancellationToken cancellationToken = default)
    {
        var r = await _travelRequestRepository.GetByIdAsync(id, cancellationToken);
        if (r == null) return (false, "Travel request not found.", null);
        if (!IsOwnerOrAdmin(r.EmployeeId)) return (false, "You can only modify your own travel requests.", null);

        var stay = new Stay
        {
            RequestId = id,
            HotelName = dto.HotelName,
            CheckIn = dto.CheckIn == default ? DateTime.UtcNow : dto.CheckIn,
            CheckOut = dto.CheckOut == default ? DateTime.UtcNow.AddDays(1) : dto.CheckOut,
            Nights = dto.Nights > 0 ? dto.Nights : 1,
            ConfirmationNumber = dto.ConfirmationNumber ?? $"#HY-{RandomNumberGenerator.GetInt32(100000, 999999)}",
            Cost = dto.Cost
        };

        await _travelRequestRepository.AddStayAsync(stay, cancellationToken);
        r.UpdatedAt = DateTime.UtcNow;
        await _travelRequestRepository.SaveChangesAsync(cancellationToken);

        return (true, string.Empty, stay);
    }

    public async Task<(bool Success, string Message)> CloseRequestAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var r = await _travelRequestRepository.GetByIdAsync(id, cancellationToken);
        if (r == null) return (false, "Travel request not found.");

        r.Status = "Completed";
        r.UpdatedAt = DateTime.UtcNow;

        await _auditService.LogAsync(
            entityType: "REQUEST",
            entityId: r.Id,
            action: "Closed Request",
            actorId: null,
            actorName: "User",
            changeDetail: $"Marked {r.RequestCode} as Completed",
            severity: "LOW",
            cancellationToken: cancellationToken);

        await _travelRequestRepository.SaveChangesAsync(cancellationToken);
        return (true, "Trip marked as completed and closed.");
    }

    private async Task<List<ApprovalStep>> BuildWorkflowStepsAsync(
        Guid requestId,
        User? requester,
        CancellationToken cancellationToken)
    {
        var steps = new List<ApprovalStep>();
        var requesterId = requester?.Id ?? Guid.Empty;
        var isManagerOrAbove = string.Equals(requester?.Role, "Manager", StringComparison.OrdinalIgnoreCase)
            || string.Equals(requester?.Role, "Finance", StringComparison.OrdinalIgnoreCase)
            || string.Equals(requester?.Role, "Admin", StringComparison.OrdinalIgnoreCase);

        Guid managerId = Guid.Empty;
        if (requester?.ManagerId.HasValue == true && requester.ManagerId.Value != requesterId)
        {
            managerId = requester.ManagerId.Value;
        }
        else if (!isManagerOrAbove)
        {
            var fallbackManager = await _travelRequestRepository.GetFirstManagerIdAsync(cancellationToken) ?? Guid.Empty;
            if (fallbackManager != requesterId)
            {
                managerId = fallbackManager;
            }
        }

        var financeId = await _travelRequestRepository.GetFirstFinanceIdAsync(cancellationToken);
        if (financeId == requesterId)
        {
            financeId = Guid.Empty;
        }

        if (managerId != Guid.Empty)
        {
            steps.Add(new ApprovalStep
            {
                RequestId = requestId,
                ApproverId = managerId,
                Level = "Manager",
                Status = "Pending"
            });

            if (financeId != Guid.Empty)
            {
                steps.Add(new ApprovalStep
                {
                    RequestId = requestId,
                    ApproverId = financeId,
                    Level = "Finance",
                    Status = "Waiting"
                });
            }
        }
        else if (financeId != Guid.Empty)
        {
            steps.Add(new ApprovalStep
            {
                RequestId = requestId,
                ApproverId = financeId,
                Level = "Finance",
                Status = "Pending"
            });
        }

        return steps;
    }

    private bool IsOwnerOrAdmin(Guid requestOwnerId)
    {
        var callerId = _currentUserService.UserId;
        if (!callerId.HasValue) return true;
        if (string.Equals(_currentUserService.UserRole, "Admin", StringComparison.OrdinalIgnoreCase)) return true;
        return callerId.Value == requestOwnerId;
    }
}
