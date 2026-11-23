using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Services;

namespace VoyageHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> GetEmployees(
        [FromQuery] string? search,
        [FromQuery] string? department,
        [FromQuery] string? role,
        CancellationToken cancellationToken)
    {
        var employees = await _employeeService.GetEmployeesAsync(search, department, role, cancellationToken);
        return Ok(employees);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployeeById(Guid id, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id, cancellationToken);
        if (employee == null)
        {
            return NotFound(new { message = "Employee not found." });
        }
        return Ok(employee);
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee([FromBody] CreateEmployeeDto dto, CancellationToken cancellationToken)
    {
        var (success, message, employee) = await _employeeService.CreateEmployeeAsync(dto, cancellationToken);
        if (!success)
        {
            return BadRequest(new { message });
        }
        return CreatedAtAction(nameof(GetEmployeeById), new { id = employee!.Id }, employee);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EmployeeDto>> UpdateEmployee(Guid id, [FromBody] UpdateEmployeeDto dto, CancellationToken cancellationToken)
    {
        var (success, message, employee) = await _employeeService.UpdateEmployeeAsync(id, dto, cancellationToken);
        if (!success)
        {
            return BadRequest(new { message });
        }
        return Ok(employee);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(Guid id, CancellationToken cancellationToken)
    {
        var (success, message) = await _employeeService.DeleteEmployeeAsync(id, cancellationToken);
        if (!success)
        {
            return BadRequest(new { message });
        }
        return Ok(new { success = true, message });
    }

    [HttpPost("batch-upload")]
    public async Task<ActionResult<BatchUploadResultDto>> BatchUpload(IFormFile? file, CancellationToken cancellationToken)
    {
        // Support both IFormFile binding and fallback to Request.Form.Files
        var uploadFile = file ?? (Request.HasFormContentType && Request.Form.Files.Count > 0 ? Request.Form.Files[0] : null);

        if (uploadFile == null || uploadFile.Length == 0)
        {
            return BadRequest(new { message = "Please upload a valid .csv file." });
        }

        if (!uploadFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Only .csv files are supported for batch upload." });
        }

        using var stream = uploadFile.OpenReadStream();
        var result = await _employeeService.BatchUploadCsvAsync(stream, cancellationToken);
        return Ok(result);
    }

    [HttpGet("csv-template")]
    [AllowAnonymous]
    public IActionResult DownloadCsvTemplate()
    {
        var csvBuilder = new StringBuilder();
        csvBuilder.AppendLine("Name,Email,Role,Department,Password");
        csvBuilder.AppendLine("Aarav Patel,aarav.patel@voyagehub.in,Employee,Engineering,VoyageHub@2026");
        csvBuilder.AppendLine("Priya Nair,priya.nair@voyagehub.in,Manager,Product Management,VoyageHub@2026");
        csvBuilder.AppendLine("Vikram Mehta,vikram.mehta@voyagehub.in,Finance,Finance & Accounts,VoyageHub@2026");

        var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
        return File(bytes, "text/csv", "employees_template.csv");
    }
}
