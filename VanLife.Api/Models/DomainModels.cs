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
    public Seller? Seller { get; set; }
    public List<ImageAsset> Photos { get; set; } = new();
}

public class UserAccount
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    // Username for login/display
    public string Username { get; set; } = string.Empty;
    // National ID (NIN/BVN) or other government id
    public string IdNumber { get; set; } = string.Empty;
    // Home address
    public string Address { get; set; } = string.Empty;
    // Primary phone number
    public string Phone { get; set; } = string.Empty;
    // Next of kin contact for emergencies
    public string NextOfKin { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    // UserAccount no longer directly owns Vans; separate Seller table is used for seller-specific data
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
    public Guid? VanId { get; set; }
    public Van? Van { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

// Separate seller table containing minimal seller info
public class Seller
{
    public Guid SellerId { get; set; }
    public string Username { get; set; } = string.Empty;
    // Seller's vans
    public List<Van> Vans { get; set; } = new();
    // Rentals associated with this seller
    public List<Rental> Rentals { get; set; } = new();
}

// Separate buyer table containing minimal buyer info
public class Buyer
{
    public Guid BuyerId { get; set; }
    public string Username { get; set; } = string.Empty;
    // Rentals associated with this buyer
    public List<Rental> Rentals { get; set; } = new();
}

// Rental table that links sellers, buyers and vans
public class Rental
{
    public Guid PurchaseId { get; set; }
    public Guid SellerId { get; set; }
    public Seller? Seller { get; set; }
    public Guid BuyerId { get; set; }
    public Buyer? Buyer { get; set; }
    public Guid VanId { get; set; }
    public Van? Van { get; set; }
    // When the rental was created / purchased
    public DateTime PurchasedAt { get; set; }

    // Expanded rental details
    // Number of days the van was rented for
    public int Days { get; set; }

    // Optional destination provided by renter
    public string? Destination { get; set; }

    // Contact information used for this rental
    public string? Contact { get; set; }

    // Caution / deposit fee provided by renter
    public decimal CautionFee { get; set; }

    // Total amount paid for this rental (rent + caution)
    public decimal TotalPaid { get; set; }

    // Rental period start and end (inclusive)
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

