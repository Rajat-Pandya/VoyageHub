using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Business.Dtos;
using VoyageHub.Data.Entities;
using VoyageHub.Data.Repositories;

namespace VoyageHub.Business.Services;

public class PolicyService : IPolicyService
{
    private readonly IPolicyRepository _policyRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;

    public PolicyService(
        IPolicyRepository policyRepository,
        IAuditService auditService,
        ICurrentUserService currentUserService)
    {
        _policyRepository = policyRepository;
        _auditService = auditService;
        _currentUserService = currentUserService;
    }

    public async Task<List<PolicyConfigDto>> GetPoliciesAsync(CancellationToken cancellationToken = default)
    {
        var policies = await _policyRepository.GetAllWithDepartmentAsync(cancellationToken);

        return policies.Select(p => new PolicyConfigDto
        {
            Id = p.Id,
            DepartmentId = p.DepartmentId,
            DepartmentName = p.Department != null ? p.Department.Name : "General",
            IntlBudgetLimit = p.IntlBudgetLimit,
            DomesticLimit = p.DomesticLimit,
            AllowedClass = p.AllowedClass,
            HardEnforcement = p.HardEnforcement
        }).ToList();
    }

    public async Task<(bool Success, string Message)> UpdatePolicyAsync(Guid id, UpdatePolicyDto dto, CancellationToken cancellationToken = default)
    {
        var policy = await _policyRepository.GetByIdWithDepartmentAsync(id, cancellationToken);
        if (policy == null)
        {
            return (false, "Policy not found.");
        }

        var oldLimit = policy.IntlBudgetLimit;
        policy.IntlBudgetLimit = dto.IntlBudgetLimit;
        policy.DomesticLimit = dto.DomesticLimit;
        policy.AllowedClass = dto.AllowedClass;
        policy.HardEnforcement = dto.HardEnforcement;

        await _auditService.LogAsync(
            entityType: "POLICY",
            entityId: policy.Id,
            action: "Update Limit",
            actorId: _currentUserService.UserId,
            actorName: _currentUserService.UserName ?? "System Admin",
            changeDetail: $"{policy.Department?.Name} Intl. Limit: ₹{oldLimit:N0} → ₹{dto.IntlBudgetLimit:N0}",
            severity: "LOW",
            cancellationToken: cancellationToken);

        await _policyRepository.SaveChangesAsync(cancellationToken);
        return (true, "Policy updated successfully.");
    }

    public async Task<(bool Success, string Message, PolicyConfigDto? Policy)> CreatePolicyAsync(CreatePolicyDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.DepartmentName))
        {
            return (false, "Department name is required.", null);
        }

        var deptName = dto.DepartmentName.Trim();
        var department = await _policyRepository.GetDepartmentByNameWithPolicyAsync(deptName, cancellationToken);
        if (department == null)
        {
            department = new Department { Name = deptName };
            await _policyRepository.AddDepartmentAsync(department, cancellationToken);
            await _policyRepository.SaveChangesAsync(cancellationToken);
        }

        if (department.Policy != null)
        {
            return (false, "A policy already exists for this department.", null);
        }

        var policy = new TravelPolicy
        {
            DepartmentId = department.Id,
            IntlBudgetLimit = dto.IntlBudgetLimit,
            DomesticLimit = dto.DomesticLimit,
            AllowedClass = string.IsNullOrWhiteSpace(dto.AllowedClass) ? "ECONOMY+" : dto.AllowedClass,
            HardEnforcement = dto.HardEnforcement
        };

        await _policyRepository.AddAsync(policy, cancellationToken);

        await _auditService.LogAsync(
            entityType: "POLICY",
            entityId: policy.Id,
            action: "Create Policy",
            actorId: _currentUserService.UserId,
            actorName: _currentUserService.UserName ?? "System Admin",
            changeDetail: $"Created policy for {deptName} (Domestic: ₹{dto.DomesticLimit:N0}, Intl: ₹{dto.IntlBudgetLimit:N0})",
            severity: "LOW",
            cancellationToken: cancellationToken);

        await _policyRepository.SaveChangesAsync(cancellationToken);

        var resultDto = new PolicyConfigDto
        {
            Id = policy.Id,
            DepartmentId = department.Id,
            DepartmentName = department.Name,
            IntlBudgetLimit = policy.IntlBudgetLimit,
            DomesticLimit = policy.DomesticLimit,
            AllowedClass = policy.AllowedClass,
            HardEnforcement = policy.HardEnforcement
        };

        return (true, string.Empty, resultDto);
    }

    public async Task<(bool Success, string Message)> DeletePolicyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var policy = await _policyRepository.GetByIdWithDepartmentAsync(id, cancellationToken);
        if (policy == null)
        {
            return (false, "Policy not found.");
        }

        _policyRepository.Delete(policy);

        await _auditService.LogAsync(
            entityType: "POLICY",
            entityId: policy.Id,
            action: "Delete Policy",
            actorId: _currentUserService.UserId,
            actorName: _currentUserService.UserName ?? "System Admin",
            changeDetail: $"Deleted policy for {policy.Department?.Name}",
            severity: "MEDIUM",
            cancellationToken: cancellationToken);

        await _policyRepository.SaveChangesAsync(cancellationToken);
        return (true, "Policy deleted successfully.");
    }

    public async Task<List<string>> GetAvailableDepartmentsAsync(CancellationToken cancellationToken = default)
    {
        return await _policyRepository.GetDepartmentsWithoutPolicyAsync(cancellationToken);
    }
}
