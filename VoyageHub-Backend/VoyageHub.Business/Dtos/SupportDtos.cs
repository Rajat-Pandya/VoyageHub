using System;

namespace VoyageHub.Business.Dtos;

public class SupportTicketDto
{
    public Guid Id { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string? RequestCode { get; set; }
    public Guid? RequestId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CallbackNumber { get; set; }
    public string Status { get; set; } = "Open";
    public string? AssignedAgent { get; set; }
    public string? ResolutionNotes { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateSupportTicketDto
{
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string? RequestCode { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CallbackNumber { get; set; }
    public Guid? UserId { get; set; }
}

public class UpdateTicketStatusDto
{
    public string Status { get; set; } = "In Progress";
    public string? ResolutionNotes { get; set; }
    public string? AssignedAgent { get; set; }
}
