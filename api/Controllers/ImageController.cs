using Microsoft.AspNetCore.Mvc;
using pastemyst.Services;

namespace pastemyst.Controllers;

[ApiController]
[Route("/api/v3/images")]
public class ImageController : ControllerBase
{
    private readonly IImageService _imageService;

    public ImageController(IImageService imageService)
    {
        _imageService = imageService;
    }

    [HttpGet("{imageId}")]
    public async Task<IActionResult> GetImage(string imageId)
    {
        var image = await _imageService.FindByIdAsync(imageId);

        if (image is null) return NotFound();

        return File(image.Bytes, image.ContentType);
    }
}