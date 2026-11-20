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
public class PoliciesController : ControllerBase
{
    private readonly IPolicyService _policyService;

    public PoliciesController(IPolicyService policyService)
    {
        _policyService = policyService;
    }

    [HttpGet]
    public async Task<ActionResult<List<PolicyConfigDto>>> GetPolicies(CancellationToken cancellationToken)
    {
        var policies = await _policyService.GetPoliciesAsync(cancellationToken);
        return Ok(policies);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePolicy(Guid id, [FromBody] UpdatePolicyDto dto, CancellationToken cancellationToken)
    {
        var (success, message) = await _policyService.UpdatePolicyAsync(id, dto, cancellationToken);
        if (!success)
        {
            return NotFound(new { message });
        }

        return Ok(new { success = true, message });
    }

    [HttpPost]
    public async Task<ActionResult<PolicyConfigDto>> CreatePolicy([FromBody] CreatePolicyDto dto, CancellationToken cancellationToken)
    {
        var (success, message, policy) = await _policyService.CreatePolicyAsync(dto, cancellationToken);
        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(policy);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePolicy(Guid id, CancellationToken cancellationToken)
    {
        var (success, message) = await _policyService.DeletePolicyAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound(new { message });
        }

        return Ok(new { success = true, message });
    }
}
