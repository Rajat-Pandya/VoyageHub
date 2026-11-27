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
                ProofPath = exp.ProofPath,
                RejectionReason = exp.RejectionReason,
                OriginalCurrency = string.IsNullOrWhiteSpace(exp.OriginalCurrency) ? "INR" : exp.OriginalCurrency,
                OriginalAmount = exp.OriginalAmount > 0 ? exp.OriginalAmount : exp.Amount,
                ExchangeRate = exp.ExchangeRate > 0 ? exp.ExchangeRate : 1m
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

    private static readonly Dictionary<string, decimal> DefaultFxRatesToInr = new(StringComparer.OrdinalIgnoreCase)
    {
        ["INR"] = 1.00m,
        ["USD"] = 84.50m,
        ["EUR"] = 91.20m,
        ["GBP"] = 107.40m,
        ["SGD"] = 63.10m,
        ["AED"] = 23.00m,
        ["NZD"] = 50.40m,
        ["JPY"] = 0.56m
    };

    private static (string Currency, decimal Rate, decimal InrAmount) ConvertToInr(decimal rawAmount, string? currency, decimal? overrideRate)
    {
        var code = string.IsNullOrWhiteSpace(currency) ? "INR" : currency.Trim().ToUpperInvariant();
        if (code == "INR")
        {
            return ("INR", 1m, Math.Round(rawAmount, 2));
        }

        var rate = overrideRate.HasValue && overrideRate.Value > 0
            ? overrideRate.Value
            : (DefaultFxRatesToInr.TryGetValue(code, out var known) ? known : 1m);

        return (code, rate, Math.Round(rawAmount * rate, 2));
    }

    private static ExpenseItemDto MapToExpenseItemDto(Expense e)
    {
        var req = e.Request;
        var approvedBudget = req?.EstimatedBudget ?? 0m;
        decimal actualSpend;
        decimal claimedTotal;
        decimal corporatePaid;
        var isPotentialDuplicate = false;
        string? policyWarning = null;

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

            isPotentialDuplicate = activeExpenses.Any(x =>
                x.Id != e.Id &&
                string.Equals(x.Category, e.Category, StringComparison.OrdinalIgnoreCase) &&
                x.Amount == e.Amount);

            var tripDays = Math.Max(1, (req.EndDate - req.StartDate).Days + 1);
            if (string.Equals(e.Category, "Meals", StringComparison.OrdinalIgnoreCase) && e.Amount > 2500m * tripDays)
            {
                policyWarning = $"Meals exceed ₹2,500/day guideline (₹{2500m * tripDays:N0} for {tripDays}d).";
            }
            else if (string.Equals(e.Category, "Transport", StringComparison.OrdinalIgnoreCase) && e.Amount > 5000m)
            {
                policyWarning = "Local transport exceeds ₹5,000 guideline.";
            }
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
            IsOverBudget = approvedBudget > 0 && actualSpend > approvedBudget,
            RejectionReason = e.RejectionReason,
            OriginalCurrency = string.IsNullOrWhiteSpace(e.OriginalCurrency) ? "INR" : e.OriginalCurrency,
            OriginalAmount = e.OriginalAmount > 0 ? e.OriginalAmount : e.Amount,
            ExchangeRate = e.ExchangeRate > 0 ? e.ExchangeRate : 1m,
            IsPotentialDuplicate = isPotentialDuplicate,
            PolicyWarning = policyWarning
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

        var (origCurrency, fxRate, inrAmount) = ConvertToInr(dto.Amount, dto.Currency, dto.ExchangeRate);

        var expense = new Expense
        {
            RequestId = dto.RequestId,
            Category = string.IsNullOrWhiteSpace(dto.Category) ? "Airfare" : dto.Category,
            Amount = inrAmount,
            OriginalCurrency = origCurrency,
            OriginalAmount = Math.Round(dto.Amount, 2),
            ExchangeRate = fxRate,
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
        expense.RejectionReason = isApprove
            ? null
            : (string.IsNullOrWhiteSpace(dto.Comment) ? "Rejected by Finance" : dto.Comment.Trim());

        await _auditService.LogAsync(
            entityType: "EXPENSE",
            entityId: expense.Id,
            action: isApprove ? "Reimbursed Claim" : "Rejected Claim",
            actorId: _currentUserService.UserId,
            actorName: _currentUserService.UserName ?? "Finance Officer",
            changeDetail: $"Processed ₹{expense.Amount:N0} {expense.Category} claim for {expense.Request?.RequestCode} ({expense.Status})" +
                          (isApprove || string.IsNullOrWhiteSpace(expense.RejectionReason) ? "" : $" — {expense.RejectionReason}"),
            severity: "LOW",
            cancellationToken: cancellationToken);

        await _expenseRepository.SaveChangesAsync(cancellationToken);

        return (true, $"Expense claim {expense.Status.ToLower()}.");
    }

    public async Task<(bool Success, string Message, Expense? Expense)> ResubmitExpenseAsync(Guid id, SubmitExpenseDto? dto, CancellationToken cancellationToken = default)
    {
        var expense = await _expenseRepository.GetByIdWithRequestAsync(id, cancellationToken);
        if (expense == null)
        {
            return (false, "Expense claim not found.", null);
        }

        if (expense.Request != null && !IsOwnerOrAdmin(expense.Request.EmployeeId))
        {
            return (false, "You can only resubmit your own expense claims.", null);
        }

        if (!string.Equals(expense.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Only rejected expense claims can be resubmitted.", null);
        }

        if (expense.Request != null && !string.Equals(expense.Request.Status, "Approved", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Expenses can only be resubmitted for Approved travel requests.", null);
        }

        if (dto != null)
        {
            if (dto.Amount > 0)
            {
                var (origCurrency, fxRate, inrAmount) = ConvertToInr(dto.Amount, dto.Currency, dto.ExchangeRate);
                expense.Amount = inrAmount;
                expense.OriginalCurrency = origCurrency;
                expense.OriginalAmount = Math.Round(dto.Amount, 2);
                expense.ExchangeRate = fxRate;
            }
            if (!string.IsNullOrWhiteSpace(dto.Category))
            {
                expense.Category = dto.Category.Trim();
            }
            if (!string.IsNullOrWhiteSpace(dto.ProofPath))
            {
                expense.ProofPath = dto.ProofPath.Trim();
            }
        }

        expense.Status = "Pending";
        expense.RejectionReason = null;
        expense.SubmittedAt = DateTime.UtcNow;

        await _auditService.LogAsync(
            entityType: "EXPENSE",
            entityId: expense.Id,
            action: "Resubmit Expense",
            actorId: _currentUserService.UserId ?? expense.Request?.EmployeeId,
            actorName: _currentUserService.UserName ?? "Employee",
            changeDetail: $"Resubmitted ₹{expense.Amount:N0} {expense.Category} claim for {expense.Request?.RequestCode}",
            severity: "LOW",
            cancellationToken: cancellationToken);

        await _expenseRepository.SaveChangesAsync(cancellationToken);
        return (true, "Expense claim resubmitted for Finance review.", expense);
    }

    public async Task<string> ExportPayoutCsvAsync(string? status, CancellationToken cancellationToken = default)
    {
        var normalizedStatus = string.IsNullOrWhiteSpace(status) ? "Reimbursed" : status.Trim();
        var items = await _expenseRepository.GetExpensesByFilterAsync(normalizedStatus, null, null, cancellationToken);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("RequestCode,EmployeeName,Department,Category,OriginalCurrency,OriginalAmount,ExchangeRate,AmountINR,Status,SubmittedAt");

        foreach (var e in items)
        {
            var reqCode = EscapeCsv(e.Request?.RequestCode ?? "");
            var emp = EscapeCsv(e.Request?.Employee?.Name ?? "");
            var dept = EscapeCsv(e.Request?.Employee?.Department?.Name ?? "");
            var cat = EscapeCsv(e.Category);
            var origCurr = EscapeCsv(string.IsNullOrWhiteSpace(e.OriginalCurrency) ? "INR" : e.OriginalCurrency);
            var origAmt = (e.OriginalAmount > 0 ? e.OriginalAmount : e.Amount).ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            var rate = (e.ExchangeRate > 0 ? e.ExchangeRate : 1m).ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            var amtInr = e.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            var st = EscapeCsv(e.Status);
            var dt = e.SubmittedAt.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            sb.AppendLine($"{reqCode},{emp},{dept},{cat},{origCurr},{origAmt},{rate},{amtInr},{st},{dt}");
        }

        return sb.ToString();
    }

    private static string EscapeCsv(string val)
    {
        if (val.Contains(',') || val.Contains('"') || val.Contains('\n'))
        {
            return $"\"{val.Replace("\"", "\"\"")}\"";
        }
        return val;
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

    public async Task<Expense?> GetExpenseByProofFileNameAsync(string fileName, CancellationToken cancellationToken = default)
    {
        return await _expenseRepository.GetByProofFileNameAsync(fileName, cancellationToken);
    }

    private bool IsOwnerOrAdmin(Guid requestOwnerId)
    {
        var callerId = _currentUserService.UserId;
        if (!callerId.HasValue) return true;
        if (string.Equals(_currentUserService.UserRole, "Admin", StringComparison.OrdinalIgnoreCase)) return true;
        return callerId.Value == requestOwnerId;
    }
}
