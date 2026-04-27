using Microsoft.AspNetCore.Mvc;
using VanLife.Api.Services;

namespace VanLife.Api.Controllers;

[ApiController]
[Route("api/finance")]
public class FinanceController(IncomeService incomeService) : ControllerBase
{
    [HttpGet("income-graph")]
    public IActionResult GetIncomeGraph([FromQuery] int year)
    {
        return Ok(incomeService.GetYearlyGraph(year));
    }

    [HttpGet("transactions")]
    public IActionResult GetTransactions([FromQuery] int? days)
    {
        return Ok(incomeService.GetTransactions(days));
    }
}

