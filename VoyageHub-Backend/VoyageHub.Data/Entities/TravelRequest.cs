using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoyageHub.Data.Entities;

[Table("TRAVEL_REQUESTS")]
public class TravelRequest
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string RequestCode { get; set; } = string.Empty; // e.g. #TR-2048

    [Required]
    public Guid EmployeeId { get; set; }
    [ForeignKey(nameof(EmployeeId))]
    public User? Employee { get; set; }

    [Required]
    [MaxLength(200)]
    public string Destination { get; set; } = string.Empty;

    public bool IsInternational { get; set; } = false;

    [Required]
    [MaxLength(500)]
    public string Purpose { get; set; } = string.Empty;

    [Column(TypeName = "date")]
    public DateTime StartDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime EndDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal EstimatedBudget { get; set; }

    [MaxLength(50)]
    public string TravelClass { get; set; } = "Economy (Standard)";

    public string? Justification { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Draft"; // Draft, In Review, Manager Review, Finance Review, Approved, Completed, Rejected

    [MaxLength(50)]
    public string PolicyCompliance { get; set; } = "Standard"; // Standard, Flagged

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ApprovalStep> ApprovalSteps { get; set; } = new List<ApprovalStep>();
    public ICollection<ItinerarySegment> ItinerarySegments { get; set; } = new List<ItinerarySegment>();
    public ICollection<Stay> Stays { get; set; } = new List<Stay>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}

