using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Services;

namespace VoyageHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    [HttpGet("pending")]
    public async Task<ActionResult<ExpenseReviewSummaryDto>> GetPending(
        [FromQuery] string? category,
        [FromQuery] bool? highPriorityOnly,
        CancellationToken cancellationToken)
    {
        var result = await _expenseService.GetPendingSummaryAsync(category, highPriorityOnly, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitExpense([FromBody] SubmitExpenseDto dto, CancellationToken cancellationToken)
    {
        var (success, message, expense) = await _expenseService.SubmitExpenseAsync(dto, cancellationToken);
        if (!success)
        {
            return NotFound(new { message });
        }

        return Ok(expense);
    }

    [HttpPost("{id}/action")]
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
    public async Task<IActionResult> BatchPayout([FromBody] BatchPayoutDto? dto, CancellationToken cancellationToken)
    {
        var (success, message) = await _expenseService.BatchPayoutAsync(dto, cancellationToken);
        return Ok(new { success = true, message });
    }
}
