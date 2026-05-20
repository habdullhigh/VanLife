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

    [HttpGet("me")]
    public async Task<IActionResult> GetMyRentals([FromQuery] Guid buyerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int skip = 0)
    {
        if (buyerId == Guid.Empty) return BadRequest(new { message = "buyerId is required when authentication is disabled." });

        var items = await rentalService.GetBuyerHistory(buyerId, page, pageSize, skip);
        return Ok(items);
    }
}
