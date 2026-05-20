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

    [HttpGet("seller/vans")]
    public async Task<IActionResult> GetSellerInventory([FromQuery] Guid sellerId, [FromQuery] VanQuery query)
    {
        if (sellerId == Guid.Empty) return BadRequest(new { message = "sellerId is required when authentication is disabled." });

        var vans = await vanService.GetSellerInventory(sellerId, query);
        return Ok(vans);
    }

    [HttpPost("seller/vans")]
    public async Task<IActionResult> CreateVan([FromQuery] Guid sellerId, [FromBody] CreateVanRequest request)
    {
        if (sellerId == Guid.Empty) return BadRequest(new { message = "sellerId is required when authentication is disabled." });

        var result = await vanService.CreateVan(sellerId, request) as CreateResult;
        if (result is null) return BadRequest(new { message = "Could not create van." });
        return result.Success ? CreatedAtAction(nameof(GetOne), new { id = result.VanId }, result) : BadRequest(result);
    }

    [HttpPut("seller/vans/{id:guid}")]
    public async Task<IActionResult> UpdateVan(Guid id, [FromQuery] Guid sellerId, [FromBody] UpdateVanRequest request)
    {
        if (sellerId == Guid.Empty) return BadRequest(new { message = "sellerId is required when authentication is disabled." });

        var result = await vanService.UpdateVan(sellerId, id, request) as OperationResult;
        if (result is null) return BadRequest(new { message = "Could not update van." });
        return result.Success ? Ok(result) : Forbid();
    }

    [HttpPatch("seller/vans/{id:guid}/availability")]
    public async Task<IActionResult> UpdateAvailability(Guid id, [FromQuery] Guid sellerId, [FromBody] UpdateAvailabilityRequest request)
    {
        if (sellerId == Guid.Empty) return BadRequest(new { message = "sellerId is required when authentication is disabled." });

        var success = await vanService.UpdateAvailability(sellerId, id, request.IsAvailable, request.NumberAvailable);
        return success ? Ok(new { message = "Availability updated." }) : Forbid();
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOne(Guid id)
    {
        var van = await vanService.GetById(id);
        return van is null ? NotFound(new { message = "Van not found." }) : Ok(van);
    }

    [HttpGet("{id:guid}/management")]
    public async Task<IActionResult> GetSellerVanDetails(Guid id, [FromQuery] Guid sellerId)
    {
        if (sellerId == Guid.Empty) return BadRequest(new { message = "sellerId is required when authentication is disabled." });

        var van = await vanService.GetSellerVan(sellerId, id);
        return van is null ? Forbid() : Ok(van);
    }

    [HttpPost("{id:guid}/rent")]
    public async Task<IActionResult> Rent(Guid id, [FromQuery] Guid buyerId, [FromBody] RentRequest request)
    {
        if (buyerId == Guid.Empty) return BadRequest(new { success = false, message = "buyerId is required when authentication is disabled." });

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

        var result = await vanService.RentVan(id, buyerId, request);
        return Ok(result);
    }
}
