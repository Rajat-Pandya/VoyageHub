using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Business.Dtos;

namespace VoyageHub.Business.Services;

public class AnalyticsService : IAnalyticsService
{
    public Task<AnalyticsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var overview = new AnalyticsOverviewDto
        {
            TotalSpent = 1845000.00m, // ₹18,45,000 INR
            VsLastPeriodPercent = -12.5,
            MonthlySpend = new List<MonthlySpendDto>
            {
                new MonthlySpendDto { Month = "Jan", Amount = 280000.00m },
                new MonthlySpendDto { Month = "Feb", Amount = 320000.00m },
                new MonthlySpendDto { Month = "Mar", Amount = 260000.00m },
                new MonthlySpendDto { Month = "Apr", Amount = 390000.00m },
                new MonthlySpendDto { Month = "May", Amount = 315000.00m },
                new MonthlySpendDto { Month = "Jun", Amount = 280000.00m },
            },
            DepartmentSpend = new List<DepartmentSpendDto>
            {
                new DepartmentSpendDto { Department = "Sales & Marketing (India)", Trips = 42, AvgCost = 28500.00m, Total = 1197000.00m },
                new DepartmentSpendDto { Department = "Engineering & IT", Trips = 18, AvgCost = 24000.00m, Total = 432000.00m },
                new DepartmentSpendDto { Department = "Human Resources & Admin", Trips = 8, AvgCost = 16500.00m, Total = 132000.00m },
                new DepartmentSpendDto { Department = "Finance & Tax Operations", Trips = 4, AvgCost = 21000.00m, Total = 84000.00m }
            },
            CategorySpend = new List<CategorySpendDto>
            {
                new CategorySpendDto { Category = "Airfare", Percentage = 52, Amount = 959400.00m, Color = "#4f46e5" },
                new CategorySpendDto { Category = "Hotel", Percentage = 28, Amount = 516600.00m, Color = "#f59e0b" },
                new CategorySpendDto { Category = "Meals", Percentage = 12, Amount = 221400.00m, Color = "#10b981" },
                new CategorySpendDto { Category = "Transport", Percentage = 8, Amount = 147600.00m, Color = "#ec4899" }
            }
        };

        return Task.FromResult(overview);
    }
}
