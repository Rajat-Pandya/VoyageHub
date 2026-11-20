using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using VoyageHub.Api.Controllers;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Services;
using VoyageHub.Data.Entities;
using VoyageHub.Data.Repositories;
using Xunit;

namespace VoyageHub.Tests;

public class AuthorizationTests
{
    private readonly Mock<ITravelRequestService> _travelServiceMock;
    private readonly Mock<ITravelRequestRepository> _travelRepoMock;
    private readonly Mock<IApprovalStepRepository> _approvalStepRepoMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<TravelRequestsController>> _loggerMock;

    public AuthorizationTests()
    {
        _travelServiceMock = new Mock<ITravelRequestService>();
        _travelRepoMock = new Mock<ITravelRequestRepository>();
        _approvalStepRepoMock = new Mock<IApprovalStepRepository>();
        _auditServiceMock = new Mock<IAuditService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<TravelRequestsController>>();
    }

    private static ControllerContext CreateControllerContext(Guid userId, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task Employee_CannotReadAnotherEmployeesRequest_ReturnsForbidOrNull()
    {
        // Documented expected behavior: Controller returns HTTP 403 ForbidResult
        // and Service level returns null when an Employee tries to access another employee's request.
        var employeeAId = Guid.NewGuid();
        var employeeBId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        _currentUserServiceMock.Setup(u => u.UserId).Returns(employeeAId);
        _currentUserServiceMock.Setup(u => u.UserRole).Returns("Employee");

        var requestBelongingToB = new TravelRequestDetailDto
        {
            Id = requestId,
            EmployeeId = employeeBId,
            RequestCode = "#TR-2048",
            Destination = "Bengaluru",
            Purpose = "Confidential Review"
        };

        _travelServiceMock.Setup(s => s.GetDetailAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(requestBelongingToB);

        var controller = new TravelRequestsController(
            _travelServiceMock.Object,
            _currentUserServiceMock.Object,
            _loggerMock.Object)
        {
            ControllerContext = CreateControllerContext(employeeAId, "Employee")
        };

        // Act
        var result = await controller.GetDetail(requestId);

        // Assert: Employee A is forbidden from reading Employee B's request (403 ForbidResult)
        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task Employee_CannotReadAnotherEmployeesStats_ReturnsForbid()
    {
        // Documented expected behavior: Controller returns HTTP 403 ForbidResult
        // when an Employee passes another employee's ID in the query string.
        var employeeAId = Guid.NewGuid();
        var employeeBId = Guid.NewGuid();

        _currentUserServiceMock.Setup(u => u.UserId).Returns(employeeAId);
        _currentUserServiceMock.Setup(u => u.UserRole).Returns("Employee");

        var controller = new TravelRequestsController(
            _travelServiceMock.Object,
            _currentUserServiceMock.Object,
            _loggerMock.Object)
        {
            ControllerContext = CreateControllerContext(employeeAId, "Employee")
        };

        // Act: Employee A attempts to fetch Employee B's stats
        var result = await controller.GetStats(employeeBId);

        // Assert: Access is denied with 403 ForbidResult
        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task Employee_CannotFetchAnotherRolesQueue_ReturnsEmptyResult()
    {
        // Documented expected behavior: When called with an Employee identity,
        // ApprovalService.GetQueueAsync returns an empty queue (0 items, 0 pending total),
        // and ApprovalsController is guarded by [Authorize(Roles = "Manager,Finance,Admin")] returning 403 Forbidden.
        var employeeId = Guid.NewGuid();

        _currentUserServiceMock.Setup(u => u.UserId).Returns(employeeId);
        _currentUserServiceMock.Setup(u => u.UserRole).Returns("Employee");

        var approvalService = new ApprovalService(
            _approvalStepRepoMock.Object,
            _auditServiceMock.Object,
            _currentUserServiceMock.Object);

        // Act: Attempt to get queue as an employee (e.g. attempting to pass role="Manager")
        var queue = await approvalService.GetQueueAsync(approverId: null, role: "Manager");

        // Assert: Empty result is returned; employee cannot access manager approval queue
        Assert.NotNull(queue);
        Assert.Empty(queue.Items);
        Assert.Equal(0m, queue.PendingTotal);
        _approvalStepRepoMock.Verify(r => r.GetPendingStepsAsync(It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
