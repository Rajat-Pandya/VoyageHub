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

public class ApprovalServiceTests
{
    private readonly Mock<IApprovalStepRepository> _stepRepoMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly ApprovalService _sut;

    public ApprovalServiceTests()
    {
        _stepRepoMock = new Mock<IApprovalStepRepository>();
        _auditServiceMock = new Mock<IAuditService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _currentUserServiceMock.Setup(u => u.UserId).Returns(Guid.NewGuid());
        _currentUserServiceMock.Setup(u => u.UserName).Returns("Test Manager");

        _sut = new ApprovalService(_stepRepoMock.Object, _auditServiceMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task TakeActionAsync_WhenStepNotFound_ReturnsFalse()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        _stepRepoMock
            .Setup(r => r.GetByIdWithRequestAndApproverAsync(stepId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApprovalStep?)null);

        // Act
        var (success, message) = await _sut.TakeActionAsync(stepId, new ApprovalActionDto { Action = "Approve" });

        // Assert
        Assert.False(success);
        Assert.Equal("Approval step not found.", message);
    }

    [Fact]
    public async Task TakeActionAsync_ManagerApprove_RoutesToFinanceReviewAndActivatesFinanceStep()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var stepId = Guid.NewGuid();

        var request = new TravelRequest
        {
            Id = requestId,
            RequestCode = "#TR-2099",
            Status = "Manager Review",
            EstimatedBudget = 50000m,
            PolicyCompliance = "Standard"
        };

        var managerStep = new ApprovalStep
        {
            Id = stepId,
            RequestId = requestId,
            Request = request,
            Level = "Manager",
            Status = "Pending"
        };

        var financeStep = new ApprovalStep
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            Request = request,
            Level = "Finance",
            Status = "Waiting"
        };

        _stepRepoMock
            .Setup(r => r.GetByIdWithRequestAndApproverAsync(stepId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(managerStep);

        _stepRepoMock
            .Setup(r => r.FindFinanceStepAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(financeStep);

        // Act
        var (success, message) = await _sut.TakeActionAsync(stepId, new ApprovalActionDto { Action = "Approve", Comment = "Approved by manager" });

        // Assert
        Assert.True(success);
        Assert.Equal("Approved", managerStep.Status);
        Assert.Equal("Approved by manager", managerStep.Comment);
        Assert.Equal("Finance Review", request.Status);
        Assert.Equal("Pending", financeStep.Status);

        _stepRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _auditServiceMock.Verify(a => a.LogAsync(
            "APPROVAL",
            requestId,
            "Approved",
            It.IsAny<Guid?>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            "LOW",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TakeActionAsync_FinanceApprove_SetsRequestStatusToApproved()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var stepId = Guid.NewGuid();

        var request = new TravelRequest
        {
            Id = requestId,
            RequestCode = "#TR-2099",
            Status = "Finance Review",
            EstimatedBudget = 50000m,
            PolicyCompliance = "Standard"
        };

        var financeStep = new ApprovalStep
        {
            Id = stepId,
            RequestId = requestId,
            Request = request,
            Level = "Finance",
            Status = "Pending"
        };

        _stepRepoMock
            .Setup(r => r.GetByIdWithRequestAndApproverAsync(stepId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(financeStep);

        // Act
        var (success, message) = await _sut.TakeActionAsync(stepId, new ApprovalActionDto { Action = "Approve", Comment = "Finance OK" });

        // Assert
        Assert.True(success);
        Assert.Equal("Approved", financeStep.Status);
        Assert.Equal("Approved", request.Status);
        _stepRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TakeActionAsync_Reject_CancelsRemainingStepsAndMarksRequestRejected()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var stepId = Guid.NewGuid();

        var request = new TravelRequest
        {
            Id = requestId,
            RequestCode = "#TR-2099",
            Status = "Manager Review",
            EstimatedBudget = 50000m,
            PolicyCompliance = "Flagged"
        };

        var managerStep = new ApprovalStep
        {
            Id = stepId,
            RequestId = requestId,
            Request = request,
            Level = "Manager",
            Status = "Pending"
        };

        var waitingFinanceStep = new ApprovalStep
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            Level = "Finance",
            Status = "Waiting"
        };

        _stepRepoMock
            .Setup(r => r.GetByIdWithRequestAndApproverAsync(stepId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(managerStep);

        _stepRepoMock
            .Setup(r => r.GetRemainingNonApprovedStepsAsync(requestId, stepId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApprovalStep> { waitingFinanceStep });

        // Act
        var (success, message) = await _sut.TakeActionAsync(stepId, new ApprovalActionDto { Action = "Reject", Comment = "Budget exceeded" });

        // Assert
        Assert.True(success);
        Assert.Equal("Rejected", managerStep.Status);
        Assert.Equal("Rejected", request.Status);
        Assert.Equal("Cancelled", waitingFinanceStep.Status);

        _auditServiceMock.Verify(a => a.LogAsync(
            "APPROVAL",
            requestId,
            "Rejected",
            It.IsAny<Guid?>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            "MEDIUM", // Flagged severity
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
