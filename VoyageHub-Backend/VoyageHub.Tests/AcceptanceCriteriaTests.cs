using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Services;
using VoyageHub.Data;
using VoyageHub.Data.Entities;
using VoyageHub.Data.Repositories;
using Xunit;

namespace VoyageHub.Tests;

public class AcceptanceCriteriaTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    private readonly TravelRequestRepository _travelRequestRepository;
    private readonly ExpenseRepository _expenseRepository;
    private readonly SupportTicketRepository _supportTicketRepository;

    private readonly TravelRequestService _travelRequestService;
    private readonly ExpenseService _expenseService;
    private readonly SupportService _supportService;

    public AcceptanceCriteriaTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _auditServiceMock = new Mock<IAuditService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _currentUserServiceMock.Setup(u => u.UserId).Returns(Guid.NewGuid());
        _currentUserServiceMock.Setup(u => u.UserName).Returns("Test User");

        _travelRequestRepository = new TravelRequestRepository(_context);
        _expenseRepository = new ExpenseRepository(_context);
        _supportTicketRepository = new SupportTicketRepository(_context);

        _travelRequestService = new TravelRequestService(
            _travelRequestRepository,
            _auditServiceMock.Object,
            _currentUserServiceMock.Object);

        _expenseService = new ExpenseService(
            _expenseRepository,
            _auditServiceMock.Object,
            _currentUserServiceMock.Object);

        _supportService = new SupportService(
            _supportTicketRepository,
            _auditServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task TravelRequest_Creation_OverBudget_SetsInReview_Flagged_CodeTRFormat_AndCreatesApprovalSteps()
    {
        // Arrange
        var deptId = Guid.NewGuid();
        var dept = new Department
        {
            Id = deptId,
            Name = "Engineering",
            Policy = new TravelPolicy
            {
                Id = Guid.NewGuid(),
                DepartmentId = deptId,
                IntlBudgetLimit = 2000m,
                AllowedClass = "Economy",
                HardEnforcement = true
            }
        };

        var managerId = Guid.NewGuid();
        var manager = new User
        {
            Id = managerId,
            Name = "Manager User",
            Email = "manager@voyagehub.internal",
            PasswordHash = "hash",
            Role = "Manager",
            DepartmentId = deptId,
            CreatedAt = DateTime.UtcNow
        };

        var financeId = Guid.NewGuid();
        var finance = new User
        {
            Id = financeId,
            Name = "Finance User",
            Email = "finance@voyagehub.internal",
            PasswordHash = "hash",
            Role = "Finance",
            DepartmentId = deptId,
            CreatedAt = DateTime.UtcNow
        };

        var empId = Guid.NewGuid();
        var employee = new User
        {
            Id = empId,
            Name = "John Doe",
            Email = "john.doe@voyagehub.internal",
            PasswordHash = "hash",
            Role = "Employee",
            DepartmentId = deptId,
            Department = dept,
            ManagerId = managerId,
            Manager = manager,
            CreatedAt = DateTime.UtcNow
        };

        _context.Departments.Add(dept);
        _context.Users.AddRange(manager, finance, employee);
        await _context.SaveChangesAsync();

        var dto = new CreateTravelRequestDto
        {
            Purpose = "International Tech Conference",
            Destination = "Tokyo, Japan",
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(15),
            EstimatedBudget = 5000m, // Over the 2000 limit
            TravelClass = "Business",
            SaveAsDraft = false
        };

        // Act
        var (success, message, createdDto) = await _travelRequestService.CreateRequestAsync(dto, empId);

        // Assert
        Assert.True(success);
        Assert.NotNull(createdDto);

        var created = await _context.TravelRequests
            .Include(t => t.ApprovalSteps)
            .FirstOrDefaultAsync(t => t.EmployeeId == empId);

        Assert.NotNull(created);
        Assert.Equal("In Review", created.Status);
        Assert.Equal("Flagged", created.PolicyCompliance);
        Assert.StartsWith("#TR-", created.RequestCode);

        // Two approval steps created: Manager and Finance
        Assert.Equal(2, created.ApprovalSteps.Count);
        Assert.Contains(created.ApprovalSteps, s => s.Level == "Manager");
        Assert.Contains(created.ApprovalSteps, s => s.Level == "Finance");
    }

    [Fact]
    public async Task TravelRequest_SubmitDraftAsync_OnlyAllowedForDraftStatus()
    {
        // Arrange
        var empId = Guid.NewGuid();
        var emp = new User
        {
            Id = empId,
            Name = "Jane Smith",
            Email = "jane.smith@voyagehub.internal",
            PasswordHash = "hash",
            Role = "Employee",
            CreatedAt = DateTime.UtcNow
        };

        var nonDraftRequest = new TravelRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = empId,
            Employee = emp,
            RequestCode = "#TR-2051",
            Purpose = "Sales Review",
            Destination = "Bengaluru",
            StartDate = DateTime.UtcNow.AddDays(5),
            EndDate = DateTime.UtcNow.AddDays(7),
            EstimatedBudget = 1500m,
            Status = "Approved", // Not Draft
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(emp);
        _context.TravelRequests.Add(nonDraftRequest);
        await _context.SaveChangesAsync();

        // Act: Attempt to submit a non-draft request
        var (statusCode, message) = await _travelRequestService.SubmitDraftAsync(nonDraftRequest.Id);

        // Assert
        Assert.Equal(400, statusCode);
        Assert.Equal("Only Draft requests can be submitted.", message);
    }

    [Fact]
    public async Task Expense_Approve_And_BatchPayout_SuccessfullyReimburses()
    {
        // Arrange
        var reqId = Guid.NewGuid();
        var empId = Guid.NewGuid();
        var emp = new User
        {
            Id = empId,
            Name = "Expense Claimant",
            Email = "claimant@voyagehub.internal",
            PasswordHash = "hash",
            Role = "Employee",
            CreatedAt = DateTime.UtcNow
        };
        var req = new TravelRequest
        {
            Id = reqId,
            EmployeeId = empId,
            Employee = emp,
            RequestCode = "#TR-2055",
            Purpose = "Trip",
            Destination = "Delhi",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(3),
            EstimatedBudget = 1000m,
            Status = "Approved",
            CreatedAt = DateTime.UtcNow
        };

        var expenseId1 = Guid.NewGuid();
        var expenseId2 = Guid.NewGuid();

        var exp1 = new Expense
        {
            Id = expenseId1,
            RequestId = reqId,
            Category = "Meals",
            Amount = 150m,
            Status = "Submitted"
        };
        var exp2 = new Expense
        {
            Id = expenseId2,
            RequestId = reqId,
            Category = "Transport",
            Amount = 300m,
            Status = "Pending"
        };

        _context.Users.Add(emp);
        _context.TravelRequests.Add(req);
        _context.Expenses.AddRange(exp1, exp2);
        await _context.SaveChangesAsync();

        // Act 1: Single approve action
        var (actionSuccess, actionMsg) = await _expenseService.TakeActionAsync(expenseId1, new ExpenseActionDto
        {
            Action = "Approve"
        });

        // Assert 1
        Assert.True(actionSuccess);
        var refreshedExp1 = await _context.Expenses.FindAsync(expenseId1);
        Assert.Equal("Reimbursed", refreshedExp1!.Status);

        // Act 2: Batch payout on exp2
        var (payoutSuccess, payoutMsg) = await _expenseService.BatchPayoutAsync(new BatchPayoutDto
        {
            ExpenseIds = new List<Guid> { expenseId2 }
        });

        // Assert 2
        Assert.True(payoutSuccess);
        var refreshedExp2 = await _context.Expenses.FindAsync(expenseId2);
        Assert.Equal("Reimbursed", refreshedExp2!.Status);
    }

    [Fact]
    public async Task SupportTicket_CreateTicket_GeneratesCodeInFormatHashTickPlusFourDigits()
    {
        // Arrange
        var dto = new CreateSupportTicketDto
        {
            Subject = "Flight cancellation query",
            Description = "Need help rebooking my flight due to weather delay",
            Category = "Booking",
            Priority = "High"
        };

        // Act
        var (success, errorMsg, ticketDto) = await _supportService.CreateTicketAsync(dto);

        // Assert
        Assert.True(success);
        Assert.NotNull(ticketDto);
        Assert.NotNull(ticketDto.TicketCode);

        // Code format: #TICK- followed by 4 digits (e.g. #TICK-4821)
        Assert.Matches(@"^#TICK-\d{4}$", ticketDto.TicketCode);
    }
}
