using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using VoyageHub.Api.Controllers;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Services;
using VoyageHub.Data;
using VoyageHub.Data.Entities;
using VoyageHub.Data.Repositories;
using Xunit;

namespace VoyageHub.Tests;

public class RepositoryAndControllerCoverageTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task PolicyRepository_GetDepartmentsWithoutPolicyAndByUser_Works()
    {
        using var ctx = CreateInMemoryContext();
        var deptWithPolicy = new Department { Id = Guid.NewGuid(), Name = "Engineering" };
        var deptWithoutPolicy = new Department { Id = Guid.NewGuid(), Name = "Analytics" };
        var policy = new TravelPolicy
        {
            Id = Guid.NewGuid(),
            DepartmentId = deptWithPolicy.Id,
            DomesticLimit = 40000m,
            IntlBudgetLimit = 250000m,
            AllowedClass = "ECONOMY+",
            HardEnforcement = true
        };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@voyagehub.in",
            PasswordHash = "hash",
            Role = "Employee",
            DepartmentId = deptWithPolicy.Id
        };

        ctx.Departments.AddRange(deptWithPolicy, deptWithoutPolicy);
        ctx.TravelPolicies.Add(policy);
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var repo = new PolicyRepository(ctx);
        var withoutPolicy = await repo.GetDepartmentsWithoutPolicyAsync();
        Assert.Single(withoutPolicy);
        Assert.Equal("Analytics", withoutPolicy[0]);

        var userPolicy = await repo.GetPolicyByUserIdAsync(user.Id);
        Assert.NotNull(userPolicy);
        Assert.Equal(40000m, userPolicy.DomesticLimit);
    }

    [Fact]
    public async Task ExpenseRepository_FilterAndByUser_Works()
    {
        using var ctx = CreateInMemoryContext();
        var dept = new Department { Id = Guid.NewGuid(), Name = "Sales" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Rohan",
            Email = "rohan@voyagehub.in",
            PasswordHash = "hash",
            Role = "Employee",
            DepartmentId = dept.Id
        };
        var req = new TravelRequest
        {
            Id = Guid.NewGuid(),
            RequestCode = "#TR-9001",
            EmployeeId = user.Id,
            Destination = "Mumbai",
            Purpose = "Client",
            Status = "Approved",
            EstimatedBudget = 20000m
        };
        var exp1 = new Expense { Id = Guid.NewGuid(), RequestId = req.Id, Category = "Meals", Amount = 12000m, Status = "Pending" };
        var exp2 = new Expense { Id = Guid.NewGuid(), RequestId = req.Id, Category = "Cab", Amount = 1500m, Status = "Reimbursed" };

        ctx.Departments.Add(dept);
        ctx.Users.Add(user);
        ctx.TravelRequests.Add(req);
        ctx.Expenses.AddRange(exp1, exp2);
        await ctx.SaveChangesAsync();

        var repo = new ExpenseRepository(ctx);
        var pendingMeals = await repo.GetExpensesByFilterAsync("Pending", "Meals", true);
        Assert.Single(pendingMeals);
        Assert.Equal(12000m, pendingMeals[0].Amount);

        var myExpenses = await repo.GetExpensesByUserIdAsync(user.Id);
        Assert.Equal(2, myExpenses.Count);
    }

    [Fact]
    public async Task PoliciesController_AvailableDepartmentsAndMyPolicy_Works()
    {
        var policyServiceMock = new Mock<IPolicyService>();
        policyServiceMock
            .Setup(s => s.GetAvailableDepartmentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "Analytics", "IT" });
        policyServiceMock
            .Setup(s => s.GetMyPolicyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PolicyConfigDto { DepartmentName = "Engineering", DomesticLimit = 40000m });

        var controller = new PoliciesController(policyServiceMock.Object);

        var deptsResult = await controller.GetAvailableDepartments(CancellationToken.None);
        var okDepts = Assert.IsType<OkObjectResult>(deptsResult.Result);
        Assert.Equal(2, Assert.IsType<List<string>>(okDepts.Value).Count);

        var myPolicyResult = await controller.GetMyPolicy(CancellationToken.None);
        var okPolicy = Assert.IsType<OkObjectResult>(myPolicyResult.Result);
        Assert.Equal("Engineering", Assert.IsType<PolicyConfigDto>(okPolicy.Value).DepartmentName);

        policyServiceMock.Setup(s => s.GetMyPolicyAsync(It.IsAny<CancellationToken>())).ReturnsAsync((PolicyConfigDto?)null);
        var notFound = await controller.GetMyPolicy(CancellationToken.None);
        Assert.IsType<NotFoundObjectResult>(notFound.Result);
    }

    [Fact]
    public async Task TravelRequestService_CloseRequestAsync_EnforcesOwnershipAndApprovedStatus()
    {
        var reqRepoMock = new Mock<ITravelRequestRepository>();
        var auditMock = new Mock<IAuditService>();
        var userMock = new Mock<ICurrentUserService>();
        var ownerId = Guid.NewGuid();
        userMock.Setup(u => u.UserId).Returns(ownerId);
        userMock.Setup(u => u.UserRole).Returns("Employee");
        userMock.Setup(u => u.UserName).Returns("Owner");

        var sut = new TravelRequestService(reqRepoMock.Object, auditMock.Object, userMock.Object);

        reqRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TravelRequest?)null);
        var (notFoundOk, _) = await sut.CloseRequestAsync(Guid.NewGuid());
        Assert.False(notFoundOk);

        var otherReq = new TravelRequest { Id = Guid.NewGuid(), EmployeeId = Guid.NewGuid(), Status = "Approved" };
        reqRepoMock.Setup(r => r.GetByIdAsync(otherReq.Id, It.IsAny<CancellationToken>())).ReturnsAsync(otherReq);
        var (nonOwnerOk, _) = await sut.CloseRequestAsync(otherReq.Id);
        Assert.False(nonOwnerOk);

        var draftReq = new TravelRequest { Id = Guid.NewGuid(), EmployeeId = ownerId, Status = "Draft" };
        reqRepoMock.Setup(r => r.GetByIdAsync(draftReq.Id, It.IsAny<CancellationToken>())).ReturnsAsync(draftReq);
        var (notApprovedOk, _) = await sut.CloseRequestAsync(draftReq.Id);
        Assert.False(notApprovedOk);

        var approvedReq = new TravelRequest { Id = Guid.NewGuid(), EmployeeId = ownerId, Status = "Approved", RequestCode = "#TR-100" };
        reqRepoMock.Setup(r => r.GetByIdAsync(approvedReq.Id, It.IsAny<CancellationToken>())).ReturnsAsync(approvedReq);
        var (ok, _) = await sut.CloseRequestAsync(approvedReq.Id);
        Assert.True(ok);
        Assert.Equal("Completed", approvedReq.Status);
    }

    [Fact]
    public async Task TravelRequestService_GetDetailAndGetRequests_MapsIsInternational()
    {
        var reqRepoMock = new Mock<ITravelRequestRepository>();
        var auditMock = new Mock<IAuditService>();
        var userMock = new Mock<ICurrentUserService>();
        var empId = Guid.NewGuid();
        userMock.Setup(u => u.UserId).Returns(empId);
        userMock.Setup(u => u.UserRole).Returns("Employee");

        var req = new TravelRequest
        {
            Id = Guid.NewGuid(),
            RequestCode = "#TR-777",
            EmployeeId = empId,
            Destination = "Tokyo",
            IsInternational = true,
            Purpose = "Expo",
            EstimatedBudget = 120000m,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(3),
            Status = "Approved",
            Employee = new User
            {
                Id = empId,
                Name = "Rohan",
                Department = new Department
                {
                    Name = "Engineering",
                    Policy = new TravelPolicy { DomesticLimit = 40000m, IntlBudgetLimit = 250000m, HardEnforcement = true }
                }
            }
        };

        reqRepoMock.Setup(r => r.GetDetailAsync(req.Id, It.IsAny<CancellationToken>())).ReturnsAsync(req);
        reqRepoMock
            .Setup(r => r.GetRequestsAsync(null, null, empId, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TravelRequest> { req });

        var sut = new TravelRequestService(reqRepoMock.Object, auditMock.Object, userMock.Object);

        var detail = await sut.GetDetailAsync(req.Id);
        Assert.NotNull(detail);
        Assert.True(detail.IsInternational);

        var list = await sut.GetRequestsAsync(null, null, null, null);
        Assert.Single(list);
        Assert.True(list[0].IsInternational);
    }
}
