using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Business.Dtos;

namespace VoyageHub.Business.Services;

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetEmployeesAsync(string? search = null, string? department = null, string? role = null, CancellationToken cancellationToken = default);
    Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message, EmployeeDto? Employee)> CreateEmployeeAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message, EmployeeDto? Employee)> UpdateEmployeeAsync(Guid id, UpdateEmployeeDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> DeleteEmployeeAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BatchUploadResultDto> BatchUploadCsvAsync(Stream csvStream, CancellationToken cancellationToken = default);
}
