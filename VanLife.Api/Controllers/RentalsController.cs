using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanLife.Api.Models;
using VanLife.Api.Services;
using Microsoft.EntityFrameworkCore;
using VanLife.Api.Data;
using VanLife.Api.Extensions;

namespace VanLife.Api.Controllers;

[ApiController]
[Route("api/rentals")]
public class RentalsController(AppDbContext db, IPaymentService payments) : ControllerBase
{
    // Buyer returns an item. Seller must verify physically and then call seller/verify-return to finalize.
    [Authorize(Roles = nameof(UserRole.Buyer))]
    [HttpPost("{id:guid}/return")]
    public async Task<IActionResult> Return(Guid id)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var rental = await db.Rentals.FirstOrDefaultAsync(r => r.PurchaseId == id && r.BuyerId == userId.Value);
        if (rental is null) return NotFound(new { message = "Rental not found." });

        if (rental.ReturnedAt.HasValue) return BadRequest(new { message = "Item already returned." });

        rental.ReturnedAt = DateTime.UtcNow;
        // freeze fine at the moment of return; actual liability may be adjusted by seller
        rental.FineFrozen = true;
        // calculate fine now
        var overdueDays = CalculateOverdueDays(rental.EndDate, rental.ReturnedAt.Value, rental.FineGraceDays);
        rental.FineAmount = overdueDays * rental.FineRate;

        await db.SaveChangesAsync();

        return Ok(new { rental.PurchaseId, overdueDays, fine = rental.FineAmount, currency = rental.FineCurrency });
    }

    // Seller verifies the return and can waive the fine or charge/collect payment
    [Authorize(Roles = nameof(UserRole.Seller))]
    [HttpPost("seller/{id:guid}/verify-return")]
    public async Task<IActionResult> VerifyReturn(Guid id, [FromBody] SellerVerifyReturnRequest request)
    {
        var sellerId = User.GetUserId();
        if (sellerId is null) return Unauthorized();

        var rental = await db.Rentals.FirstOrDefaultAsync(r => r.PurchaseId == id && r.SellerId == sellerId.Value);
        if (rental is null) return NotFound(new { message = "Rental not found." });

        if (!rental.ReturnedAt.HasValue) return BadRequest(new { message = "Buyer has not returned the item yet." });

        if (request.IssueCost.HasValue && request.IssueCost > 0)
        {
            // charge exact repair cost to buyer
            rental.FineAmount += request.IssueCost.Value;
        }

        if (request.WaiveFine)
        {
            rental.FineWaived = true;
            rental.FineAmount = 0;
        }

        // Only seller can override; if auto-charge accepted and there's an outstanding fine, attempt to charge
        if (!rental.FineWaived && rental.FineAmount > 0 && rental.AcceptsAutoCharge && !string.IsNullOrWhiteSpace(rental.PaymentToken))
        {
            var ok = await payments.ChargeAsync(rental.PaymentToken, rental.FineAmount);
            if (ok)
            {
                db.Transactions.Add(new Transaction { Id = Guid.NewGuid(), SellerId = rental.SellerId, VanId = rental.VanId, Price = rental.FineAmount, Date = DateTime.UtcNow });
            }
        }

        await db.SaveChangesAsync();
        return Ok(new { rental.PurchaseId, fine = rental.FineAmount, waived = rental.FineWaived });
    }

    private static int CalculateOverdueDays(DateTime endDate, DateTime returnedAt, int graceDays)
    {
        var graceEnd = endDate.AddDays(graceDays);
        if (returnedAt.Date <= graceEnd.Date) return 0;
        var days = (int)(returnedAt.Date - endDate.Date).TotalDays;
        return Math.Max(0, days);
    }
}

public class SellerVerifyReturnRequest
{
    // If there was damage, exact cost to fix
    public decimal? IssueCost { get; set; }
    // Seller can choose to waive fine
    public bool WaiveFine { get; set; }
}
