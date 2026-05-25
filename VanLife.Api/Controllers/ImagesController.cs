using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanLife.Api.Models;
using VanLife.Api.Services;
using VanLife.Api.Extensions;

namespace VanLife.Api.Controllers;

[ApiController]
[Route("api/images")]
public class ImagesController(ImageService imageService) : ControllerBase
{
    [Authorize(Roles = nameof(UserRole.Seller))]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromQuery] string? fileName, [FromQuery] Guid? vanId)
    {
        if (file is null || file.Length == 0) return BadRequest(new { message = "file is required." });

        var sellerId = User.GetUserId();
        if (sellerId is null) return Unauthorized(new { message = "Invalid token." });

        var image = await imageService.Upload(file, vanId, fileName);
        return Ok(image);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery query) => Ok(await imageService.GetAll(query));

    [Authorize(Roles = nameof(UserRole.Seller))]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var sellerId = User.GetUserId();
        if (sellerId is null) return Unauthorized(new { message = "Invalid token." });

        var deleted = await imageService.Delete(id);
        return deleted ? Ok(new { message = "Image deleted." }) : NotFound(new { message = "Image not found." });
    }
}
