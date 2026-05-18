using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanLife.Api.Extensions;
using VanLife.Api.Models;
using VanLife.Api.Services;

namespace VanLife.Api.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Seller))]
[Route("api/dashboard")]
public class DashboardController(DashboardService dashboardService, IncomeService incomeService) : ControllerBase
{
    [HttpGet("sellers/{sellerId:guid}")]
    public async Task<IActionResult> GetSellerSummary(
        Guid sellerId,
        [FromQuery] VanQuery vanQuery,
        [FromQuery] TransactionQuery transactionQuery)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized(new { message = "Invalid token." });
        if (userId.Value != sellerId) return Forbid();

        var summary = await dashboardService.GetSellerSummary(sellerId, transactionQuery, vanQuery);
        return Ok(summary);
    }

    [HttpGet("income-graph")]
    public async Task<IActionResult> GetIncomeGraph([FromQuery] int year, [FromQuery] Guid? sellerId)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized(new { message = "Invalid token." });
        if (sellerId.HasValue && sellerId.Value != userId.Value) return Forbid();

        return Ok(await incomeService.GetYearlyGraph(year, sellerId ?? userId));
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] TransactionQuery query)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized(new { message = "Invalid token." });

        query.SellerId = userId;
        return Ok(await incomeService.GetTransactions(query));
    }
}
