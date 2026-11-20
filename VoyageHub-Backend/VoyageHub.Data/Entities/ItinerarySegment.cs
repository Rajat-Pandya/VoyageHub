using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoyageHub.Data.Entities;

[Table("ITINERARY_SEGMENTS")]
public class ItinerarySegment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid RequestId { get; set; }
    [ForeignKey(nameof(RequestId))]
    public TravelRequest? Request { get; set; }

    [Required]
    [MaxLength(50)]
    public string Mode { get; set; } = "Flight"; // Flight, Train, Car

    [MaxLength(150)]
    public string? Carrier { get; set; } // e.g. LUFTHANSA LH123

    [Required]
    [MaxLength(100)]
    public string Origin { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Destination { get; set; } = string.Empty;

    public DateTime DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }

    [MaxLength(100)]
    public string? ConfirmationNumber { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Cost { get; set; } = 0.00m;
}

