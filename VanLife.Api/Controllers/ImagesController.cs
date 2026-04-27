using Microsoft.AspNetCore.Mvc;
using VanLife.Api.Services;

namespace VanLife.Api.Controllers;

[ApiController]
[Route("api/images")]
public class ImagesController(ImageService imageService) : ControllerBase
{
    [HttpPost("upload")]
    public IActionResult Upload([FromQuery] string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest(new { message = "fileName is required." });
        }

        var image = imageService.Upload(fileName);
        return Ok(image);
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(imageService.GetAll());

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var deleted = imageService.Delete(id);
        return deleted ? Ok(new { message = "Image deleted." }) : NotFound(new { message = "Image not found." });
    }
}

