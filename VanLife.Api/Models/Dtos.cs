using System.ComponentModel.DataAnnotations;

namespace VanLife.Api.Models;

public record VanListItemDto(Guid Id, string Name, VanCategory Category, decimal PricePerDay);

public record VanDetailsDto(
    Guid Id,
    string Name,
    decimal PricePerDay,
    string FullDescription,
    bool Availability,
    int NumberAvailable
);

public record SellerVanDetailsDto(
    Guid Id,
    string Name,
    VanCategory Category,
    string FullDescription,
    bool Visibility,
    decimal Pricing,
    List<string> Photos
);

public class SignUpRequest
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
    [Required] public string ConfirmPassword { get; set; } = string.Empty;
    [Required] public UserRole Role { get; set; }
}

public class LoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string NewPassword { get; set; } = string.Empty;
    [Required] public string ConfirmPassword { get; set; } = string.Empty;
}

