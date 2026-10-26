using System.ComponentModel.DataAnnotations;

namespace TMS.Api.Dtos
{
    // INPUT DTO = the JSON the client SENDS (used by both Create and Update).
    // Only fields a user is allowed to type. No Id, Status or CreatedOn.
    // [ApiController] checks these attributes automatically and returns 400 if any fail.
    public class TravelRequestInputDto
    {
        [Required, MaxLength(100)]
        public string EmployeeName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Destination { get; set; } = string.Empty;

        // DateTime? (nullable) so [Required] can detect a missing value.
        // A plain DateTime would silently default to 0001-01-01 and pass validation.
        [Required]
        public DateTime? StartDate { get; set; }

        [Required]
        public DateTime? EndDate { get; set; }

        [Required, MaxLength(500)]
        public string Purpose { get; set; } = string.Empty;

        [Range(1, 10000000)]
        public decimal EstimatedBudget { get; set; }
    }
}
