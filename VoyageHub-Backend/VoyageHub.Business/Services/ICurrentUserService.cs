using System;

namespace VoyageHub.Business.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    string? UserEmail { get; }
    string? UserRole { get; }
}
