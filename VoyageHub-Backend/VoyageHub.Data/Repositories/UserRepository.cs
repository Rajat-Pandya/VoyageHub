using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VoyageHub.Data.Entities;

namespace VoyageHub.Data.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await DbSet
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);
    }

    public async Task<User?> GetByEmailWithDetailsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await DbSet
            .Include(u => u.Department)
            .Include(u => u.Manager)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);
    }

    public async Task<User?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.Department)
            .Include(u => u.Manager)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<List<User>> GetAllPersonasAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.Department)
            .OrderBy(u => u.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<User>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.Department)
            .Include(u => u.Manager)
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<User>> GetSubordinatesAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(u => u.ManagerId == managerId)
            .ToListAsync(cancellationToken);
    }
}
