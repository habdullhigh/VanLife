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

public class PaginationQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    // Query-string alias: ?perpage=20 will behave like ?pageSize=20
    public int PerPage { get => PageSize; set => PageSize = value; }
    public int Skip { get; set; } = 0;
}

public class VanQuery : PaginationQuery
{
    public VanCategory? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public Guid? SellerId { get; set; }
    public bool? IsVisible { get; set; }
    // Optional free-text search that will match against van name and full description
    public string? Query { get; set; }
}

public class RentRequest
{
    // Number of days user intends to rent
    [Range(1, 365)]
    public int Days { get; set; } = 1;
    // Optional destination for emergency contact purposes
    public string? Destination { get; set; }
    // Renter contact info (phone or alternative)
    [Required]
    public string Contact { get; set; } = string.Empty;
    // Payment token or method identifier (in production integrate with payment gateway)
    [Required]
    public string PaymentToken { get; set; } = string.Empty;
    // Caution fee amount provided by renter
    public decimal CautionFee { get; set; }
}

public class RentalHistoryItemDto
{
    public Guid PurchaseId { get; set; }
    public Guid VanId { get; set; }
    public Guid SellerId { get; set; }
    public DateTime PurchasedAt { get; set; }
    public int Days { get; set; }
    public decimal TotalPaid { get; set; }
    public string? Destination { get; set; }
}

public class CreateReviewRequest
{
    [Required]
    public Guid TargetUserId { get; set; }
    [Required]
    public ReviewType Type { get; set; }
    [Range(1,5)]
    public int Stars { get; set; }
    public string? Comment { get; set; }
}

public class ReviewQuery : PaginationQuery
{
    public Guid UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ReviewType? ReviewType { get; set; }
}

public class TransactionQuery : PaginationQuery
{
    public Guid? SellerId { get; set; }
    public int? Days { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}

public record PagedResult<T>(IEnumerable<T> Items, int Total, int Page, int PageSize, int Skip);

// Generic operation result
public record OperationResult(bool Success, string Message);

// Operation result that includes a newly created entity id
public record CreateResult(bool Success, string Message, Guid VanId);

// Request used by sellers to create a new van listing
public class CreateVanRequest
{
    public string Name { get; set; } = string.Empty;
    public VanCategory Category { get; set; }
    public decimal PricePerDay { get; set; }
    public string FullDescription { get; set; } = string.Empty;
    public int NumberAvailable { get; set; } = 1;
}

// Request used to update van details
public class UpdateVanRequest
{
    public string? Name { get; set; }
    public VanCategory? Category { get; set; }
    public decimal? PricePerDay { get; set; }
    public string? FullDescription { get; set; }
    public int? NumberAvailable { get; set; }
    public bool? IsVisible { get; set; }
}

// Request to update availability (quick toggle)
public class UpdateAvailabilityRequest
{
    public bool IsAvailable { get; set; }
    public int? NumberAvailable { get; set; }
}

public class SignUpRequest
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required] public string Username { get; set; } = string.Empty;
    [Required] public string IdNumber { get; set; } = string.Empty;
    [Required] public string Address { get; set; } = string.Empty;
    [Required] public string Phone { get; set; } = string.Empty;
    [Required] public string NextOfKin { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
    [Required] public string ConfirmPassword { get; set; } = string.Empty;
    [Required] public UserRole Role { get; set; }
}

public class LoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
    [Required] public UserRole Role { get; set; }
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

