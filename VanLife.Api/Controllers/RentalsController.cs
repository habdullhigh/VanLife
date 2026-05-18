using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanLife.Api.Extensions;
using VanLife.Api.Services;

namespace VanLife.Api.Controllers;

[ApiController]
[Route("api/rentals")]
public class RentalsController : ControllerBase
{
    private readonly RentalService rentalService;

    public RentalsController(RentalService rentalService)
    {
        this.rentalService = rentalService;
    }

    [Authorize(Roles = nameof(VanLife.Api.Models.UserRole.Buyer))]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyRentals([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int skip = 0)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized(new { message = "Invalid token." });

        var items = await rentalService.GetBuyerHistory(userId.Value, page, pageSize, skip);
        return Ok(items);
    }
}
