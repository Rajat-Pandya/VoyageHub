using System;
using System.Collections.Generic;

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
    public DateTime SubmittedDate { get; set; }
    public string PrimaryContact { get; set; } = "Global Support Desk (+1-800-VOYAGE)";

    public List<ItinerarySegmentDto> Segments { get; set; } = new();
    public List<StayDto> Stays { get; set; } = new();
    public List<ExpenseItemDto> Expenses { get; set; } = new();
    public List<ApprovalStepItemDto> ApprovalHistory { get; set; } = new();
}

public class CreateTravelRequestDto
{
    public string Destination { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal EstimatedBudget { get; set; }
    public string TravelClass { get; set; } = "Economy (Standard)";
    public string? Justification { get; set; }
    public bool SaveAsDraft { get; set; } = false;

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
    public Guid RequestId { get; set; }
    public string Category { get; set; } = "Airfare";
    public decimal Amount { get; set; }
    public string? ProofPath { get; set; }
    public string? Description { get; set; }
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
