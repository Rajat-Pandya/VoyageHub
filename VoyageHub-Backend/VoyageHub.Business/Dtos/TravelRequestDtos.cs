using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VoyageHub.Business.Dtos;

public class DashboardStatsDto
{
    public int DraftsCount { get; set; }
    public int PendingCount { get; set; }
    public int UpcomingTripsCount { get; set; }
    public decimal TotalReimbursement { get; set; }
}

public class TravelRequestListDto
{
    public Guid Id { get; set; }
    public string RequestCode { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public bool IsInternational { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string DatesFormatted { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime SubmittedDate { get; set; }
    public decimal EstimatedBudget { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // In Review, Approved, Completed, Rejected, Draft, Manager Review
    public string PolicyCompliance { get; set; } = "Standard"; // Standard, Flagged
    public string TravelClass { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
}

public class ItinerarySegmentDto
{
    public Guid Id { get; set; }
    public string Mode { get; set; } = "Flight";
    public string? Carrier { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public string? ConfirmationNumber { get; set; }
    public decimal Cost { get; set; }
}

public class StayDto
{
    public Guid Id { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int Nights { get; set; }
    public string? ConfirmationNumber { get; set; }
    public decimal Cost { get; set; }
}

public class ExpenseItemDto
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public string RequestCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string TripPurpose { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ProofPath { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public decimal ApprovedBudget { get; set; }
    public decimal ActualSpend { get; set; }
    public decimal TripClaimedTotal { get; set; }
    public decimal CorporatePaidAmount { get; set; }
    public bool IsOverBudget { get; set; }
    public string? RejectionReason { get; set; }
    public string OriginalCurrency { get; set; } = "INR";
    public decimal OriginalAmount { get; set; }
    public decimal ExchangeRate { get; set; } = 1m;
    public bool IsPotentialDuplicate { get; set; }
    public string? PolicyWarning { get; set; }
}

public class ReimbursementLineItemDto
{
    public string Kind { get; set; } = "Expense"; // Itinerary, Expense
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty; // Corporate-Paid, Pending, Reimbursed, Rejected
    public DateTime Date { get; set; }
    public string? ProofPath { get; set; }
    public string? RejectionReason { get; set; }
    public string OriginalCurrency { get; set; } = "INR";
    public decimal OriginalAmount { get; set; }
    public decimal ExchangeRate { get; set; } = 1m;
}

public class TripReimbursementReportDto
{
    public Guid RequestId { get; set; }
    public string RequestCode { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string TripStatus { get; set; } = string.Empty;
    public decimal ApprovedBudget { get; set; }
    public decimal TotalTripSpend { get; set; }
    public decimal CorporatePaidAmount { get; set; }
    public decimal ClaimedTotal { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal ReimbursedAmount { get; set; }
    public string ReportStatus { get; set; } = "No Claims";
    public List<ReimbursementLineItemDto> LineItems { get; set; } = new();
}

public class ApprovalStepItemDto
{
    public Guid Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string ApproverName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime? DecidedAt { get; set; }
}

public class TravelRequestDetailDto
{
    public Guid Id { get; set; }
    public string RequestCode { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public bool IsInternational { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TripDurationDays { get; set; }
    public int TripDurationNights { get; set; }
    public decimal EstimatedBudget { get; set; }
    public string TravelClass { get; set; } = string.Empty;
    public string? Justification { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PolicyCompliance { get; set; } = "Standard";
    public List<string> PolicyFlagReasons { get; set; } = new();
    public DateTime SubmittedDate { get; set; }
    public string PrimaryContact { get; set; } = "Global Support Desk (+1-800-VOYAGE)";

    public List<ItinerarySegmentDto> Segments { get; set; } = new();
    public List<StayDto> Stays { get; set; } = new();
    public List<ExpenseItemDto> Expenses { get; set; } = new();
    public List<ApprovalStepItemDto> ApprovalHistory { get; set; } = new();
}

public class CreateTravelRequestDto
{
    [Required(ErrorMessage = "Destination is required.")]
    public string Destination { get; set; } = string.Empty;

    public bool IsInternational { get; set; } = false;

    [Required(ErrorMessage = "Purpose is required.")]
    public string Purpose { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required.")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required.")]
    public DateTime EndDate { get; set; }

    [Range(0.01, 100000000, ErrorMessage = "Estimated budget must be greater than zero.")]
    public decimal EstimatedBudget { get; set; }

    public string TravelClass { get; set; } = "Economy (Standard)";
    public string? Justification { get; set; }
    public bool SaveAsDraft { get; set; } = false;
    public bool SelfPaidItinerary { get; set; } = false;

    public List<CreateSegmentDto> Segments { get; set; } = new();
    public List<CreateStayDto> Stays { get; set; } = new();
}

public class CreateSegmentDto
{
    public string Mode { get; set; } = "Flight";
    public string? Carrier { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public string? ConfirmationNumber { get; set; }
    public decimal Cost { get; set; }
}

public class CreateStayDto
{
    public string HotelName { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int Nights { get; set; } = 1;
    public string? ConfirmationNumber { get; set; }
    public decimal Cost { get; set; }
}

public class SubmitExpenseDto
{
    [Required(ErrorMessage = "Travel request ID is required.")]
    public Guid RequestId { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    public string Category { get; set; } = "Airfare";

    [Range(0.01, 100000000, ErrorMessage = "Expense amount must be greater than zero.")]
    public decimal Amount { get; set; }

    public string? ProofPath { get; set; }
    public string? Description { get; set; }
    public string Currency { get; set; } = "INR";
    public decimal? ExchangeRate { get; set; }
}

public class AddSegmentDto
{
    public string Mode { get; set; } = "Flight";
    public string? Carrier { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public string? ConfirmationNumber { get; set; }
    public decimal Cost { get; set; }
}

public class AddStayDto
{
    public string HotelName { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int Nights { get; set; } = 1;
    public string? ConfirmationNumber { get; set; }
    public decimal Cost { get; set; }
}
