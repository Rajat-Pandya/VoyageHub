using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMS.Data.Entity
{
    // An Entity = one table in the database. Each property = one column.
    // EF Core reads this class and creates a table called "TravelRequests" (the DbSet name).
    public class TravelRequest
    {
        [Key] // Primary key. Because it is an int, SQL Server makes it IDENTITY (auto-increment).
        public int TravelRequestId { get; set; }

        [Required, MaxLength(100)] // NOT NULL, nvarchar(100)
        public string EmployeeName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Destination { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Required, MaxLength(500)]
        public string Purpose { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")] // Money: 18 digits, 2 after the decimal point
        public decimal EstimatedBudget { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Draft"; // Draft -> Submitted (later: Approved / Rejected)

        public DateTime CreatedOn { get; set; }
    }
}
