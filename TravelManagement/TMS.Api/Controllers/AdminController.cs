using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMS.Data;

namespace TMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;

        // Static in-memory corporate policy list that can be retrieved or managed
        private static readonly List<PolicyDto> CorporatePolicies = new()
        {
            new PolicyDto
            {
                Id = 1,
                Level = "Executive (L6+)",
                FlightClass = "Business (International) / Economy (Domestic)",
                HotelLimit = "₹12,000 / night",
                PerDiem = "₹3,500 / day",
                AutoApprovalLimit = "₹1,50,000"
            },
            new PolicyDto
            {
                Id = 2,
                Level = "Senior Staff (L4-L5)",
                FlightClass = "Economy Plus",
                HotelLimit = "₹8,000 / night",
                PerDiem = "₹2,500 / day",
                AutoApprovalLimit = "₹50,000"
            },
            new PolicyDto
            {
                Id = 3,
                Level = "Staff & Associate (L1-L3)",
                FlightClass = "Standard Economy",
                HotelLimit = "₹5,500 / night",
                PerDiem = "₹1,800 / day",
                AutoApprovalLimit = "₹25,000"
            }
        };

        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        // GET api/Admin/policies
        [HttpGet("policies")]
        public IActionResult GetPolicies()
        {
            return Ok(CorporatePolicies);
        }

        // GET api/Admin/system-status
        [HttpGet("system-status")]
        public async Task<IActionResult> GetSystemStatus()
        {
            var requestCount = await _db.TravelRequests.CountAsync();
            var lastRequest = await _db.TravelRequests
                .OrderByDescending(r => r.CreatedOn)
                .Select(r => r.CreatedOn)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                Environment = "Production / Enterprise",
                Framework = ".NET 8.0 ASP.NET Core Web API",
                Database = "Microsoft SQL Server Express (localhost\\SQLEXPRESS06)",
                DatabaseName = "TravelDB",
                TotalRequests = requestCount,
                ActivePolicies = CorporatePolicies.Count,
                LastRequestTime = lastRequest != default ? lastRequest.ToString("yyyy-MM-dd HH:mm:ss") : "N/A",
                ServerStatus = "Healthy"
            });
        }
    }

    public class PolicyDto
    {
        public int Id { get; set; }
        public string Level { get; set; } = string.Empty;
        public string FlightClass { get; set; } = string.Empty;
        public string HotelLimit { get; set; } = string.Empty;
        public string PerDiem { get; set; } = string.Empty;
        public string AutoApprovalLimit { get; set; } = string.Empty;
    }
}
