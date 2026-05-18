using Microsoft.EntityFrameworkCore;
using VanLife.Api.Data;
using VanLife.Api.Models;

namespace VanLife.Api.Services;

public class RentalService
{
    private readonly AppDbContext db;

    public RentalService(AppDbContext db)
    {
        this.db = db;
    }

    private static DateTime NormalizeToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    public async Task<IEnumerable<RentalHistoryItemDto>> GetBuyerHistory(Guid buyerId, int page = 1, int pageSize = 20, int skip = 0)
    {
        var query = db.Rentals.Where(r => r.BuyerId == buyerId).OrderByDescending(r => r.PurchasedAt);
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);
        var pageSkip = (safePage - 1) * safePageSize;
        var toSkip = Math.Max(skip, pageSkip);

        var items = await query.Skip(toSkip).Take(safePageSize).ToListAsync();

        return items.Select(r => new RentalHistoryItemDto
        {
            PurchaseId = r.PurchaseId,
            VanId = r.VanId,
            SellerId = r.SellerId,
            PurchasedAt = NormalizeToUtc(r.PurchasedAt),
            Days = r.Days,
            TotalPaid = r.TotalPaid,
            Destination = r.Destination
        });
    }
}
