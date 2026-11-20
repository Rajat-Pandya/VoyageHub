using VoyageHub.Data.Entities;

namespace VoyageHub.Business.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
