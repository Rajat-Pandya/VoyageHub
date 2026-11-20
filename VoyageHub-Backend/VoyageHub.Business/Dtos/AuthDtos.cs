using System;

namespace VoyageHub.Business.Dtos;

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string Fullname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Employee";
    public string? DepartmentName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? ManagerName { get; set; }
    public string? AvatarUrl { get; set; }
    public string RegisteredAt { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserProfileDto? User { get; set; }
}

public class UpdateProfileDto
{
    public Guid UserId { get; set; }
    public string? Fullname { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? HomeCity { get; set; }
    public string? SeatPreference { get; set; }
    public string? MealPreference { get; set; }
    public string? RoomPreference { get; set; }
    public string? AirlineLoyalty { get; set; }
    public string? HotelLoyalty { get; set; }
    public bool? NotifyTripStatus { get; set; }
    public bool? NotifyExpensePayout { get; set; }
    public bool? NotifyDailyDigest { get; set; }
    public bool? NotifySmsAlerts { get; set; }
}

public class ChangePasswordDto
{
    public Guid UserId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}

public class ForgotPasswordResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ResetToken { get; set; }
    public string? ResetLink { get; set; }
}

public class VerifyResetTokenDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
