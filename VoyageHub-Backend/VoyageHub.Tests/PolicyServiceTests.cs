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

public class PolicyServiceTests
{
    private readonly Mock<IPolicyRepository> _policyRepoMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly PolicyService _sut;

    public PolicyServiceTests()
    {
        _policyRepoMock = new Mock<IPolicyRepository>();
        _auditServiceMock = new Mock<IAuditService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _currentUserServiceMock.Setup(u => u.UserId).Returns(Guid.NewGuid());
        _currentUserServiceMock.Setup(u => u.UserName).Returns("Admin User");

        _sut = new PolicyService(_policyRepoMock.Object, _auditServiceMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task CreatePolicyAsync_WhenDepartmentAlreadyHasPolicy_ReturnsBadRequest()
    {
        // Arrange
        var existingDept = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Engineering",
            Policy = new TravelPolicy { Id = Guid.NewGuid(), IntlBudgetLimit = 5000m }
        };

        _policyRepoMock
            .Setup(r => r.GetDepartmentByNameWithPolicyAsync("Engineering", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDept);

        var dto = new CreatePolicyDto
        {
            DepartmentName = "Engineering",
            IntlBudgetLimit = 6000m,
            DomesticLimit = 1500m
        };

        // Act
        var (success, message, policy) = await _sut.CreatePolicyAsync(dto);

        // Assert
        Assert.False(success);
        Assert.Equal("A policy already exists for this department.", message);
        Assert.Null(policy);
        _policyRepoMock.Verify(r => r.AddAsync(It.IsAny<TravelPolicy>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreatePolicyAsync_ValidDepartmentWithoutPolicy_CreatesPolicyAndLogsAudit()
    {
        // Arrange
        var dept = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Sales",
            Policy = null
        };

        _policyRepoMock
            .Setup(r => r.GetDepartmentByNameWithPolicyAsync("Sales", It.IsAny<CancellationToken>()))
            .ReturnsAsync(dept);

        var dto = new CreatePolicyDto
        {
            DepartmentName = "Sales",
            IntlBudgetLimit = 80000m,
            DomesticLimit = 20000m,
            AllowedClass = "BUSINESS",
            HardEnforcement = true
        };

        // Act
        var (success, message, policy) = await _sut.CreatePolicyAsync(dto);

        // Assert
        Assert.True(success);
        Assert.NotNull(policy);
        Assert.Equal("Sales", policy!.DepartmentName);
        Assert.Equal(80000m, policy.IntlBudgetLimit);
        Assert.Equal("BUSINESS", policy.AllowedClass);

        _policyRepoMock.Verify(r => r.AddAsync(It.IsAny<TravelPolicy>(), It.IsAny<CancellationToken>()), Times.Once);
        _policyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _auditServiceMock.Verify(a => a.LogAsync(
            "POLICY",
            It.IsAny<Guid>(),
            "Create Policy",
            It.IsAny<Guid?>(),
            "Admin User",
            It.IsAny<string>(),
            "LOW",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePolicyAsync_ValidPolicy_UpdatesLimitsAndLogsAudit()
    {
        // Arrange
        var policyId = Guid.NewGuid();
        var department = new Department { Id = Guid.NewGuid(), Name = "HR" };
        var existingPolicy = new TravelPolicy
        {
            Id = policyId,
            DepartmentId = department.Id,
            Department = department,
            IntlBudgetLimit = 4000m,
            DomesticLimit = 1000m,
            AllowedClass = "ECONOMY",
            HardEnforcement = false
        };

        _policyRepoMock
            .Setup(r => r.GetByIdWithDepartmentAsync(policyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPolicy);

        var updateDto = new UpdatePolicyDto
        {
            IntlBudgetLimit = 6500m,
            DomesticLimit = 1800m,
            AllowedClass = "ECONOMY+",
            HardEnforcement = true
        };

        // Act
        var (success, message) = await _sut.UpdatePolicyAsync(policyId, updateDto);

        // Assert
        Assert.True(success);
        Assert.Equal("Policy updated successfully.", message);
        Assert.Equal(6500m, existingPolicy.IntlBudgetLimit);
        Assert.Equal(1800m, existingPolicy.DomesticLimit);
        Assert.Equal("ECONOMY+", existingPolicy.AllowedClass);
        Assert.True(existingPolicy.HardEnforcement);

        _policyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _auditServiceMock.Verify(a => a.LogAsync(
            "POLICY",
            policyId,
            "Update Limit",
            It.IsAny<Guid?>(),
            "Admin User",
            It.Is<string>(s => s.Contains("4,000") && s.Contains("6,500")),
            "LOW",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePolicyAsync_WhenPolicyNotFound_ReturnsFalse()
    {
        // Arrange
        var policyId = Guid.NewGuid();
        _policyRepoMock
            .Setup(r => r.GetByIdWithDepartmentAsync(policyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TravelPolicy?)null);

        // Act
        var (success, message) = await _sut.DeletePolicyAsync(policyId);

        // Assert
        Assert.False(success);
        Assert.Equal("Policy not found.", message);
    }

    [Fact]
    public void PolicyEvaluator_EvaluatesDomesticVsInternationalLimit()
    {
        var policy = new TravelPolicy
        {
            Id = Guid.NewGuid(),
            DomesticLimit = 40000m,
            IntlBudgetLimit = 250000m,
            AllowedClass = "ECONOMY+",
            HardEnforcement = true
        };

        // 50,000 exceeds domestic limit (40,000) -> flagged
        var (domFlagged, domReasons) = VoyageHub.Business.Helpers.PolicyEvaluator.Evaluate(
            50000m, "Economy (Standard)", policy, "Engineering", null, isInternational: false);
        Assert.True(domFlagged);
        Assert.Contains(domReasons, r => r.Contains("domestic budget limit"));

        // 50,000 is within international limit (250,000) -> not flagged
        var (intlFlagged, intlReasons) = VoyageHub.Business.Helpers.PolicyEvaluator.Evaluate(
            50000m, "Economy (Standard)", policy, "Engineering", null, isInternational: true);
        Assert.False(intlFlagged);
        Assert.Empty(intlReasons);
    }

    [Fact]
    public async Task GetAvailableDepartmentsAsync_ReturnsDepartmentsWithoutPolicy()
    {
        _policyRepoMock
            .Setup(r => r.GetDepartmentsWithoutPolicyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "Analytics", "IT" });

        var result = await _sut.GetAvailableDepartmentsAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains("Analytics", result);
        Assert.Contains("IT", result);
    }

    [Fact]
    public async Task GetMyPolicyAsync_ReturnsCurrentUsersDepartmentPolicy()
    {
        var userId = Guid.NewGuid();
        var deptId = Guid.NewGuid();
        _currentUserServiceMock.Setup(c => c.UserId).Returns(userId);
        _policyRepoMock
            .Setup(r => r.GetPolicyByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TravelPolicy
            {
                Id = Guid.NewGuid(),
                DepartmentId = deptId,
                Department = new Department { Id = deptId, Name = "Engineering" },
                DomesticLimit = 40000m,
                IntlBudgetLimit = 250000m,
                AllowedClass = "ECONOMY+",
                HardEnforcement = true
            });

        var result = await _sut.GetMyPolicyAsync();

        Assert.NotNull(result);
        Assert.Equal("Engineering", result.DepartmentName);
        Assert.Equal(40000m, result.DomesticLimit);
        Assert.Equal(250000m, result.IntlBudgetLimit);
    }

    [Fact]
    public async Task GetMyPolicyAsync_WhenNoUserOrPolicy_ReturnsNull()
    {
        _currentUserServiceMock.Setup(c => c.UserId).Returns((Guid?)null);
        Assert.Null(await _sut.GetMyPolicyAsync());

        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(c => c.UserId).Returns(userId);
        _policyRepoMock.Setup(r => r.GetPolicyByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((TravelPolicy?)null);
        Assert.Null(await _sut.GetMyPolicyAsync());
    }
}
