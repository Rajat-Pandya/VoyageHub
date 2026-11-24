using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        CancellationToken cancellationToken)
    {
        var result = await _expenseService.GetPendingSummaryAsync(category, highPriorityOnly, cancellationToken);
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
    public IActionResult GetProofFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest(new { message = "File name is required." });
        }

        var cleanName = Path.GetFileName(fileName);
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
}
