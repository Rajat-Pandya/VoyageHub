using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VoyageHub.Api.Controllers;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Services;
using Xunit;

namespace VoyageHub.Tests;

public class EmployeesControllerTests
{
    private readonly Mock<IEmployeeService> _employeeServiceMock;
    private readonly EmployeesController _controller;

    public EmployeesControllerTests()
    {
        _employeeServiceMock = new Mock<IEmployeeService>();
        _controller = new EmployeesController(_employeeServiceMock.Object);
    }

    [Fact]
    public async Task GetEmployees_ReturnsOkWithList()
    {
        var list = new List<EmployeeDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Employee 1", Email = "emp1@example.com", Role = "Employee" }
        };
        _employeeServiceMock
            .Setup(s => s.GetEmployeesAsync(null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var result = await _controller.GetEmployees(null, null, null, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(list, okResult.Value);
    }

    [Fact]
    public async Task GetEmployeeById_Found_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var emp = new EmployeeDto { Id = id, Name = "Found", Email = "found@example.com", Role = "Manager" };
        _employeeServiceMock
            .Setup(s => s.GetEmployeeByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emp);

        var result = await _controller.GetEmployeeById(id, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(emp, okResult.Value);
    }

    [Fact]
    public async Task GetEmployeeById_NotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _employeeServiceMock
            .Setup(s => s.GetEmployeeByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeDto?)null);

        var result = await _controller.GetEmployeeById(id, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateEmployee_ValidDto_ReturnsCreatedAtAction()
    {
        var dto = new CreateEmployeeDto { Name = "New", Email = "new@example.com", Role = "Employee" };
        var created = new EmployeeDto { Id = Guid.NewGuid(), Name = "New", Email = "new@example.com", Role = "Employee" };

        _employeeServiceMock
            .Setup(s => s.CreateEmployeeAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "Created", created));

        var result = await _controller.CreateEmployee(dto, CancellationToken.None);

        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(created, createdAtResult.Value);
    }

    [Fact]
    public async Task BatchUpload_NullFile_ReturnsBadRequest()
    {
        var result = await _controller.BatchUpload(null, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task BatchUpload_ValidCsv_ReturnsOkResult()
    {
        var csvBytes = Encoding.UTF8.GetBytes("Name,Email,Role,Department\nTest,test@example.com,Employee,IT");
        var formFile = new FormFile(new MemoryStream(csvBytes), 0, csvBytes.Length, "file", "employees.csv");

        var batchResult = new BatchUploadResultDto
        {
            TotalProcessed = 1,
            SuccessCount = 1,
            FailureCount = 0
        };

        _employeeServiceMock
            .Setup(s => s.BatchUploadCsvAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(batchResult);

        var result = await _controller.BatchUpload(formFile, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(batchResult, okResult.Value);
    }

    [Fact]
    public void DownloadCsvTemplate_ReturnsFileContentResult()
    {
        var result = _controller.DownloadCsvTemplate();

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("text/csv", fileResult.ContentType);
        Assert.Equal("employees_template.csv", fileResult.FileDownloadName);
    }
}
