using Microsoft.AspNetCore.Mvc;
using VanLife.Api.Services;

namespace VanLife.Api.Controllers;

[ApiController]
[Route("api/seller")]
public class SellerDashboardController(VanService vanService, DashboardService dashboardService) : ControllerBase
{
    [HttpGet("{sellerId:guid}/dashboard/vans")]
    public IActionResult GetSellerVans(Guid sellerId) => Ok(vanService.GetSellerVans(sellerId));

    [HttpGet("{sellerId:guid}/dashboard/income")]
    public IActionResult GetSellerIncome(Guid sellerId, [FromQuery] int? days)
        => Ok(dashboardService.GetSellerIncome(sellerId, days));

    [HttpGet("vans/{id:guid}")]
    public IActionResult GetSellerVanDetails(Guid id)
    {
        var van = vanService.GetSellerVan(id);
        return van is null ? NotFound(new { message = "Van not found." }) : Ok(van);
    }
}

