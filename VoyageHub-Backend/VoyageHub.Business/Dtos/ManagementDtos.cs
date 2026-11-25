using System;
using System.Collections.Generic;

namespace VoyageHub.Business.Dtos;

// Page 6: Manager Approval Queue
public class ApprovalQueueSummaryDto
{
    public decimal PendingTotal { get; set; }
    public double AvgResponseTimeHours { get; set; }
    public int PolicyCompliancePercent { get; set; }
    public List<ApprovalQueueItemDto> Items { get; set; } = new();
}

public class ApprovalQueueItemDto
{
    public Guid StepId { get; set; }
    public Guid RequestId { get; set; }
    public string RequestCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string TravelDates { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public string PolicyStatus { get; set; } = "Standard"; // Standard, Flagged
    public List<string> PolicyFlagReasons { get; set; } = new();
    public string Status { get; set; } = "Pending";
    public string Level { get; set; } = "Manager"; // Manager, Finance
}

public class ApprovalActionDto
{
    public string Action { get; set; } = "Approve"; // Approve, Reject
    public string? Comment { get; set; }
}

// Page 7: Finance Reimbursement Review
public class ExpenseReviewSummaryDto
{
    public decimal TotalPendingAmount { get; set; }
    public List<ExpenseItemDto> PendingClaims { get; set; } = new();
}

public class ExpenseActionDto
{
    public string Action { get; set; } = "Approve"; // Approve, Reject
}

public class BatchPayoutDto
{
    public List<Guid> ExpenseIds { get; set; } = new();
}

// Page 8: Analytics
public class AnalyticsOverviewDto
{
    public decimal TotalSpent { get; set; }
    public double VsLastPeriodPercent { get; set; }
    public List<MonthlySpendDto> MonthlySpend { get; set; } = new();
    public List<DepartmentSpendDto> DepartmentSpend { get; set; } = new();
    public List<CategorySpendDto> CategorySpend { get; set; } = new();
}

public class MonthlySpendDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class DepartmentSpendDto
{
    public string Department { get; set; } = string.Empty;
    public int Trips { get; set; }
    public decimal AvgCost { get; set; }
    public decimal Total { get; set; }
}

public class CategorySpendDto
{
    public string Category { get; set; } = string.Empty;
    public int Percentage { get; set; }
    public decimal Amount { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class EmployeeSpendReportDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int TripsCount { get; set; }
    public decimal TotalApprovedBudget { get; set; }
    public decimal TotalReimbursedExpenses { get; set; }
}

public class CostSummaryReportDto
{
    public int TotalTrips { get; set; }
    public decimal TotalCommittedBudget { get; set; }
    public decimal TotalReimbursed { get; set; }
    public decimal TotalPendingReimbursement { get; set; }
    public decimal AverageCostPerTrip { get; set; }
}

// Page 9: Policy & Hierarchy Config
public class PolicyConfigDto
{
    public Guid Id { get; set; }
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public decimal IntlBudgetLimit { get; set; }
    public decimal DomesticLimit { get; set; }
    public string AllowedClass { get; set; } = string.Empty;
    public bool HardEnforcement { get; set; }
}

public class UpdatePolicyDto
{
    public decimal IntlBudgetLimit { get; set; }
    public decimal DomesticLimit { get; set; }
    public string AllowedClass { get; set; } = "ECONOMY+";
    public bool HardEnforcement { get; set; } = true;
}

public class CreatePolicyDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public decimal IntlBudgetLimit { get; set; } = 250000;
    public decimal DomesticLimit { get; set; } = 40000;
    public string AllowedClass { get; set; } = "ECONOMY+";
    public bool HardEnforcement { get; set; } = true;
}

// Page 10: Audit Log
public class AuditLogDto
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string ChangeDetail { get; set; } = string.Empty;
    public string Severity { get; set; } = "LOW"; // LOW, MEDIUM, HIGH
}
