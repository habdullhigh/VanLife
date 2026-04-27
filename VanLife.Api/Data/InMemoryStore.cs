using VanLife.Api.Models;

namespace VanLife.Api.Data;

public class InMemoryStore
{
    public List<Van> Vans { get; } = [];
    public List<UserAccount> Users { get; } = [];
    public List<Review> Reviews { get; } = [];
    public List<Transaction> Transactions { get; } = [];
    public List<ImageAsset> Images { get; } = [];

    public InMemoryStore()
    {
        var sellerId = Guid.Parse("2cb91a52-42b5-4f59-9f3a-a96925f90488");
        var buyerId = Guid.Parse("73a10d33-7da4-4f0a-b58d-0d0efc5e70e3");
        var adminId = Guid.Parse("f69bfdb3-8b25-4d8f-9d59-ee0ef03ef6a5");

        Users.AddRange([
            new UserAccount { Id = sellerId, FirstName = "Sarah", LastName = "Host", Email = "seller@vanlife.com", Password = "Seller123!", Role = UserRole.Seller },
            new UserAccount { Id = buyerId, FirstName = "Ben", LastName = "Traveler", Email = "buyer@vanlife.com", Password = "Buyer123!", Role = UserRole.Buyer },
            new UserAccount { Id = adminId, FirstName = "Ava", LastName = "Owner", Email = "admin@vanlife.com", Password = "Admin123!", Role = UserRole.Admin }
        ]);

        var van1 = new Van
        {
            Id = Guid.NewGuid(),
            Name = "Road Explorer",
            Category = VanCategory.Simple,
            PricePerDay = 75,
            FullDescription = "Easy-to-drive van with a basic kitchen and foldable bed.",
            IsAvailable = true,
            NumberAvailable = 4,
            IsVisible = true,
            SellerId = sellerId,
            Photos = ["https://cdn.vanlife.fake/road-explorer-1.jpg"]
        };

        var van2 = new Van
        {
            Id = Guid.NewGuid(),
            Name = "Luxury Nomad",
            Category = VanCategory.Luxury,
            PricePerDay = 180,
            FullDescription = "Premium van with solar panels, shower, and queen bed.",
            IsAvailable = true,
            NumberAvailable = 2,
            IsVisible = true,
            SellerId = sellerId,
            Photos = ["https://cdn.vanlife.fake/luxury-nomad-1.jpg", "https://cdn.vanlife.fake/luxury-nomad-2.jpg"]
        };

        Vans.AddRange([van1, van2]);

        Transactions.AddRange([
            new Transaction { Id = Guid.NewGuid(), SellerId = sellerId, VanId = van1.Id, Price = 320, Date = DateTime.UtcNow.AddDays(-40) },
            new Transaction { Id = Guid.NewGuid(), SellerId = sellerId, VanId = van1.Id, Price = 150, Date = DateTime.UtcNow.AddDays(-15) },
            new Transaction { Id = Guid.NewGuid(), SellerId = sellerId, VanId = van2.Id, Price = 760, Date = DateTime.UtcNow.AddDays(-8) },
            new Transaction { Id = Guid.NewGuid(), SellerId = sellerId, VanId = van2.Id, Price = 540, Date = DateTime.UtcNow.AddDays(-2) }
        ]);

        Reviews.AddRange([
            new Review { Id = Guid.NewGuid(), TargetUserId = sellerId, Type = ReviewType.Positive, Stars = 5, Comment = "Amazing service", Date = DateTime.UtcNow.AddDays(-20) },
            new Review { Id = Guid.NewGuid(), TargetUserId = sellerId, Type = ReviewType.Neutral, Stars = 3, Comment = "Good but can improve", Date = DateTime.UtcNow.AddDays(-12) },
            new Review { Id = Guid.NewGuid(), TargetUserId = buyerId, Type = ReviewType.Positive, Stars = 4, Comment = "Great renter", Date = DateTime.UtcNow.AddDays(-5) },
            new Review { Id = Guid.NewGuid(), TargetUserId = buyerId, Type = ReviewType.Negative, Stars = 2, Comment = "Late pickup", Date = DateTime.UtcNow.AddDays(-1) }
        ]);
    }
}

