using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Services;
using VoyageHub.Data.Entities;
using VoyageHub.Data.Repositories;
using Xunit;

namespace VoyageHub.Tests;

public class TravelRequestValidationTests
{
    private readonly Mock<ITravelRequestRepository> _travelRepoMock;
    private readonly Mock<IExpenseRepository> _expenseRepoMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly TravelRequestService _travelRequestService;
    private readonly ExpenseService _expenseService;

    public TravelRequestValidationTests()
    {
        _travelRepoMock = new Mock<ITravelRequestRepository>();
        _expenseRepoMock = new Mock<IExpenseRepository>();
        _auditServiceMock = new Mock<IAuditService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _currentUserServiceMock.Setup(u => u.UserId).Returns(Guid.NewGuid());
        _currentUserServiceMock.Setup(u => u.UserName).Returns("Test User");

        _travelRequestService = new TravelRequestService(
            _travelRepoMock.Object,
            _auditServiceMock.Object,
            _currentUserServiceMock.Object);

        _expenseService = new ExpenseService(
            _expenseRepoMock.Object,
            _auditServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task CreateRequestAsync_WhenStartDateAfterEndDate_ReturnsValidationError()
    {
        // Arrange
        var dto = new CreateTravelRequestDto
        {
            Destination = "Tokyo, Japan",
            Purpose = "Client QBR",
            StartDate = new DateTime(2026, 12, 10),
            EndDate = new DateTime(2026, 12, 5), // Invalid: before start date
            EstimatedBudget = 150000m
        };

        // Act
        var (success, message, request) = await _travelRequestService.CreateRequestAsync(dto, Guid.NewGuid());

        // Assert
        Assert.False(success);
        Assert.Equal("Travel start date cannot be after end date.", message);
        Assert.Null(request);
    }

    [Fact]
    public async Task CreateRequestAsync_WhenEstimatedBudgetZeroOrNegative_ReturnsValidationError()
    {
        // Arrange
        var dto = new CreateTravelRequestDto
        {
            Destination = "Dubai, UAE",
            Purpose = "Tech Summit",
            StartDate = new DateTime(2026, 11, 25),
            EndDate = new DateTime(2026, 11, 30),
            EstimatedBudget = -500m // Invalid: negative
        };

        // Act
        var (success, message, request) = await _travelRequestService.CreateRequestAsync(dto, Guid.NewGuid());

        // Assert
        Assert.False(success);
        Assert.Equal("Estimated budget must be greater than zero.", message);
        Assert.Null(request);
    }

    [Fact]
    public async Task CreateRequestAsync_WhenBudgetExceedsPolicyLimit_AutoFlagsCompliance()
    {
        // Arrange
        var empId = Guid.NewGuid();
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Sales",
            Policy = new TravelPolicy
            {
                Id = Guid.NewGuid(),
                IntlBudgetLimit = 50000m // Policy limit 50,000
            }
        };

        var employee = new User
        {
            Id = empId,
            Name = "John Doe",
            DepartmentId = department.Id,
            Department = department,
            ManagerId = Guid.NewGuid()
        };

        _travelRepoMock
            .Setup(r => r.GetEmployeeWithDepartmentAndManagerAsync(empId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _travelRepoMock
            .Setup(r => r.GetCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        var dto = new CreateTravelRequestDto
        {
            Destination = "London, UK",
            Purpose = "Enterprise Pitch",
            StartDate = new DateTime(2026, 12, 1),
            EndDate = new DateTime(2026, 12, 6),
            EstimatedBudget = 75000m, // Exceeds 50,000 limit!
            SaveAsDraft = false
        };

        // Act
        var (success, message, request) = await _travelRequestService.CreateRequestAsync(dto, empId);

        // Assert
        Assert.True(success);
        Assert.NotNull(request);
        Assert.Equal("In Review", request!.Status);
        _travelRepoMock.Verify(r => r.AddAsync(It.Is<TravelRequest>(tr => tr.PolicyCompliance == "Flagged"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitExpenseAsync_WhenAmountZeroOrNegative_ReturnsValidationError()
    {
        // Arrange
        var dto = new SubmitExpenseDto
        {
            RequestId = Guid.NewGuid(),
            Category = "Meals",
            Amount = -150m // Invalid negative amount
        };

        // Act
        var (success, message, expense) = await _expenseService.SubmitExpenseAsync(dto);

        // Assert
        Assert.False(success);
        Assert.Equal("Expense amount must be greater than zero.", message);
        Assert.Null(expense);
        _expenseRepoMock.Verify(r => r.AddAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
