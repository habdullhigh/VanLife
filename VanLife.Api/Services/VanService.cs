using Microsoft.EntityFrameworkCore;
using VanLife.Api.Data;
using VanLife.Api.Models;

namespace VanLife.Api.Services;

public class VanService
{
    private readonly AppDbContext db;
    private readonly IPaymentService payments;

    public VanService(AppDbContext db, IPaymentService payments)
    {
        this.db = db;
        this.payments = payments;
    }

    public async Task<PagedResult<VanListItemDto>> GetAll(VanQuery query)
    {
        var vans = db.Vans.AsQueryable();

        // Public listing defaults to visible vans only unless explicitly overridden.
        if (!query.IsVisible.HasValue)
        {
            vans = vans.Where(v => v.IsVisible);
        }

        // If a free-text query is provided, match against Name and FullDescription
        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            var q = query.Query.Trim().ToLower();
            vans = vans.Where(v => v.Name.ToLower().Contains(q) || v.FullDescription.ToLower().Contains(q));
        }


        if (query.Category.HasValue)
        {
            vans = vans.Where(v => v.Category == query.Category.Value);
        }

        if (query.MinPrice.HasValue)
        {
            vans = vans.Where(v => v.PricePerDay >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            vans = vans.Where(v => v.PricePerDay <= query.MaxPrice.Value);
        }

        if (query.SellerId.HasValue)
        {
            vans = vans.Where(v => v.SellerId == query.SellerId.Value);
        }

        if (query.IsVisible.HasValue)
        {
            vans = vans.Where(v => v.IsVisible == query.IsVisible.Value);
        }

        var total = await vans.CountAsync();
        var safePage = Math.Max(1, query.Page);
        var safePageSize = Math.Clamp(query.PageSize, 1, 100);
        var pageSkip = (safePage - 1) * safePageSize;
        var skip = Math.Max(query.Skip, pageSkip);

        var items = await vans
            .OrderBy(v => v.Name)
            .Skip(skip)
            .Take(safePageSize)
            .Select(v => new VanListItemDto(v.Id, v.Name, v.Category, v.PricePerDay))
            .ToListAsync();

        return new PagedResult<VanListItemDto>(items, total, safePage, safePageSize, skip);
    }

    public async Task<VanDetailsDto?> GetById(Guid id)
    {
        var van = await db.Vans.FirstOrDefaultAsync(v => v.Id == id);
        return van is null
            ? null
            : new VanDetailsDto(van.Id, van.Name, van.PricePerDay, van.FullDescription, van.IsAvailable, van.NumberAvailable);
    }

    public async Task<SellerVanDetailsDto?> GetSellerVan(Guid sellerId, Guid vanId)
    {
        var isSeller = await db.Users.AnyAsync(u => u.Id == sellerId && u.Role == UserRole.Seller);
        if (!isSeller) return null;

        var van = await db.Vans
            .Include(v => v.Photos)
            .FirstOrDefaultAsync(v => v.Id == vanId && v.SellerId == sellerId);
        return van is null
            ? null
            : new SellerVanDetailsDto(van.Id, van.Name, van.Category, van.FullDescription, van.IsVisible, van.PricePerDay, van.Photos.Select(x => x.Url).ToList());
    }

    public async Task<object> CreateVan(Guid sellerId, CreateVanRequest request)
    {
        // Ensure seller exists and has seller role
        var seller = await db.Users.FirstOrDefaultAsync(u => u.Id == sellerId && u.Role == UserRole.Seller);
        if (seller is null)
        {
            return new { success = false, message = "Seller not found or not authorized." };
        }

        var van = new Van
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Category = request.Category,
            PricePerDay = request.PricePerDay,
            FullDescription = request.FullDescription,
            NumberAvailable = request.NumberAvailable,
            IsAvailable = request.NumberAvailable > 0,
            IsVisible = true,
            SellerId = sellerId
        };

        db.Vans.Add(van);
        await db.SaveChangesAsync();

        return new CreateResult(true, "Van created.", van.Id);
    }

    public async Task<object> UpdateVan(Guid sellerId, Guid vanId, UpdateVanRequest request)
    {
        var isSeller = await db.Users.AnyAsync(u => u.Id == sellerId && u.Role == UserRole.Seller);
        if (!isSeller)
        {
            return new { success = false, message = "Seller not found or not authorized." };
        }

        var van = await db.Vans.FirstOrDefaultAsync(v => v.Id == vanId && v.SellerId == sellerId);
        if (van is null)
        {
            return new { success = false, message = "Van not found or not owned by seller." };
        }

        if (request.Name is not null) van.Name = request.Name;
        if (request.Category.HasValue) van.Category = request.Category.Value;
        if (request.PricePerDay.HasValue) van.PricePerDay = request.PricePerDay.Value;
        if (request.FullDescription is not null) van.FullDescription = request.FullDescription;
        if (request.NumberAvailable.HasValue)
        {
            if (request.NumberAvailable.Value < 0)
            {
                return new { success = false, message = "NumberAvailable cannot be negative." };
            }
            van.NumberAvailable = request.NumberAvailable.Value;
            van.IsAvailable = van.NumberAvailable > 0;
        }
        if (request.IsVisible.HasValue) van.IsVisible = request.IsVisible.Value;

        await db.SaveChangesAsync();
        return new OperationResult(true, "Van updated.");
    }

    public async Task<bool> UpdateAvailability(Guid sellerId, Guid vanId, bool isAvailable, int? numberAvailable)
    {
        var isSeller = await db.Users.AnyAsync(u => u.Id == sellerId && u.Role == UserRole.Seller);
        if (!isSeller) return false;

        var van = await db.Vans.FirstOrDefaultAsync(v => v.Id == vanId && v.SellerId == sellerId);
        if (van is null) return false;

        van.IsAvailable = isAvailable;
        if (numberAvailable.HasValue)
        {
            if (numberAvailable.Value < 0) return false;
            van.NumberAvailable = numberAvailable.Value;
        }
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<VanListItemDto>> GetSellerInventory(Guid sellerId, VanQuery query)
    {
        var isSeller = await db.Users.AnyAsync(u => u.Id == sellerId && u.Role == UserRole.Seller);
        if (!isSeller) return new PagedResult<VanListItemDto>(Array.Empty<VanListItemDto>(), 0, 1, 10, 0);

        query.SellerId = sellerId;
        query.IsVisible = null; // sellers can see their full inventory
        return await GetAll(query);
    }

    public async Task<object> RentVan(Guid vanId, Guid buyerId, RentRequest request)
    {
        var van = await db.Vans.FirstOrDefaultAsync(v => v.Id == vanId);
        if (van is null)
        {
            return new { success = false, message = "Van not found." };
        }

        // Enforce: seller must have an account (and be a seller)
        var seller = await db.Users.FirstOrDefaultAsync(u => u.Id == van.SellerId && u.Role == UserRole.Seller);
        if (seller is null)
        {
            return new { success = false, message = "This van's seller does not have a valid seller account." };
        }

        // Enforce: buyer must have an account (and be a buyer)
        var buyer = await db.Users.FirstOrDefaultAsync(u => u.Id == buyerId && u.Role == UserRole.Buyer);
        if (buyer is null)
        {
            return new { success = false, message = "Buyer must have an account before renting. Please sign up first." };
        }

        // Basic availability check
        if (!van.IsAvailable || van.NumberAvailable < 1)
        {
            return new { success = false, message = "Van is not currently available." };
        }
        if (request.Days < 1)
        {
            return new { success = false, message = "Days must be at least 1." };
        }

        // Date-based availability: use today as start date and compute end date
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(request.Days - 1);

        // Ensure no overlapping rentals exist for the van in the requested period
        var overlapping = await db.Rentals.AnyAsync(r => r.VanId == vanId && !(r.EndDate < start || r.StartDate > end));
        if (overlapping)
        {
            return new { success = false, message = "Van is already rented for the requested dates." };
        }

        // Payment and caution checks (placeholder - integrate with gateway in production)
        if (string.IsNullOrWhiteSpace(request.PaymentToken))
        {
            return new { success = false, message = "Payment required." };
        }

        if (request.CautionFee < 0)
        {
            return new { success = false, message = "Caution fee cannot be negative." };
        }

        var rentFee = van.PricePerDay * request.Days;
        var totalDue = rentFee + request.CautionFee;

        // Charge payment using payment service
        var charged = await payments.ChargeAsync(request.PaymentToken, totalDue);
        if (!charged)
        {
            return new { success = false, message = "Payment failed." };
        }

        // Deduct availability
        van.NumberAvailable -= 1;
        van.IsAvailable = van.NumberAvailable > 0;

        db.Rentals.Add(new Rental
        {
            PurchaseId = Guid.NewGuid(),
            SellerId = van.SellerId,
            BuyerId = buyer.Id,
            VanId = van.Id,
            PurchasedAt = DateTime.UtcNow,
            Days = request.Days,
            Destination = request.Destination,
            Contact = request.Contact,
            CautionFee = request.CautionFee,
            TotalPaid = totalDue,
            StartDate = start,
            EndDate = end
        });

        db.Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(),
            SellerId = van.SellerId,
            VanId = van.Id,
            Price = totalDue,
            Date = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        return new
        {
            success = true,
            message = "Van rented successfully.",
            vanId,
            days = request.Days,
            rentFee,
            cautionFee = request.CautionFee,
            totalDue
        };
    }
}

