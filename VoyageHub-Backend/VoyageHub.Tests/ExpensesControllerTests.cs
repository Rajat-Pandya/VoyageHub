using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VoyageHub.Api.Controllers;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Services;
using Xunit;

namespace VoyageHub.Tests;

public class ExpensesControllerTests : IDisposable
{
    private readonly Mock<IExpenseService> _expenseServiceMock;
    private readonly Mock<IWebHostEnvironment> _environmentMock;
    private readonly string _tempDirectory;
    private readonly ExpensesController _controller;

    public ExpensesControllerTests()
    {
        _expenseServiceMock = new Mock<IExpenseService>();
        _environmentMock = new Mock<IWebHostEnvironment>();

        _tempDirectory = Path.Combine(Path.GetTempPath(), "VoyageHubTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
        _environmentMock.Setup(e => e.ContentRootPath).Returns(_tempDirectory);

        _controller = new ExpensesController(_expenseServiceMock.Object, _environmentMock.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            try
            {
                Directory.Delete(_tempDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        GC.SuppressFinalize(this);
    }

    private static FormFile CreateMockFormFile(string fileName, byte[] content)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName);
    }

    [Fact]
    public async Task UploadProof_NullFile_ReturnsBadRequest()
    {
        var result = await _controller.UploadProof(null, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task UploadProof_DisallowedExtension_ReturnsBadRequest()
    {
        var file = CreateMockFormFile("malicious.exe", new byte[] { 1, 2, 3 });

        var result = await _controller.UploadProof(file, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task UploadProof_ValidPdf_SavesFileAndReturnsOk()
    {
        var content = Encoding.UTF8.GetBytes("%PDF-1.4 test receipt");
        var file = CreateMockFormFile("hotel_receipt.pdf", content);

        var result = await _controller.UploadProof(file, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        // Verify file was written into App_Data/Receipts
        var receiptsDir = Path.Combine(_tempDirectory, "App_Data", "Receipts");
        Assert.True(Directory.Exists(receiptsDir));
        var savedFiles = Directory.GetFiles(receiptsDir);
        Assert.Single(savedFiles);
        Assert.EndsWith(".pdf", savedFiles[0]);
    }

    [Fact]
    public async Task AttachProof_NullFile_ReturnsBadRequest()
    {
        var expenseId = Guid.NewGuid();
        var result = await _controller.AttachProof(expenseId, null, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AttachProof_NonExistentExpense_ReturnsNotFound()
    {
        var expenseId = Guid.NewGuid();
        var file = CreateMockFormFile("bill.png", new byte[] { 0x89, 0x50, 0x4E, 0x47 });

        _expenseServiceMock
            .Setup(s => s.AttachProofAsync(expenseId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "Expense not found", null));

        var result = await _controller.AttachProof(expenseId, file, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public async Task AttachProof_ValidExpense_AttachesProofAndReturnsOk()
    {
        var expenseId = Guid.NewGuid();
        var file = CreateMockFormFile("taxi_bill.jpg", new byte[] { 0xFF, 0xD8, 0xFF });

        _expenseServiceMock
            .Setup(s => s.AttachProofAsync(expenseId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "Proof attached successfully", "/api/expenses/proof/taxi_bill.jpg"));

        var result = await _controller.AttachProof(expenseId, file, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetPending_ReturnsOkWithSummary()
    {
        var summary = new ExpenseReviewSummaryDto
        {
            TotalPendingAmount = 15000m,
            PendingClaims = new List<ExpenseItemDto>()
        };

        _expenseServiceMock
            .Setup(s => s.GetPendingSummaryAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        var result = await _controller.GetPending(null, null, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(summary, okResult.Value);
    }

    [Fact]
    public async Task TakeAction_CallsServiceAndReturnsOk()
    {
        var expenseId = Guid.NewGuid();
        var dto = new ExpenseActionDto
        {
            Action = "Approve"
        };

        _expenseServiceMock
            .Setup(s => s.TakeActionAsync(expenseId, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "Expense approved"));

        var result = await _controller.TakeAction(expenseId, dto, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task UploadProof_InvalidExtension_ReturnsBadRequest()
    {
        var file = CreateMockFormFile("malicious.exe", new byte[] { 0x4D, 0x5A });

        var result = await _controller.UploadProof(file, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task UploadProof_EmptyFile_ReturnsBadRequest()
    {
        var file = CreateMockFormFile("empty.pdf", Array.Empty<byte>());

        var result = await _controller.UploadProof(file, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task SubmitExpense_ServiceReturnsFailure_ReturnsBadRequest()
    {
        var dto = new SubmitExpenseDto { RequestId = Guid.NewGuid(), Amount = -50m };
        _expenseServiceMock
            .Setup(s => s.SubmitExpenseAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "Expense amount must be greater than zero.", null));

        var result = await _controller.SubmitExpense(dto, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task SubmitExpense_ValidDto_ReturnsOk()
    {
        var dto = new SubmitExpenseDto { RequestId = Guid.NewGuid(), Amount = 500m, Category = "Meals" };
        var expense = new VoyageHub.Data.Entities.Expense { Id = Guid.NewGuid(), Amount = 500m, Category = "Meals" };
        _expenseServiceMock
            .Setup(s => s.SubmitExpenseAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, string.Empty, expense));

        var result = await _controller.SubmitExpense(dto, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expense, okResult.Value);
    }

    [Fact]
    public async Task TakeAction_NotFound_ReturnsNotFound()
    {
        var expenseId = Guid.NewGuid();
        var dto = new ExpenseActionDto { Action = "Reject" };

        _expenseServiceMock
            .Setup(s => s.TakeActionAsync(expenseId, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "Expense claim not found."));

        var result = await _controller.TakeAction(expenseId, dto, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public async Task BatchPayout_Failure_ReturnsBadRequest()
    {
        var dto = new BatchPayoutDto { ExpenseIds = new List<Guid>() };
        _expenseServiceMock
            .Setup(s => s.BatchPayoutAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "Payout failed"));

        var result = await _controller.BatchPayout(dto, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task BatchPayout_Success_ReturnsOk()
    {
        var dto = new BatchPayoutDto { ExpenseIds = new List<Guid> { Guid.NewGuid() } };
        _expenseServiceMock
            .Setup(s => s.BatchPayoutAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "Payout processed"));

        var result = await _controller.BatchPayout(dto, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task UploadProof_FileTooLarge_ReturnsBadRequest()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(16 * 1024 * 1024);
        mockFile.Setup(f => f.FileName).Returns("large.pdf");

        var result = await _controller.UploadProof(mockFile.Object, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public void GetProofFile_EmptyFileName_ReturnsBadRequest()
    {
        var result = _controller.GetProofFile("");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public void GetProofFile_FileNotFound_ReturnsNotFound()
    {
        var result = _controller.GetProofFile("nonexistent_receipt.pdf");

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFound.Value);
    }

    [Fact]
    public void GetProofFile_FileExists_ReturnsFileStreamResult()
    {
        var receiptsDir = Path.Combine(_tempDirectory, "App_Data", "Receipts");
        Directory.CreateDirectory(receiptsDir);
        var testFilePath = Path.Combine(receiptsDir, "sample_receipt.pdf");
        File.WriteAllBytes(testFilePath, new byte[] { 0x25, 0x50, 0x44, 0x46 });

        var result = _controller.GetProofFile("sample_receipt.pdf");

        var fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal("sample_receipt.pdf", fileResult.FileDownloadName);
        fileResult.FileStream.Dispose();
    }
}
