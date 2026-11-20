using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoyageHub.Data.Entities;

[Table("AUDIT_LOGS")]
public class AuditLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = "REQUEST"; // POLICY, APPROVAL, ACCESS, REQUEST

    public Guid? EntityId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    public Guid? ActorId { get; set; }
    [ForeignKey(nameof(ActorId))]
    public User? Actor { get; set; }

    [Required]
    [MaxLength(150)]
    public string ActorName { get; set; } = string.Empty;

    [Required]
    public string ChangeDetail { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Severity { get; set; } = "LOW"; // LOW, MEDIUM, HIGH

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
