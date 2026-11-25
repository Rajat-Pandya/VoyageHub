using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VoyageHub.Data.Entities;

namespace VoyageHub.Data.Repositories;

public class PolicyRepository : Repository<TravelPolicy>, IPolicyRepository
{
    public PolicyRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<TravelPolicy>> GetAllWithDepartmentAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Department)
            .OrderBy(p => p.Department != null ? p.Department.Name : "")
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<TravelPolicy?> GetByIdWithDepartmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Department)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Department?> GetDepartmentByNameWithPolicyAsync(string name, CancellationToken cancellationToken = default)
    {
        var lowerName = name.ToLower();
        return await Context.Departments
            .Include(d => d.Policy)
            .FirstOrDefaultAsync(d => d.Name.ToLower() == lowerName, cancellationToken);
    }

    public async Task<List<string>> GetDepartmentsWithoutPolicyAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Departments
            .AsNoTracking()
            .Where(d => d.Policy == null)
            .OrderBy(d => d.Name)
            .Select(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<TravelPolicy?> GetPolicyByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await Context.Users
            .AsNoTracking()
            .Include(u => u.Department)
            .ThenInclude(d => d!.Policy)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user?.Department?.Policy;
    }

    public async Task AddDepartmentAsync(Department department, CancellationToken cancellationToken = default)
    {
        await Context.Departments.AddAsync(department, cancellationToken);
    }
}
