using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoyageHub.Data.Entities;

[Table("TRAVEL_POLICIES")]
public class TravelPolicy
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid DepartmentId { get; set; }
    [ForeignKey(nameof(DepartmentId))]
    public Department? Department { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MaxBudget { get; set; } = 3000.00m;

    [Column(TypeName = "decimal(18,2)")]
    public decimal IntlBudgetLimit { get; set; } = 3000.00m;

    [Column(TypeName = "decimal(18,2)")]
    public decimal DomesticLimit { get; set; } = 800.00m;

    [Required]
    [MaxLength(50)]
    public string AllowedClass { get; set; } = "ECONOMY+";

    public bool HardEnforcement { get; set; } = true;
}

