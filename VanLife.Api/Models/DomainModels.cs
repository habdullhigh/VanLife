namespace VanLife.Api.Models;

public enum VanCategory
{
    Simple,
    Luxury
}

public enum UserRole
{
    Buyer,
    Seller,
    Admin
}

public enum ReviewType
{
    Positive,
    Neutral,
    Negative
}

public class Van
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public VanCategory Category { get; set; }
    public decimal PricePerDay { get; set; }
    public string FullDescription { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public int NumberAvailable { get; set; }
    public bool IsVisible { get; set; } = true;
    public Guid SellerId { get; set; }
    public List<string> Photos { get; set; } = [];
}

public class UserAccount
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}

public class Review
{
    public Guid Id { get; set; }
    public Guid TargetUserId { get; set; }
    public ReviewType Type { get; set; }
    public int Stars { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class Transaction
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public Guid VanId { get; set; }
    public decimal Price { get; set; }
    public DateTime Date { get; set; }
}

public class ImageAsset
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

