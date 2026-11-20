using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoyageHub.Data.Entities;

[Table("APPROVAL_STEPS")]
public class ApprovalStep
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid RequestId { get; set; }
    [ForeignKey(nameof(RequestId))]
    public TravelRequest? Request { get; set; }

    public Guid? ApproverId { get; set; }
    [ForeignKey(nameof(ApproverId))]
    public User? Approver { get; set; }

    [Required]
    [MaxLength(50)]
    public string Level { get; set; } = "Manager"; // Manager, Finance

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

    public string? Comment { get; set; }

    public DateTime? DecidedAt { get; set; }
}

