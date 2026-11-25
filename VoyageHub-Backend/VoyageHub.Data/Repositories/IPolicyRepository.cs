using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Data.Entities;

namespace VoyageHub.Data.Repositories;

public interface IPolicyRepository : IRepository<TravelPolicy>
{
    Task<List<TravelPolicy>> GetAllWithDepartmentAsync(CancellationToken cancellationToken = default);
    Task<List<string>> GetDepartmentsWithoutPolicyAsync(CancellationToken cancellationToken = default);
    Task<TravelPolicy?> GetByIdWithDepartmentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Department?> GetDepartmentByNameWithPolicyAsync(string name, CancellationToken cancellationToken = default);
    Task AddDepartmentAsync(Department department, CancellationToken cancellationToken = default);
}
