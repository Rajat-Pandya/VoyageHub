using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Services;
using VoyageHub.Data;
using VoyageHub.Data.Entities;
using VoyageHub.Data.Repositories;
using Xunit;

namespace VoyageHub.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly UserRepository _userRepository;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
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
        _loggerMock = new Mock<ILogger<AuthService>>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _jwtTokenServiceMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("test-valid-jwt-token");
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _authService = new AuthService(
            _userRepository,
            _auditServiceMock.Object,
            _loggerMock.Object,
            _jwtTokenServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Login_WithEmptyEmailOrPassword_Returns400WithDocumentedMessage()
    {
        // Act - Empty Email
        var (statusEmail, responseEmail) = await _authService.LoginAsync(new LoginDto
        {
            Email = "",
            Password = "SomePassword123!"
        });

        // Act - Empty Password
        var (statusPass, responsePass) = await _authService.LoginAsync(new LoginDto
        {
            Email = "user@voyagehub.internal",
            Password = ""
        });

        // Assert
        Assert.Equal(400, statusEmail);
        Assert.False(responseEmail.Success);
        Assert.Equal("Email and password are required.", responseEmail.Message);

        Assert.Equal(400, statusPass);
        Assert.False(responsePass.Success);
        Assert.Equal("Email and password are required.", responsePass.Message);
    }

    [Fact]
    public async Task Login_WithWrongPassword_IsRejectedWith401()
    {
        // Arrange: User with valid BCrypt hash for a secure password
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "secure.user@voyagehub.internal",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectSecurePassword123!", 12),
            Role = "Employee",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act: Attempt to login with an incorrect password
        var (status, response) = await _authService.LoginAsync(new LoginDto
        {
            Email = "secure.user@voyagehub.internal",
            Password = "IncorrectPassword999!"
        });

        // Assert
        Assert.Equal(401, status);
        Assert.False(response.Success);
        Assert.Equal("Incorrect password. Please try again.", response.Message);
    }

    [Fact]
    public async Task Login_WhenPasswordHashIsPlaintext_CannotLogInWithPlaintext()
    {
        // Arrange: User whose PasswordHash column holds an unhashed plaintext value
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Plaintext User",
            Email = "plaintext.user@voyagehub.internal",
            PasswordHash = "UnencryptedPlaintextString123!",
            Role = "Employee",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act: Attempt to login with the plaintext string
        var (status, response) = await _authService.LoginAsync(new LoginDto
        {
            Email = "plaintext.user@voyagehub.internal",
            Password = "UnencryptedPlaintextString123!"
        });

        // Assert: Must fail closed with 401, not 200
        Assert.Equal(401, status);
        Assert.False(response.Success);
        Assert.Equal("Incorrect password. Please try again.", response.Message);
    }

    [Fact]
    public async Task Login_WithCorrectPassword_WhenHashIsBCrypt_IsAcceptedWith200()
    {
        // Arrange: User with valid BCrypt hash
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Valid User",
            Email = "valid.user@voyagehub.internal",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ValidPassword123!", 12),
            Role = "Employee",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act: Attempt login with the matching password
        var (status, response) = await _authService.LoginAsync(new LoginDto
        {
            Email = "valid.user@voyagehub.internal",
            Password = "ValidPassword123!"
        });

        // Assert
        Assert.Equal(200, status);
        Assert.True(response.Success);
        Assert.NotNull(response.Token);
        Assert.Equal("Valid User", response.User?.Fullname);
    }
}
