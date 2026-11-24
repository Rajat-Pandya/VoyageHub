using System;
using System.Collections.Generic;

namespace VoyageHub.Business.Dtos;

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Employee";
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateEmployeeDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string Role { get; set; } = "Employee";
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? ManagerId { get; set; }
}

public class UpdateEmployeeDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string Role { get; set; } = "Employee";
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? ManagerId { get; set; }
}

public class BatchUploadResultDto
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<EmployeeDto> CreatedEmployees { get; set; } = new();
}
