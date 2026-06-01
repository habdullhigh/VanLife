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
    public async Task<IActionResult> Upload([FromQuery] string fileName, [FromQuery] Guid? vanId)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest(new { message = "fileName is required." });
        }

        var image = await imageService.Upload(fileName, vanId);
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
