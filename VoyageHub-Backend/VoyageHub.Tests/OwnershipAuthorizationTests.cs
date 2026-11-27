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

public class OwnershipAuthorizationTests
{
    [Fact]
    public async Task SupportService_GetTicketsAsync_Employee_ForcesOwnUserId()
    {
        var employeeId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var repoMock = new Mock<ISupportTicketRepository>();
        repoMock
            .Setup(r => r.GetTicketsAsync(null, employeeId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SupportTicket>
            {
                new SupportTicket { Id = Guid.NewGuid(), TicketCode = "#TICK-1", UserId = employeeId, Subject = "Mine", Description = "Desc" }
            });

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(u => u.UserId).Returns(employeeId);
        currentUserMock.Setup(u => u.UserRole).Returns("Employee");

        var service = new SupportService(repoMock.Object, Mock.Of<IAuditService>(), currentUserMock.Object);

        var result = await service.GetTicketsAsync(null, otherUserId, null);

        Assert.Single(result);
        Assert.Equal(employeeId, result[0].UserId);
        repoMock.Verify(r => r.GetTicketsAsync(null, employeeId, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("Manager")]
    [InlineData("Finance")]
    [InlineData("Admin")]
    public async Task SupportService_GetTicketsAsync_NonEmployee_PreservesVisibility(string role)
    {
        var callerId = Guid.NewGuid();
        var repoMock = new Mock<ISupportTicketRepository>();
        repoMock
            .Setup(r => r.GetTicketsAsync(null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SupportTicket>
            {
                new SupportTicket { Id = Guid.NewGuid(), TicketCode = "#TICK-1", UserId = Guid.NewGuid(), Subject = "T1", Description = "D1" },
                new SupportTicket { Id = Guid.NewGuid(), TicketCode = "#TICK-2", UserId = Guid.NewGuid(), Subject = "T2", Description = "D2" }
            });

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(u => u.UserId).Returns(callerId);
        currentUserMock.Setup(u => u.UserRole).Returns(role);

        var service = new SupportService(repoMock.Object, Mock.Of<IAuditService>(), currentUserMock.Object);

        var result = await service.GetTicketsAsync(null, null, null);

        Assert.Equal(2, result.Count);
        repoMock.Verify(r => r.GetTicketsAsync(null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SupportService_CreateTicketAsync_IgnoresDtoUserId_UsesCurrentUser()
    {
        var callerId = Guid.NewGuid();
        var spoofedUserId = Guid.NewGuid();

        var repoMock = new Mock<ISupportTicketRepository>();
        repoMock
            .Setup(r => r.FindUserByIdAsync(callerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = callerId, Name = "Real Caller" });

        SupportTicket? addedTicket = null;
        repoMock
            .Setup(r => r.AddAsync(It.IsAny<SupportTicket>(), It.IsAny<CancellationToken>()))
            .Callback<SupportTicket, CancellationToken>((t, _) => addedTicket = t)
            .Returns(Task.CompletedTask);

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(u => u.UserId).Returns(callerId);
        currentUserMock.Setup(u => u.UserName).Returns("Real Caller");
        currentUserMock.Setup(u => u.UserRole).Returns("Employee");

        var service = new SupportService(repoMock.Object, Mock.Of<IAuditService>(), currentUserMock.Object);

        var (success, _, ticket) = await service.CreateTicketAsync(new CreateSupportTicketDto
        {
            Category = "General",
            Priority = "Medium",
            Subject = "Need help",
            Description = "Details",
            UserId = spoofedUserId
        });

        Assert.True(success);
        Assert.NotNull(addedTicket);
        Assert.Equal(callerId, addedTicket!.UserId);
        Assert.Equal(callerId, ticket!.UserId);
    }

    [Fact]
    public async Task SupportController_GetTicketById_OtherEmployeesTicket_ReturnsForbid()
    {
        var callerId = Guid.NewGuid();
        var otherEmployeeId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();

        var supportServiceMock = new Mock<ISupportService>();
        supportServiceMock
            .Setup(s => s.GetTicketByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SupportTicketDto { Id = ticketId, UserId = otherEmployeeId, Subject = "Secret" });

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(u => u.UserId).Returns(callerId);
        currentUserMock.Setup(u => u.UserRole).Returns("Employee");

        var controller = new SupportController(supportServiceMock.Object, currentUserMock.Object, Mock.Of<ILogger<SupportController>>());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, callerId.ToString()),
                    new Claim(ClaimTypes.Role, "Employee")
                }, "TestAuth"))
            }
        };

        var result = await controller.GetTicketById(ticketId, CancellationToken.None);

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task TravelRequestsController_Create_Manager_ForcesTokenUserId()
    {
        var managerId = Guid.NewGuid();
        var spoofedEmployeeId = Guid.NewGuid();

        var serviceMock = new Mock<ITravelRequestService>();
        serviceMock
            .Setup(s => s.CreateRequestAsync(It.IsAny<CreateTravelRequestDto>(), managerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, string.Empty, new TravelRequestDetailDto { Id = Guid.NewGuid(), EmployeeId = managerId }));

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(u => u.UserId).Returns(managerId);
        currentUserMock.Setup(u => u.UserRole).Returns("Manager");

        var controller = new TravelRequestsController(serviceMock.Object, currentUserMock.Object, Mock.Of<ILogger<TravelRequestsController>>());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, managerId.ToString()),
                    new Claim(ClaimTypes.Role, "Manager")
                }, "TestAuth"))
            }
        };

        var dto = new CreateTravelRequestDto
        {
            Destination = "Mumbai",
            Purpose = "Meeting",
            StartDate = DateTime.UtcNow.Date.AddDays(1),
            EndDate = DateTime.UtcNow.Date.AddDays(3),
            EstimatedBudget = 5000
        };

        var result = await controller.Create(dto, spoofedEmployeeId);

        Assert.IsType<CreatedAtActionResult>(result.Result);
        serviceMock.Verify(s => s.CreateRequestAsync(dto, managerId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
