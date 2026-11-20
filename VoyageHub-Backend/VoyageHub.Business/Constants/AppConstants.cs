namespace VoyageHub.Business.Constants;

public static class TravelRequestStatus
{
    public const string Draft = "Draft";
    public const string InReview = "In Review";
    public const string ManagerReview = "Manager Review";
    public const string FinanceReview = "Finance Review";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}

public static class UserRoles
{
    public const string Employee = "Employee";
    public const string Manager = "Manager";
    public const string Finance = "Finance";
    public const string Admin = "Admin";
}

public static class PolicyComplianceStatus
{
    public const string Standard = "Standard";
    public const string Flagged = "Flagged";
}

public static class ApprovalStepStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
}

public static class ApprovalLevel
{
    public const string Manager = "Manager";
    public const string Finance = "Finance";
}

public static class ExpenseStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Reimbursed = "Reimbursed";
}
