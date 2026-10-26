namespace TMS.Api.Dtos
{
    // RESPONSE DTO = the JSON the API SENDS BACK.
    // We choose exactly what the client sees. The entity can change (new internal columns)
    // without changing what the client receives.
    public class TravelRequestResponseDto
    {
        public int TravelRequestId { get; set; }

        public string EmployeeName { get; set; } = string.Empty;

        public string Destination { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int TripDays { get; set; } // Not in the database: calculated by the mapper

        public string Purpose { get; set; } = string.Empty;

        public decimal EstimatedBudget { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }
    }
}
