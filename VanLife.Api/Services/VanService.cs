using VanLife.Api.Data;
using VanLife.Api.Models;

namespace VanLife.Api.Services;

public class VanService(InMemoryStore store)
{
    public IEnumerable<VanListItemDto> GetAll(VanCategory? category, decimal? minPrice, decimal? maxPrice)
    {
        var query = store.Vans.AsQueryable();

        if (category.HasValue)
        {
            query = query.Where(v => v.Category == category.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(v => v.PricePerDay >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(v => v.PricePerDay <= maxPrice.Value);
        }

        return query.Select(v => new VanListItemDto(v.Id, v.Name, v.Category, v.PricePerDay));
    }

    public VanDetailsDto? GetById(Guid id)
    {
        var van = store.Vans.FirstOrDefault(v => v.Id == id);
        return van is null
            ? null
            : new VanDetailsDto(van.Id, van.Name, van.PricePerDay, van.FullDescription, van.IsAvailable, van.NumberAvailable);
    }

    public SellerVanDetailsDto? GetSellerVan(Guid id)
    {
        var van = store.Vans.FirstOrDefault(v => v.Id == id);
        return van is null
            ? null
            : new SellerVanDetailsDto(van.Id, van.Name, van.Category, van.FullDescription, van.IsVisible, van.PricePerDay, van.Photos);
    }

    public object RentVan(Guid vanId, int days)
    {
        var van = store.Vans.FirstOrDefault(v => v.Id == vanId);
        if (van is null)
        {
            return new { success = false, message = "Van not found." };
        }

        if (!van.IsAvailable || van.NumberAvailable < 1)
        {
            return new { success = false, message = "Van is not currently available." };
        }

        if (days < 1)
        {
            return new { success = false, message = "Days must be at least 1." };
        }

        var totalPrice = van.PricePerDay * days;
        van.NumberAvailable -= 1;
        van.IsAvailable = van.NumberAvailable > 0;

        store.Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(),
            SellerId = van.SellerId,
            VanId = van.Id,
            Price = totalPrice,
            Date = DateTime.UtcNow
        });

        return new
        {
            success = true,
            message = "Van rented successfully.",
            vanId,
            days,
            totalPrice
        };
    }

    public IEnumerable<VanListItemDto> GetSellerVans(Guid sellerId)
    {
        return store.Vans
            .Where(v => v.SellerId == sellerId)
            .Select(v => new VanListItemDto(v.Id, v.Name, v.Category, v.PricePerDay));
    }
}

