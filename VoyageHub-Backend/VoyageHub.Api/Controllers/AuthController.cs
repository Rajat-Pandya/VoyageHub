using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VoyageHub.Business.Dtos;
using VoyageHub.Business.Services;

namespace VoyageHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public AuthController(
        IAuthService authService,
        ILogger<AuthController> logger,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _authService = authService;
        _logger = logger;
        _environment = environment;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var (statusCode, response) = await _authService.LoginAsync(dto, cancellationToken);
            return StatusCode(statusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new AuthResponseDto
            {
                Success = false,
                Message = "An unexpected error occurred. Please try again later."
            });
        }
    }

    [HttpGet("personas")]
    [AllowAnonymous]
    public async Task<ActionResult<List<UserProfileDto>>> GetQuickPersonas(CancellationToken cancellationToken)
    {
        var enabled = _configuration.GetValue<bool>("Features:EnableDemoPersonas", _environment.IsDevelopment());
        if (!enabled)
        {
            return NotFound();
        }

        var users = await _authService.GetPersonasAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<AuthResponseDto>> UpdateProfile([FromBody] UpdateProfileDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var (statusCode, response) = await _authService.UpdateProfileAsync(dto, cancellationToken);
            return StatusCode(statusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return StatusCode(500, new AuthResponseDto
            {
                Success = false,
                Message = "An unexpected error occurred. Please try again later."
            });
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<AuthResponseDto>> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var (statusCode, response) = await _authService.ChangePasswordAsync(dto, cancellationToken);
            return StatusCode(statusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(500, new AuthResponseDto
            {
                Success = false,
                Message = "An unexpected error occurred. Please try again later."
            });
        }
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ForgotPasswordResponseDto>> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var frontendBaseUrl = _configuration["FrontendUrl"];
            var (statusCode, response) = await _authService.ForgotPasswordAsync(
                dto,
                _environment.IsDevelopment(),
                frontendBaseUrl,
                cancellationToken);

            return StatusCode(statusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password request");
            return StatusCode(500, new ForgotPasswordResponseDto
            {
                Success = false,
                Message = "An unexpected error occurred. Please try again later."
            });
        }
    }

    [HttpPost("verify-reset-token")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> VerifyResetToken([FromBody] VerifyResetTokenDto dto, CancellationToken cancellationToken)
    {
        var (statusCode, response) = await _authService.VerifyResetTokenAsync(dto, cancellationToken);
        return StatusCode(statusCode, response);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken cancellationToken)
    {
        var (statusCode, response) = await _authService.ResetPasswordAsync(dto, cancellationToken);
        return StatusCode(statusCode, response);
    }
}
