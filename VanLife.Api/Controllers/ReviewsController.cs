using Microsoft.AspNetCore.Mvc;
using VanLife.Api.Models;
using VanLife.Api.Services;

namespace VanLife.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController(ReviewService reviewService) : ControllerBase
{
    [HttpGet("users/{userId:guid}")]
    public IActionResult GetUserReviews(
        Guid userId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] ReviewType? reviewType)
    {
        return Ok(reviewService.GetUserReviews(userId, startDate, endDate, reviewType));
    }
}

