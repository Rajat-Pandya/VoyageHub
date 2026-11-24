using System;
using System.IO;
using System.Text;
using System.Threading;
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

public class EmployeeServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly UserRepository _userRepository;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly EmployeeService _employeeService;

    public EmployeeServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _userRepository = new UserRepository(_context);
        _auditServiceMock = new Mock<IAuditService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        // Default current user: an Admin
        var adminId = Guid.NewGuid();
        _currentUserServiceMock.Setup(c => c.UserId).Returns(adminId);
        _currentUserServiceMock.Setup(c => c.UserRole).Returns("Admin");
        _currentUserServiceMock.Setup(c => c.UserName).Returns("Admin User");

        _employeeService = new EmployeeService(
            _context,
            _userRepository,
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
    public async Task CreateEmployeeAsync_ValidInput_CreatesEmployeeAndDepartment()
    {
        var dto = new CreateEmployeeDto
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Role = "Employee",
            DepartmentName = "Sales Operations",
            Password = "SecurePassword123!"
        };

        var (success, message, created) = await _employeeService.CreateEmployeeAsync(dto);

        Assert.True(success);
        Assert.NotNull(created);
        Assert.Equal("John Doe", created.Name);
        Assert.Equal("john.doe@example.com", created.Email);
        Assert.Equal("Sales Operations", created.DepartmentName);

        // Verify persisted in DB
        var userInDb = await _context.Users.Include(u => u.Department).FirstOrDefaultAsync(u => u.Email == "john.doe@example.com");
        Assert.NotNull(userInDb);
        Assert.Equal("Sales Operations", userInDb.Department?.Name);
    }

    [Fact]
    public async Task CreateEmployeeAsync_EmptyName_ReturnsFailure()
    {
        var dto = new CreateEmployeeDto
        {
            Name = "   ",
            Email = "invalid@example.com"
        };

        var (success, message, created) = await _employeeService.CreateEmployeeAsync(dto);

        Assert.False(success);
        Assert.Equal("Full Name is required.", message);
        Assert.Null(created);
    }

    [Fact]
    public async Task CreateEmployeeAsync_InvalidEmail_ReturnsFailure()
    {
        var dto = new CreateEmployeeDto
        {
            Name = "Valid Name",
            Email = "invalid-email-without-at"
        };

        var (success, message, created) = await _employeeService.CreateEmployeeAsync(dto);

        Assert.False(success);
        Assert.Equal("A valid email address is required.", message);
        Assert.Null(created);
    }

    [Fact]
    public async Task CreateEmployeeAsync_DuplicateEmail_ReturnsFailure()
    {
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "Existing",
            Email = "duplicate@example.com",
            Role = "Employee",
            PasswordHash = "hash"
        };
        await _context.Users.AddAsync(existingUser);
        await _context.SaveChangesAsync();

        var dto = new CreateEmployeeDto
        {
            Name = "New Employee",
            Email = "duplicate@example.com",
            Role = "Employee"
        };

        var (success, message, created) = await _employeeService.CreateEmployeeAsync(dto);

        Assert.False(success);
        Assert.Contains("already exists", message);
        Assert.Null(created);
    }

    [Fact]
    public async Task GetEmployeesAsync_FiltersBySearchDepartmentAndRole()
    {
        var dept1 = new Department { Id = Guid.NewGuid(), Name = "Finance" };
        var dept2 = new Department { Id = Guid.NewGuid(), Name = "Engineering" };
        await _context.Departments.AddRangeAsync(dept1, dept2);

        var u1 = new User { Id = Guid.NewGuid(), Name = "Alice Smith", Email = "alice@example.com", Role = "Finance", DepartmentId = dept1.Id, PasswordHash = "h" };
        var u2 = new User { Id = Guid.NewGuid(), Name = "Bob Jones", Email = "bob@example.com", Role = "Manager", DepartmentId = dept2.Id, PasswordHash = "h" };
        var u3 = new User { Id = Guid.NewGuid(), Name = "Charlie Brown", Email = "charlie@example.com", Role = "Employee", DepartmentId = dept2.Id, PasswordHash = "h" };
        await _context.Users.AddRangeAsync(u1, u2, u3);
        await _context.SaveChangesAsync();

        // Filter by search
        var searchResult = await _employeeService.GetEmployeesAsync(search: "alice");
        Assert.Single(searchResult);
        Assert.Equal("Alice Smith", searchResult[0].Name);

        // Filter by department
        var deptResult = await _employeeService.GetEmployeesAsync(department: "Engineering");
        Assert.Equal(2, deptResult.Count);

        // Filter by role
        var roleResult = await _employeeService.GetEmployeesAsync(role: "Manager");
        Assert.Single(roleResult);
        Assert.Equal("Bob Jones", roleResult[0].Name);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_ValidUpdate_UpdatesEmployeeProperties()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Original Name",
            Email = "original@example.com",
            Role = "Employee",
            PasswordHash = "h"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateEmployeeDto
        {
            Name = "Updated Name",
            Email = "updated@example.com",
            Role = "Manager",
            DepartmentName = "Product Management"
        };

        var (success, message, updated) = await _employeeService.UpdateEmployeeAsync(user.Id, updateDto);

        Assert.True(success);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal("updated@example.com", updated.Email);
        Assert.Equal("Manager", updated.Role);
        Assert.Equal("Product Management", updated.DepartmentName);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_SelfDeletion_ReturnsError()
    {
        var currentUserId = _currentUserServiceMock.Object.UserId!.Value;
        var adminUser = new User
        {
            Id = currentUserId,
            Name = "Admin Self",
            Email = "admin@example.com",
            Role = "Admin",
            PasswordHash = "h"
        };
        await _context.Users.AddAsync(adminUser);
        await _context.SaveChangesAsync();

        var (success, message) = await _employeeService.DeleteEmployeeAsync(currentUserId);

        Assert.False(success);
        Assert.Equal("You cannot delete your own active administrator account.", message);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_ValidEmployee_DeletesFromDatabaseAndAudits()
    {
        var employee = new User
        {
            Id = Guid.NewGuid(),
            Name = "To Delete",
            Email = "delete.me@example.com",
            Role = "Employee",
            PasswordHash = "h"
        };
        await _context.Users.AddAsync(employee);
        await _context.SaveChangesAsync();

        var (success, message) = await _employeeService.DeleteEmployeeAsync(employee.Id);

        Assert.True(success);
        var userInDb = await _context.Users.FindAsync(employee.Id);
        Assert.Null(userInDb);

        _auditServiceMock.Verify(a => a.LogAsync(
            "EMPLOYEE",
            employee.Id,
            "Delete Employee",
            It.IsAny<Guid?>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            "HIGH",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BatchUploadCsvAsync_ValidCsvStream_CreatesEmployees()
    {
        var csvContent = "Name,Email,Role,Department\n" +
                         "\"Kumar, Vijay\",vijay.kumar@example.com,Employee,Operations\n" +
                         "Sara Connor,sara@example.com,Manager,Security\n";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var result = await _employeeService.BatchUploadCsvAsync(stream);

        Assert.Equal(2, result.TotalProcessed);
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);

        var vijay = await _context.Users.FirstOrDefaultAsync(u => u.Email == "vijay.kumar@example.com");
        Assert.NotNull(vijay);
        Assert.Equal("Kumar, Vijay", vijay.Name);
    }
}
