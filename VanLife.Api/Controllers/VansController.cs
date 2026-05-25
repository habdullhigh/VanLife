using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanLife.Api.Extensions;
using VanLife.Api.Models;
using VanLife.Api.Services;

namespace VanLife.Api.Controllers;

[ApiController]
[Route("api/vans")]
public class VansController(VanService vanService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] VanQuery query)
    {
        var vans = await vanService.GetAll(query);
        return Ok(vans);
    }

    [Authorize(Roles = nameof(UserRole.Seller))]
    [HttpGet("seller/vans")]
    public async Task<IActionResult> GetSellerInventory([FromQuery] Guid? sellerId, [FromQuery] VanQuery query)
    {
        var userId = User.GetUserId();
        var id = sellerId ?? userId;
        if (id is null || id == Guid.Empty) return BadRequest(new { message = "sellerId is required." });

        var vans = await vanService.GetSellerInventory(id.Value, query);
        return Ok(vans);
    }

    [Authorize(Roles = nameof(UserRole.Seller))]
    [HttpPost("seller/vans")]
    public async Task<IActionResult> CreateVan([FromQuery] Guid? sellerId, [FromBody] CreateVanRequest request)
    {
        var userId = User.GetUserId();
        var id = sellerId ?? userId;
        if (id is null || id == Guid.Empty) return BadRequest(new { message = "sellerId is required." });

        var result = await vanService.CreateVan(id.Value, request) as CreateResult;
        if (result is null) return BadRequest(new { message = "Could not create van." });
        return result.Success ? CreatedAtAction(nameof(GetOne), new { id = result.VanId }, result) : BadRequest(result);
    }

    [Authorize(Roles = nameof(UserRole.Seller))]
    [HttpPut("seller/vans/{id:guid}")]
    public async Task<IActionResult> UpdateVan(Guid id, [FromQuery] Guid? sellerId, [FromBody] UpdateVanRequest request)
    {
        var userId = User.GetUserId();
        var sid = sellerId ?? userId;
        if (sid is null || sid == Guid.Empty) return BadRequest(new { message = "sellerId is required." });

        var result = await vanService.UpdateVan(sid.Value, id, request) as OperationResult;
        if (result is null) return BadRequest(new { message = "Could not update van." });
        return result.Success ? Ok(result) : Forbid();
    }

    [Authorize(Roles = nameof(UserRole.Seller))]
    [HttpPatch("seller/vans/{id:guid}/availability")]
    public async Task<IActionResult> UpdateAvailability(Guid id, [FromQuery] Guid? sellerId, [FromBody] UpdateAvailabilityRequest request)
    {
        var userId = User.GetUserId();
        var sid = sellerId ?? userId;
        if (sid is null || sid == Guid.Empty) return BadRequest(new { message = "sellerId is required." });

        var success = await vanService.UpdateAvailability(sid.Value, id, request.IsAvailable, request.NumberAvailable);
        return success ? Ok(new { message = "Availability updated." }) : Forbid();
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOne(Guid id)
    {
        var van = await vanService.GetById(id);
        return van is null ? NotFound(new { message = "Van not found." }) : Ok(van);
    }

    [Authorize(Roles = nameof(UserRole.Seller))]
    [HttpGet("{id:guid}/management")]
    public async Task<IActionResult> GetSellerVanDetails(Guid id, [FromQuery] Guid? sellerId)
    {
        var userId = User.GetUserId();
        var sid = sellerId ?? userId;
        if (sid is null || sid == Guid.Empty) return BadRequest(new { message = "sellerId is required." });

        var van = await vanService.GetSellerVan(sid.Value, id);
        return van is null ? Forbid() : Ok(van);
    }

    [Authorize(Roles = nameof(UserRole.Buyer))]
    [HttpPost("{id:guid}/rent")]
    public async Task<IActionResult> Rent(Guid id, [FromQuery] Guid? buyerId, [FromBody] RentRequest request)
    {
        var userId = User.GetUserId();
        var bid = buyerId ?? userId;
        if (bid is null || bid == Guid.Empty) return BadRequest(new { success = false, message = "buyerId is required." });

        if (request.Days < 1)
        {
            return BadRequest(new { success = false, message = "Days must be at least 1." });
        }

        if (string.IsNullOrWhiteSpace(request.PaymentToken))
        {
            return BadRequest(new { success = false, message = "Payment is required before taking the van." });
        }

        if (string.IsNullOrWhiteSpace(request.Contact))
        {
            return BadRequest(new { success = false, message = "Contact information is required." });
        }

        var result = await vanService.RentVan(id, bid.Value, request);
        return Ok(result);
    }
}
