using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Services;
using VoyageHub.Data.Entities;
using VoyageHub.Data.Repositories;
using Xunit;

namespace VoyageHub.Tests;

public class ExpenseServiceTests
{
    private readonly Mock<IExpenseRepository> _expenseRepoMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly ExpenseService _expenseService;

    public ExpenseServiceTests()
    {
        _expenseRepoMock = new Mock<IExpenseRepository>();
        _auditServiceMock = new Mock<IAuditService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _currentUserServiceMock.Setup(c => c.UserId).Returns(Guid.NewGuid());
        _currentUserServiceMock.Setup(c => c.UserName).Returns("Test User");
        _currentUserServiceMock.Setup(c => c.UserRole).Returns("Finance");

        _expenseService = new ExpenseService(
            _expenseRepoMock.Object,
            _auditServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task GetPendingSummaryAsync_ReturnsAggregatedData()
    {
        var requestId = Guid.NewGuid();
        var expenses = new List<Expense>
        {
            new()
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                Category = "Meals",
                Amount = 1200m,
                Status = "Pending",
                Request = new TravelRequest
                {
                    Id = requestId,
                    RequestCode = "#TR-1001",
                    Purpose = "Client Visit",
                    Employee = new User
                    {
                        Name = "Alice",
                        Department = new Department { Name = "Sales" }
                    }
                }
            }
        };

        _expenseRepoMock
            .Setup(r => r.GetPendingExpensesAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expenses);
        _expenseRepoMock
            .Setup(r => r.GetTotalPendingAmountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1200m);

        var result = await _expenseService.GetPendingSummaryAsync(null, null, CancellationToken.None);

        Assert.Equal(1200m, result.TotalPendingAmount);
        Assert.Single(result.PendingClaims);
        Assert.Equal("Alice", result.PendingClaims[0].EmployeeName);
        Assert.Equal("#TR-1001", result.PendingClaims[0].RequestCode);
    }

    [Fact]
    public async Task SubmitExpenseAsync_NullDto_ReturnsFalse()
    {
        var (success, message, expense) = await _expenseService.SubmitExpenseAsync(null!);

        Assert.False(success);
        Assert.Equal("Invalid expense data.", message);
        Assert.Null(expense);
    }

    [Fact]
    public async Task SubmitExpenseAsync_ZeroOrNegativeAmount_ReturnsFalse()
    {
        var dto = new SubmitExpenseDto { RequestId = Guid.NewGuid(), Amount = 0 };

        var (success, message, expense) = await _expenseService.SubmitExpenseAsync(dto);

        Assert.False(success);
        Assert.Equal("Expense amount must be greater than zero.", message);
        Assert.Null(expense);
    }

    [Fact]
    public async Task SubmitExpenseAsync_RequestNotFound_ReturnsFalse()
    {
        var requestId = Guid.NewGuid();
        var dto = new SubmitExpenseDto { RequestId = requestId, Amount = 1500m };

        _expenseRepoMock
            .Setup(r => r.FindTravelRequestWithEmployeeAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TravelRequest?)null);

        var (success, message, expense) = await _expenseService.SubmitExpenseAsync(dto);

        Assert.False(success);
        Assert.Equal("Travel request not found.", message);
        Assert.Null(expense);
    }

    [Fact]
    public async Task SubmitExpenseAsync_Valid_CreatesExpenseAndLogsAudit()
    {
        var requestId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var travelRequest = new TravelRequest
        {
            Id = requestId,
            RequestCode = "#TR-2000",
            EmployeeId = employeeId,
            Employee = new User { Name = "Bob" }
        };

        _expenseRepoMock
            .Setup(r => r.FindTravelRequestWithEmployeeAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(travelRequest);

        var dto = new SubmitExpenseDto
        {
            RequestId = requestId,
            Amount = 3500m,
            Category = "Lodging",
            ProofPath = "receipt_hotel.pdf"
        };

        var (success, message, created) = await _expenseService.SubmitExpenseAsync(dto);

        Assert.True(success);
        Assert.NotNull(created);
        Assert.Equal("Lodging", created.Category);
        Assert.Equal(3500m, created.Amount);
        Assert.Equal("receipt_hotel.pdf", created.ProofPath);

        _expenseRepoMock.Verify(r => r.AddAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()), Times.Once);
        _expenseRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _auditServiceMock.Verify(a => a.LogAsync(
            "EXPENSE",
            created.Id,
            "Submit Expense",
            It.IsAny<Guid?>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            "LOW",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TakeActionAsync_NotFound_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        _expenseRepoMock
            .Setup(r => r.GetByIdWithRequestAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expense?)null);

        var (success, message) = await _expenseService.TakeActionAsync(id, new ExpenseActionDto { Action = "Approve" });

        Assert.False(success);
        Assert.Equal("Expense claim not found.", message);
    }

    [Fact]
    public async Task TakeActionAsync_Approve_UpdatesStatusAndLogsAudit()
    {
        var id = Guid.NewGuid();
        var expense = new Expense
        {
            Id = id,
            Amount = 2000m,
            Category = "Cab",
            Status = "Pending",
            Request = new TravelRequest { RequestCode = "#TR-3000" }
        };

        _expenseRepoMock
            .Setup(r => r.GetByIdWithRequestAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);

        var (success, message) = await _expenseService.TakeActionAsync(id, new ExpenseActionDto { Action = "Approve" });

        Assert.True(success);
        Assert.Equal("Reimbursed", expense.Status);
        Assert.Contains("reimbursed", message);

        _expenseRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TakeActionAsync_Reject_UpdatesStatusToRejected()
    {
        var id = Guid.NewGuid();
        var expense = new Expense
        {
            Id = id,
            Amount = 5000m,
            Category = "Dinner",
            Status = "Pending"
        };

        _expenseRepoMock
            .Setup(r => r.GetByIdWithRequestAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);

        var (success, message) = await _expenseService.TakeActionAsync(id, new ExpenseActionDto { Action = "Reject" });

        Assert.True(success);
        Assert.Equal("Rejected", expense.Status);
        Assert.Contains("rejected", message);
    }

    [Fact]
    public async Task BatchPayoutAsync_UpdatesAllClaimsToReimbursed()
    {
        var claims = new List<Expense>
        {
            new() { Id = Guid.NewGuid(), Amount = 1000m, Status = "Pending" },
            new() { Id = Guid.NewGuid(), Amount = 2000m, Status = "Pending" }
        };

        _expenseRepoMock
            .Setup(r => r.GetPendingExpensesByIdsAsync(It.IsAny<List<Guid>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(claims);

        var (success, message) = await _expenseService.BatchPayoutAsync(new BatchPayoutDto { ExpenseIds = new List<Guid>() });

        Assert.True(success);
        Assert.All(claims, c => Assert.Equal("Reimbursed", c.Status));
        Assert.Contains("2 claims", message);
        _expenseRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AttachProofAsync_NotFound_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        _expenseRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expense?)null);

        var (success, message, proofPath) = await _expenseService.AttachProofAsync(id, "path.pdf");

        Assert.False(success);
        Assert.Null(proofPath);
    }

    [Fact]
    public async Task AttachProofAsync_Found_UpdatesProofPathAndLogsAudit()
    {
        var id = Guid.NewGuid();
        var expense = new Expense { Id = id, Status = "Pending" };

        _expenseRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);

        var (success, message, proofPath) = await _expenseService.AttachProofAsync(id, "receipts/hotel_123.pdf");

        Assert.True(success);
        Assert.Equal("receipts/hotel_123.pdf", proofPath);
        Assert.Equal("receipts/hotel_123.pdf", expense.ProofPath);

        _expenseRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _auditServiceMock.Verify(a => a.LogAsync(
            "EXPENSE",
            id,
            "Attach Proof",
            It.IsAny<Guid?>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            "LOW",
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
