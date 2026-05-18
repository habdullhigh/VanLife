using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanLife.Api.Models;
using VanLife.Api.Services;

namespace VanLife.Api.Controllers;

[ApiController]
[Route("api/images")]
public class ImagesController(ImageService imageService) : ControllerBase
{
    [Authorize(Roles = nameof(UserRole.Seller))]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] Guid? vanId)
    {
        if (file is null || file.Length == 0) return BadRequest(new { message = "file is required." });

        var image = await imageService.Upload(file, vanId);
        return Ok(image);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery query) => Ok(await imageService.GetAll(query));

    [Authorize(Roles = nameof(UserRole.Seller))]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await imageService.Delete(id);
        return deleted ? Ok(new { message = "Image deleted." }) : NotFound(new { message = "Image not found." });
    }
}
