using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Business.Dtos;

namespace VoyageHub.Business.Services;

public interface IPolicyService
{
    Task<List<PolicyConfigDto>> GetPoliciesAsync(CancellationToken cancellationToken = default);
    Task<PolicyConfigDto?> GetMyPolicyAsync(CancellationToken cancellationToken = default);
    Task<List<string>> GetAvailableDepartmentsAsync(CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> UpdatePolicyAsync(Guid id, UpdatePolicyDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message, PolicyConfigDto? Policy)> CreatePolicyAsync(CreatePolicyDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> DeletePolicyAsync(Guid id, CancellationToken cancellationToken = default);
}
