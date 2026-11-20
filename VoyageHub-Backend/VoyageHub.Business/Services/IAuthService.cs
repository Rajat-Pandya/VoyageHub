using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Business.Dtos;

namespace VoyageHub.Business.Services;

public interface IAuthService
{
    Task<(int StatusCode, AuthResponseDto Response)> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<List<UserProfileDto>> GetPersonasAsync(CancellationToken cancellationToken = default);
    Task<(int StatusCode, AuthResponseDto Response)> UpdateProfileAsync(UpdateProfileDto dto, CancellationToken cancellationToken = default);
    Task<(int StatusCode, AuthResponseDto Response)> ChangePasswordAsync(ChangePasswordDto dto, CancellationToken cancellationToken = default);
    Task<(int StatusCode, ForgotPasswordResponseDto Response)> ForgotPasswordAsync(ForgotPasswordDto dto, bool isDevelopment, string? frontendBaseUrl = null, CancellationToken cancellationToken = default);
    Task<(int StatusCode, AuthResponseDto Response)> VerifyResetTokenAsync(VerifyResetTokenDto dto, CancellationToken cancellationToken = default);
    Task<(int StatusCode, AuthResponseDto Response)> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default);
}
