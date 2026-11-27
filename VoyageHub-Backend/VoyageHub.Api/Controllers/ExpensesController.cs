using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Services;

namespace VoyageHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".png", ".jpg", ".jpeg", ".webp"
    };

    private readonly IExpenseService _expenseService;
    private readonly IWebHostEnvironment _environment;

    public ExpensesController(IExpenseService expenseService, IWebHostEnvironment environment)
    {
        _expenseService = expenseService;
        _environment = environment;
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Finance,Admin")]
    public async Task<ActionResult<ExpenseReviewSummaryDto>> GetPending(
        [FromQuery] string? category,
        [FromQuery] bool? highPriorityOnly,
        [FromQuery] string? status = "Pending",
        CancellationToken cancellationToken = default)
    {
        var result = string.IsNullOrWhiteSpace(status) || string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase)
            ? await _expenseService.GetPendingSummaryAsync(category, highPriorityOnly, cancellationToken)
            : await _expenseService.GetExpensesSummaryAsync(status, category, highPriorityOnly, cancellationToken);
        return Ok(result);
    }

    [HttpGet("my-expenses")]
    [Authorize(Roles = "Employee,Manager,Finance,Admin")]
    public async Task<ActionResult<List<ExpenseItemDto>>> GetMyExpenses(CancellationToken cancellationToken)
    {
        var result = await _expenseService.GetMyExpensesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("my-reports")]
    [Authorize(Roles = "Employee,Manager,Finance,Admin")]
    public async Task<ActionResult<List<TripReimbursementReportDto>>> GetMyReports(CancellationToken cancellationToken)
    {
        var result = await _expenseService.GetMyReimbursementReportsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Employee,Manager,Finance,Admin")]
    public async Task<IActionResult> SubmitExpense([FromBody] SubmitExpenseDto dto, CancellationToken cancellationToken)
    {
        var (success, message, expense) = await _expenseService.SubmitExpenseAsync(dto, cancellationToken);
        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(expense);
    }

    [HttpPost("{id}/action")]
    [Authorize(Roles = "Finance,Admin")]
    public async Task<IActionResult> TakeAction(Guid id, [FromBody] ExpenseActionDto dto, CancellationToken cancellationToken)
    {
        var (success, message) = await _expenseService.TakeActionAsync(id, dto, cancellationToken);
        if (!success)
        {
            return NotFound(new { message });
        }

        return Ok(new { success = true, message });
    }

    [HttpPost("{id}/resubmit")]
    [Authorize(Roles = "Employee,Manager,Finance,Admin")]
    public async Task<IActionResult> ResubmitExpense(Guid id, [FromBody] SubmitExpenseDto? dto, CancellationToken cancellationToken)
    {
        var (success, message, expense) = await _expenseService.ResubmitExpenseAsync(id, dto, cancellationToken);
        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(expense);
    }

    [HttpPost("batch-payout")]
    [Authorize(Roles = "Finance,Admin")]
    public async Task<IActionResult> BatchPayout([FromBody] BatchPayoutDto? dto, CancellationToken cancellationToken)
    {
        var (success, message) = await _expenseService.BatchPayoutAsync(dto, cancellationToken);
        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new { success = true, message });
    }

    [HttpGet("export-payout")]
    [Authorize(Roles = "Finance,Admin")]
    public async Task<IActionResult> ExportPayoutCsv([FromQuery] string? status = "Reimbursed", CancellationToken cancellationToken = default)
    {
        var csv = await _expenseService.ExportPayoutCsvAsync(status, cancellationToken);
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", $"voyagehub_payout_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpPost("upload-proof")]
    [Authorize(Roles = "Employee,Manager,Finance,Admin")]
    public async Task<IActionResult> UploadProof(IFormFile? file, CancellationToken cancellationToken)
    {
        var (errorResult, relativeUrl, fileName) = await SaveProofFileAsync(file, cancellationToken);
        if (errorResult != null)
        {
            return errorResult;
        }

        return Ok(new
        {
            success = true,
            proofPath = relativeUrl,
            fileName,
            fileUrl = $"{Request.Scheme}://{Request.Host}{relativeUrl}"
        });
    }

    [HttpPost("{id}/proof")]
    [Authorize(Roles = "Employee,Manager,Finance,Admin")]
    public async Task<IActionResult> AttachProof(Guid id, IFormFile? file, CancellationToken cancellationToken)
    {
        var (errorResult, relativeUrl, fileName) = await SaveProofFileAsync(file, cancellationToken);
        if (errorResult != null)
        {
            return errorResult;
        }

        var (success, message, proofPath) = await _expenseService.AttachProofAsync(id, relativeUrl!, cancellationToken);
        if (!success)
        {
            return NotFound(new { message });
        }

        return Ok(new
        {
            success = true,
            proofPath = relativeUrl,
            fileName,
            fileUrl = $"{Request.Scheme}://{Request.Host}{relativeUrl}",
            message
        });
    }

    private async Task<(IActionResult? ErrorResult, string? RelativeUrl, string? FileName)> SaveProofFileAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return (BadRequest(new { message = "No receipt or bill file uploaded." }), null, null);
        }

        if (file.Length > 15 * 1024 * 1024)
        {
            return (BadRequest(new { message = "File size exceeds the 15MB limit." }), null, null);
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            return (BadRequest(new { message = "Only PDF and image files (.pdf, .jpg, .jpeg, .png, .webp) are allowed as proof." }), null, null);
        }

        var receiptsDir = Path.Combine(_environment.ContentRootPath, "App_Data", "Receipts");
        if (!Directory.Exists(receiptsDir))
        {
            Directory.CreateDirectory(receiptsDir);
        }

        var safeFileName = $"{Guid.NewGuid():N}_{Path.GetFileNameWithoutExtension(file.FileName).Replace(" ", "_")}{ext}";
        var filePath = Path.Combine(receiptsDir, safeFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var relativeUrl = $"/api/expenses/proof/{safeFileName}";
        return (null, relativeUrl, file.FileName);
    }

    [HttpGet("proof/{fileName}")]
    [Authorize(Roles = "Employee,Manager,Finance,Admin")]
    public async Task<IActionResult> GetProofFile(string fileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest(new { message = "File name is required." });
        }

        var cleanName = Path.GetFileName(fileName);
        var expense = await _expenseService.GetExpenseByProofFileNameAsync(cleanName, cancellationToken);
        if (expense == null)
        {
            return NotFound(new { message = "Receipt document not found." });
        }

        if (!CanAccessExpenseProof(expense))
        {
            return Forbid();
        }

        var receiptsDir = Path.Combine(_environment.ContentRootPath, "App_Data", "Receipts");
        var filePath = Path.Combine(receiptsDir, cleanName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(new { message = "Receipt document not found." });
        }

        var ext = Path.GetExtension(cleanName).ToLowerInvariant();
        var contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return File(stream, contentType, cleanName, enableRangeProcessing: true);
    }

    private bool CanAccessExpenseProof(VoyageHub.Data.Entities.Expense expense)
    {
        if (User.IsInRole("Finance") || User.IsInRole("Admin"))
        {
            return true;
        }

        var callerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("id")?.Value;

        if (!Guid.TryParse(callerIdStr, out var callerId))
        {
            return false;
        }

        var ownerId = expense.Request?.EmployeeId;
        if (ownerId.HasValue && ownerId.Value == callerId)
        {
            return true;
        }

        if (User.IsInRole("Manager") && expense.Request?.Employee?.ManagerId == callerId)
        {
            return true;
        }

        return false;
    }
}
