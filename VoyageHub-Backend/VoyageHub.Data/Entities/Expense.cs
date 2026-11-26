using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoyageHub.Data.Entities;

[Table("EXPENSES")]
public class Expense
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid RequestId { get; set; }
    [ForeignKey(nameof(RequestId))]
    public TravelRequest? Request { get; set; }

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = "Airfare"; // Airfare, Hotel, Meals, Transport

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string? ProofPath { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Reimbursed, Rejected

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    [MaxLength(10)]
    public string OriginalCurrency { get; set; } = "INR";

    [Column(TypeName = "decimal(18,2)")]
    public decimal OriginalAmount { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal ExchangeRate { get; set; } = 1m;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
