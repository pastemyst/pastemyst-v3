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
        var imageMeta = await _imageService.FindByIdAsync(imageId);
        var imageBytes = await _imageService.DownloadByIdAsync(imageId);

        if (imageMeta is null || imageBytes is null) return NotFound();

        return File(imageBytes, imageMeta.Metadata["Content-Type"].ToString());
    }
}
