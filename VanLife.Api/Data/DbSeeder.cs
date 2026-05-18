using Microsoft.EntityFrameworkCore;
using VanLife.Api.Models;
using VanLife.Api.Services;

namespace VanLife.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        //await db.Database.EnsureCreatedAsync();

        if (await db.Users.AnyAsync())
        {
            return;
        }

        var sellerId = Guid.Parse("2cb91a52-42b5-4f59-9f3a-a96925f90488");
        var buyerId = Guid.Parse("73a10d33-7da4-4f0a-b58d-0d0efc5e70e3");
        db.Users.AddRange(
            new UserAccount { Id = sellerId, FirstName = "Sarah", LastName = "Host", Username = "sarahhost", IdNumber = "NIN12345", Address = "123 Caravan Lane", Phone = "555-0001", NextOfKin = "John Host", Email = "seller@vanlife.com", Password = PasswordHasher.Hash("Seller123!"), Role = UserRole.Seller },
            new UserAccount { Id = buyerId, FirstName = "Ben", LastName = "Traveler", Username = "bentravels", IdNumber = "NIN67890", Address = "456 Road St", Phone = "555-0002", NextOfKin = "Sara Traveler", Email = "buyer@vanlife.com", Password = PasswordHasher.Hash("Buyer123!"), Role = UserRole.Buyer }
        );

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
            SellerId = sellerId
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
            SellerId = sellerId
        };

        db.Vans.AddRange(van1, van2);

        // Seed seller and buyer tables for separate minimal representations
        var sellerEntry = new Seller { SellerId = sellerId, Username = "sarahhost" };
        var buyerEntry = new Buyer { BuyerId = buyerId, Username = "bentravels" };
        db.Sellers.AddRange(sellerEntry);
        db.Buyers.AddRange(buyerEntry);

        db.Images.AddRange(
            new ImageAsset { Id = Guid.NewGuid(), VanId = van1.Id, FileName = "road-explorer-1.jpg", Url = "https://cdn.vanlife.fake/road-explorer-1.jpg", UploadedAt = DateTime.UtcNow },
            new ImageAsset { Id = Guid.NewGuid(), VanId = van2.Id, FileName = "luxury-nomad-1.jpg", Url = "https://cdn.vanlife.fake/luxury-nomad-1.jpg", UploadedAt = DateTime.UtcNow },
            new ImageAsset { Id = Guid.NewGuid(), VanId = van2.Id, FileName = "luxury-nomad-2.jpg", Url = "https://cdn.vanlife.fake/luxury-nomad-2.jpg", UploadedAt = DateTime.UtcNow }
        );

        // Seed a sample rental linking seller, buyer and van
        db.Rentals.Add(new Rental
        {
            PurchaseId = Guid.NewGuid(),
            SellerId = sellerId,
            BuyerId = buyerId,
            VanId = van1.Id,
            PurchasedAt = DateTime.UtcNow.AddDays(-10),
            Days = 3,
            Destination = "Lagos",
            Contact = "555-0002",
            CautionFee = 50,
            TotalPaid = 230
        });

        db.Transactions.AddRange(
            new Transaction { Id = Guid.NewGuid(), SellerId = sellerId, VanId = van1.Id, Price = 320, Date = DateTime.UtcNow.AddDays(-40) },
            new Transaction { Id = Guid.NewGuid(), SellerId = sellerId, VanId = van1.Id, Price = 150, Date = DateTime.UtcNow.AddDays(-15) },
            new Transaction { Id = Guid.NewGuid(), SellerId = sellerId, VanId = van2.Id, Price = 760, Date = DateTime.UtcNow.AddDays(-8) },
            new Transaction { Id = Guid.NewGuid(), SellerId = sellerId, VanId = van2.Id, Price = 540, Date = DateTime.UtcNow.AddDays(-2) }
        );

        db.Reviews.AddRange(
            new Review { Id = Guid.NewGuid(), TargetUserId = sellerId, Type = ReviewType.Positive, Stars = 5, Comment = "Amazing service", Date = DateTime.UtcNow.AddDays(-20) },
            new Review { Id = Guid.NewGuid(), TargetUserId = sellerId, Type = ReviewType.Neutral, Stars = 3, Comment = "Good but can improve", Date = DateTime.UtcNow.AddDays(-12) },
            new Review { Id = Guid.NewGuid(), TargetUserId = buyerId, Type = ReviewType.Positive, Stars = 4, Comment = "Great renter", Date = DateTime.UtcNow.AddDays(-5) },
            new Review { Id = Guid.NewGuid(), TargetUserId = buyerId, Type = ReviewType.Negative, Stars = 2, Comment = "Late pickup", Date = DateTime.UtcNow.AddDays(-1) }
        );

        await db.SaveChangesAsync();
    }

    public static async Task EnsureExistingUsersAsync(AppDbContext db)
    {
        var users = await db.Users.ToListAsync();
        if (users.Count == 0) return;

        var changed = false;
        foreach (var user in users)
        {
            if (!user.Password.StartsWith("$2"))
            {
                user.Password = PasswordHasher.Hash(user.Password);
                changed = true;
            }

            if (user.Role == UserRole.Seller && !await db.Sellers.AnyAsync(s => s.SellerId == user.Id))
            {
                db.Sellers.Add(new Seller { SellerId = user.Id, Username = user.Username });
                changed = true;
            }

            if (user.Role == UserRole.Buyer && !await db.Buyers.AnyAsync(b => b.BuyerId == user.Id))
            {
                db.Buyers.Add(new Buyer { BuyerId = user.Id, Username = user.Username });
                changed = true;
            }
        }

        if (changed)
        {
            await db.SaveChangesAsync();
        }
    }
}
