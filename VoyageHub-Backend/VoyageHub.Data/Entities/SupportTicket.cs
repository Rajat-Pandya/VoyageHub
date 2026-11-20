using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoyageHub.Data.Entities;

[Table("SUPPORT_TICKETS")]
public class SupportTicket
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string TicketCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Priority { get; set; } = "Medium";

    [MaxLength(50)]
    public string? RequestCode { get; set; }

    public Guid? RequestId { get; set; }
    [ForeignKey(nameof(RequestId))]
    public TravelRequest? Request { get; set; }

    [Required]
    [MaxLength(300)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? CallbackNumber { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Open";

    [MaxLength(150)]
    public string? AssignedAgent { get; set; } = "Meera Sen (Corporate Travel Desk)";

    public string? ResolutionNotes { get; set; }

    public Guid? UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
