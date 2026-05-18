using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using VanLife.Api.Data;
using VanLife.Api.Services;
using VanLife.Api.Models;

public class RentalTests
{
    [Fact]
    public async Task RentVan_CreatesRental_And_UpdatesAvailability()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("testdb1").Options;
        await using var db = new AppDbContext(options);
        var payment = new PaymentService();
        var vanService = new VanService(db, payment);

        var sellerId = Guid.NewGuid();
        var buyerId = Guid.NewGuid();
        db.Users.Add(new UserAccount { Id = sellerId, Email = "s@e.com", FirstName = "S", LastName = "L", IdNumber = "id", Address = "a", Phone = "p", NextOfKin = "n", Password = "pw", Role = UserRole.Seller, Username = "s" });
        db.Users.Add(new UserAccount { Id = buyerId, Email = "b@e.com", FirstName = "B", LastName = "L", IdNumber = "id2", Address = "a2", Phone = "p2", NextOfKin = "n2", Password = "pw2", Role = UserRole.Buyer, Username = "b" });
        var van = new Van { Id = Guid.NewGuid(), Name = "V1", Category = VanCategory.Simple, PricePerDay = 10, FullDescription = "d", IsAvailable = true, NumberAvailable = 1, IsVisible = true, SellerId = sellerId };
        db.Vans.Add(van);
        db.Sellers.Add(new Seller { SellerId = sellerId, Username = "s" });
        db.Buyers.Add(new Buyer { BuyerId = buyerId, Username = "b" });
        await db.SaveChangesAsync();

        var result = await vanService.RentVan(van.Id, buyerId, new RentRequest { Days = 2, Contact = "c", PaymentToken = "tkn", CautionFee = 5 });
        Assert.True(((dynamic)result).success);
        var refreshed = await db.Vans.FirstAsync(v => v.Id == van.Id);
        Assert.Equal(0, refreshed.NumberAvailable);
    }
}
