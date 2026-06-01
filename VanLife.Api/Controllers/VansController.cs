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
    public async Task<IActionResult> GetSellerInventory([FromQuery] VanQuery query)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized(new { message = "Invalid token." });

        var vans = await vanService.GetSellerInventory(userId.Value, query);
        return Ok(vans);
    }

    [Authorize(Roles = nameof(UserRole.Seller))]
    [HttpPost("seller/vans")]
    public async Task<IActionResult> CreateVan([FromBody] CreateVanRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized(new { message = "Invalid token." });

        var result = await vanService.CreateVan(userId.Value, request) as CreateResult;
        if (result is null) return BadRequest(new { message = "Could not create van." });
        return result.Success ? CreatedAtAction(nameof(GetOne), new { id = result.VanId }, result) : BadRequest(result);
    }

    [Authorize(Roles = nameof(UserRole.Seller))]
    [HttpPut("seller/vans/{id:guid}")]
    public async Task<IActionResult> UpdateVan(Guid id, [FromBody] UpdateVanRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized(new { message = "Invalid token." });

        var result = await vanService.UpdateVan(userId.Value, id, request) as OperationResult;
        if (result is null) return BadRequest(new { message = "Could not update van." });
        return result.Success ? Ok(result) : Forbid();
    }

    [Authorize(Roles = nameof(UserRole.Seller))]
    [HttpPatch("seller/vans/{id:guid}/availability")]
    public async Task<IActionResult> UpdateAvailability(Guid id, [FromBody] UpdateAvailabilityRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized(new { message = "Invalid token." });

        var success = await vanService.UpdateAvailability(userId.Value, id, request.IsAvailable, request.NumberAvailable);
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
    public async Task<IActionResult> GetSellerVanDetails(Guid id)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized(new { message = "Invalid token." });

        var van = await vanService.GetSellerVan(userId.Value, id);
        return van is null ? Forbid() : Ok(van);
    }

    [Authorize(Roles = nameof(UserRole.Buyer))]
    [HttpPost("{id:guid}/rent")]
    public async Task<IActionResult> Rent(Guid id, [FromBody] RentRequest request)
    {
        var buyerId = User.GetUserId();
        if (buyerId is null) return Unauthorized(new { success = false, message = "Invalid token." });

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

        var result = await vanService.RentVan(id, buyerId.Value, request);
        return Ok(result);
    }
}
