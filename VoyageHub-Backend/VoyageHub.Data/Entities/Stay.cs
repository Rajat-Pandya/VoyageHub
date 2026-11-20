using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoyageHub.Data.Entities;

[Table("STAYS")]
public class Stay
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid RequestId { get; set; }
    [ForeignKey(nameof(RequestId))]
    public TravelRequest? Request { get; set; }

    [Required]
    [MaxLength(200)]
    public string HotelName { get; set; } = string.Empty;

    [Column(TypeName = "date")]
    public DateTime CheckIn { get; set; }

    [Column(TypeName = "date")]
    public DateTime CheckOut { get; set; }

    public int Nights { get; set; } = 1;

    [MaxLength(100)]
    public string? ConfirmationNumber { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Cost { get; set; } = 0.00m;
}
