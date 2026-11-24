using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VoyageHub.Business.Dtos;
using VoyageHub.Data;
using VoyageHub.Data.Entities;
using VoyageHub.Data.Repositories;

namespace VoyageHub.Business.Services;

public class EmployeeService : IEmployeeService
{
    private static readonly string[] DefaultAvatars = new[]
    {
        "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?auto=format&fit=crop&w=256&q=80",
        "https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=256&q=80",
        "https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?auto=format&fit=crop&w=256&q=80",
        "https://images.unsplash.com/photo-1580489944761-15a19d654956?auto=format&fit=crop&w=256&q=80",
        "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=256&q=80"
    };

    private static readonly HashSet<string> ValidRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Employee", "Manager", "Finance", "Admin"
    };

    private readonly AppDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;

    public EmployeeService(
        AppDbContext context,
        IUserRepository userRepository,
        IAuditService auditService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _userRepository = userRepository;
        _auditService = auditService;
        _currentUserService = currentUserService;
    }

    public async Task<List<EmployeeDto>> GetEmployeesAsync(
        string? search = null,
        string? department = null,
        string? role = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .Include(u => u.Department)
            .Include(u => u.Manager)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u => u.Name.ToLower().Contains(term) || u.Email.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(department) && !department.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var deptTerm = department.Trim().ToLower();
            query = query.Where(u => u.Department != null && u.Department.Name.ToLower() == deptTerm);
        }

        if (!string.IsNullOrWhiteSpace(role) && !role.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var roleTerm = role.Trim().ToLower();
            query = query.Where(u => u.Role.ToLower() == roleTerm);
        }

        var users = await query
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);

        return users.Select(MapToDto).ToList();
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Manager)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return user != null ? MapToDto(user) : null;
    }

    public async Task<(bool Success, string Message, EmployeeDto? Employee)> CreateEmployeeAsync(
        CreateEmployeeDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return (false, "Employee data is required.", null);

        var name = dto.Name?.Trim() ?? string.Empty;
        var email = dto.Email?.Trim().ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
            return (false, "Full Name is required.", null);

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return (false, "A valid email address is required.", null);

        var existingUser = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email, cancellationToken);
        if (existingUser)
            return (false, $"An account with email '{email}' already exists.", null);

        var role = NormalizeRole(dto.Role);

        Guid? deptId = dto.DepartmentId;
        if (!deptId.HasValue && !string.IsNullOrWhiteSpace(dto.DepartmentName))
        {
            deptId = await ResolveOrCreateDepartmentIdAsync(dto.DepartmentName.Trim(), cancellationToken);
        }

        var rawPassword = string.IsNullOrWhiteSpace(dto.Password) ? "VoyageHub@2026" : dto.Password.Trim();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        var avatar = !string.IsNullOrWhiteSpace(dto.AvatarUrl)
            ? dto.AvatarUrl.Trim()
            : DefaultAvatars[Math.Abs(name.GetHashCode()) % DefaultAvatars.Length];

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            PasswordHash = passwordHash,
            Role = role,
            DepartmentId = deptId,
            ManagerId = dto.ManagerId,
            AvatarUrl = avatar,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(newUser, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            entityType: "EMPLOYEE",
            entityId: newUser.Id,
            action: "Create Employee",
            actorId: _currentUserService.UserId,
            actorName: _currentUserService.UserName ?? "System Admin",
            changeDetail: $"Created employee account '{newUser.Name}' ({newUser.Email}) with role '{newUser.Role}'",
            severity: "MEDIUM",
            cancellationToken: cancellationToken);

        var created = await GetEmployeeByIdAsync(newUser.Id, cancellationToken);
        return (true, "Employee created successfully.", created);
    }

    public async Task<(bool Success, string Message, EmployeeDto? Employee)> UpdateEmployeeAsync(
        Guid id,
        UpdateEmployeeDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return (false, "Employee data is required.", null);

        var user = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Manager)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
            return (false, "Employee not found.", null);

        var name = dto.Name?.Trim() ?? string.Empty;
        var email = dto.Email?.Trim().ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
            return (false, "Full Name is required.", null);

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return (false, "A valid email address is required.", null);

        var duplicateEmail = await _context.Users
            .AnyAsync(u => u.Id != id && u.Email.ToLower() == email, cancellationToken);
        if (duplicateEmail)
            return (false, $"Email '{email}' is already in use by another account.", null);

        var oldRole = user.Role;
        user.Name = name;
        user.Email = email;
        user.Role = NormalizeRole(dto.Role);

        if (dto.DepartmentId.HasValue)
        {
            user.DepartmentId = dto.DepartmentId.Value;
        }
        else if (!string.IsNullOrWhiteSpace(dto.DepartmentName))
        {
            user.DepartmentId = await ResolveOrCreateDepartmentIdAsync(dto.DepartmentName.Trim(), cancellationToken);
        }

        if (dto.ManagerId.HasValue && dto.ManagerId.Value != id)
        {
            user.ManagerId = dto.ManagerId.Value;
        }
        else if (!dto.ManagerId.HasValue)
        {
            user.ManagerId = null;
        }

        if (!string.IsNullOrWhiteSpace(dto.AvatarUrl))
        {
            user.AvatarUrl = dto.AvatarUrl.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password.Trim());
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            entityType: "EMPLOYEE",
            entityId: user.Id,
            action: "Update Employee",
            actorId: _currentUserService.UserId,
            actorName: _currentUserService.UserName ?? "System Admin",
            changeDetail: $"Updated employee '{user.Name}' ({user.Email}) [Role: {oldRole} → {user.Role}]",
            severity: "LOW",
            cancellationToken: cancellationToken);

        var updated = await GetEmployeeByIdAsync(user.Id, cancellationToken);
        return (true, "Employee updated successfully.", updated);
    }

    public async Task<(bool Success, string Message)> DeleteEmployeeAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.Subordinates)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
            return (false, "Employee not found.");

        if (_currentUserService.UserId.HasValue && _currentUserService.UserId.Value == id)
            return (false, "You cannot delete your own active administrator account.");

        // Clear subordinate manager references
        var subordinates = await _context.Users
            .Where(u => u.ManagerId == id)
            .ToListAsync(cancellationToken);
        foreach (var sub in subordinates)
        {
            sub.ManagerId = null;
        }

        // Clear approver references on approval steps
        var steps = await _context.ApprovalSteps
            .Where(s => s.ApproverId == id)
            .ToListAsync(cancellationToken);
        foreach (var s in steps)
        {
            s.ApproverId = null;
        }

        // Clear user references on support tickets
        var tickets = await _context.SupportTickets
            .Where(t => t.UserId == id)
            .ToListAsync(cancellationToken);
        foreach (var t in tickets)
        {
            t.UserId = null;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            entityType: "EMPLOYEE",
            entityId: id,
            action: "Delete Employee",
            actorId: _currentUserService.UserId,
            actorName: _currentUserService.UserName ?? "System Admin",
            changeDetail: $"Deleted employee record for '{user.Name}' ({user.Email})",
            severity: "HIGH",
            cancellationToken: cancellationToken);

        return (true, $"Employee '{user.Name}' deleted successfully.");
    }

    public async Task<BatchUploadResultDto> BatchUploadCsvAsync(
        Stream csvStream,
        CancellationToken cancellationToken = default)
    {
        var result = new BatchUploadResultDto();
        using var reader = new StreamReader(csvStream);

        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            result.Errors.Add("CSV file is empty.");
            return result;
        }

        var delimiter = headerLine.Contains(';') ? ';' : ',';
        var headers = ParseCsvRow(headerLine, delimiter)
            .Select(h => h.Trim().ToLowerInvariant())
            .ToList();

        int nameCol = headers.FindIndex(h => h == "name" || h == "fullname" || h == "full name");
        int emailCol = headers.FindIndex(h => h == "email" || h == "emailaddress" || h == "work email");
        int roleCol = headers.FindIndex(h => h == "role" || h == "designation");
        int deptCol = headers.FindIndex(h => h == "department" || h == "departmentname" || h == "dept");
        int pwdCol = headers.FindIndex(h => h == "password" || h == "initial password");
        int mgrCol = headers.FindIndex(h => h == "manager" || h == "manageremail" || h == "manager name");

        if (nameCol < 0 || emailCol < 0)
        {
            result.Errors.Add("CSV must contain at least 'Name' and 'Email' columns in the header.");
            return result;
        }

        var existingEmails = (await _context.Users
            .Select(u => u.Email.ToLower())
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var existingDepartments = await _context.Departments
            .ToListAsync(cancellationToken);

        int lineNumber = 1;
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            result.TotalProcessed++;
            var values = ParseCsvRow(line, delimiter);

            var name = nameCol < values.Count ? values[nameCol].Trim() : string.Empty;
            var email = emailCol < values.Count ? values[emailCol].Trim().ToLowerInvariant() : string.Empty;

            if (string.IsNullOrWhiteSpace(name))
            {
                result.FailureCount++;
                result.Errors.Add($"Row {lineNumber}: Employee name is missing.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            {
                result.FailureCount++;
                result.Errors.Add($"Row {lineNumber}: Invalid email '{email}'.");
                continue;
            }

            if (existingEmails.Contains(email))
            {
                result.FailureCount++;
                result.Errors.Add($"Row {lineNumber}: Email '{email}' already exists.");
                continue;
            }

            var roleStr = (roleCol >= 0 && roleCol < values.Count) ? values[roleCol] : "Employee";
            var role = NormalizeRole(roleStr);

            Guid? deptId = null;
            if (deptCol >= 0 && deptCol < values.Count && !string.IsNullOrWhiteSpace(values[deptCol]))
            {
                var deptName = values[deptCol].Trim();
                var matchDept = existingDepartments.FirstOrDefault(d => d.Name.Equals(deptName, StringComparison.OrdinalIgnoreCase));
                if (matchDept == null)
                {
                    matchDept = new Department { Id = Guid.NewGuid(), Name = deptName };
                    await _context.Departments.AddAsync(matchDept, cancellationToken);
                    existingDepartments.Add(matchDept);
                }
                deptId = matchDept.Id;
            }

            Guid? mgrId = null;
            if (mgrCol >= 0 && mgrCol < values.Count && !string.IsNullOrWhiteSpace(values[mgrCol]))
            {
                var mgrVal = values[mgrCol].Trim().ToLowerInvariant();
                var mgrUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == mgrVal || u.Name.ToLower() == mgrVal, cancellationToken);
                if (mgrUser != null)
                {
                    mgrId = mgrUser.Id;
                }
            }

            var rawPwd = (pwdCol >= 0 && pwdCol < values.Count && !string.IsNullOrWhiteSpace(values[pwdCol]))
                ? values[pwdCol].Trim()
                : "VoyageHub@2026";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPwd);

            var avatar = DefaultAvatars[Math.Abs(name.GetHashCode()) % DefaultAvatars.Length];

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email,
                PasswordHash = passwordHash,
                Role = role,
                DepartmentId = deptId,
                ManagerId = mgrId,
                AvatarUrl = avatar,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(newUser, cancellationToken);
            existingEmails.Add(email);
            result.SuccessCount++;
            result.CreatedEmployees.Add(new EmployeeDto
            {
                Id = newUser.Id,
                Name = newUser.Name,
                Email = newUser.Email,
                Role = newUser.Role,
                DepartmentId = newUser.DepartmentId,
                DepartmentName = deptCol >= 0 && deptCol < values.Count ? values[deptCol].Trim() : null,
                AvatarUrl = newUser.AvatarUrl,
                CreatedAt = newUser.CreatedAt
            });
        }

        if (result.SuccessCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                entityType: "EMPLOYEE",
                entityId: _currentUserService.UserId ?? Guid.Empty,
                action: "Batch CSV Upload",
                actorId: _currentUserService.UserId,
                actorName: _currentUserService.UserName ?? "System Admin",
                changeDetail: $"Imported {result.SuccessCount} employees from CSV batch upload ({result.FailureCount} skipped/failed)",
                severity: "MEDIUM",
                cancellationToken: cancellationToken);
        }

        return result;
    }

    private async Task<Guid?> ResolveOrCreateDepartmentIdAsync(string departmentName, CancellationToken cancellationToken)
    {
        var lower = departmentName.ToLower();
        var dept = await _context.Departments
            .FirstOrDefaultAsync(d => d.Name.ToLower() == lower, cancellationToken);

        if (dept == null)
        {
            dept = new Department
            {
                Id = Guid.NewGuid(),
                Name = departmentName
            };
            await _context.Departments.AddAsync(dept, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return dept.Id;
    }

    private static string NormalizeRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role)) return "Employee";
        var trimmed = role.Trim();
        if (ValidRoles.TryGetValue(trimmed, out var valid))
        {
            return valid;
        }
        return "Employee";
    }

    private static EmployeeDto MapToDto(User u)
    {
        return new EmployeeDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role,
            DepartmentId = u.DepartmentId,
            DepartmentName = u.Department != null ? u.Department.Name : "General",
            ManagerId = u.ManagerId,
            ManagerName = u.Manager != null ? u.Manager.Name : null,
            AvatarUrl = u.AvatarUrl,
            CreatedAt = u.CreatedAt
        };
    }

    private static List<string> ParseCsvRow(string row, char delimiter)
    {
        var list = new List<string>();
        var pattern = string.Format(@"(?<=^|{0})(\""(?:[^\""]|\""\"")*\""|[^\""{0}]*)", Regex.Escape(delimiter.ToString()));
        var matches = Regex.Matches(row, pattern, RegexOptions.None, TimeSpan.FromSeconds(1));

        foreach (Match match in matches)
        {
            var val = match.Value;
            if (val.StartsWith('\"') && val.EndsWith('\"') && val.Length >= 2)
            {
                val = val.Substring(1, val.Length - 2).Replace("\"\"", "\"");
            }
            list.Add(val);
        }

        return list;
    }
}
