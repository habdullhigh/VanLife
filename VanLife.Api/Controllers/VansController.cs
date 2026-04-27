using Microsoft.AspNetCore.Mvc;
using VanLife.Api.Models;
using VanLife.Api.Services;

namespace VanLife.Api.Controllers;

[ApiController]
[Route("api/vans")]
public class VansController(VanService vanService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll([FromQuery] VanCategory? category, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice)
    {
        var vans = vanService.GetAll(category, minPrice, maxPrice);
        return Ok(vans);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetOne(Guid id)
    {
        var van = vanService.GetById(id);
        return van is null ? NotFound(new { message = "Van not found." }) : Ok(van);
    }

    [HttpPost("{id:guid}/rent")]
    public IActionResult Rent(Guid id, [FromQuery] int days = 1)
    {
        var result = vanService.RentVan(id, days);
        return Ok(result);
    }
}

