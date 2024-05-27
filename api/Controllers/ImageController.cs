using Microsoft.AspNetCore.Mvc;
using pastemyst.Services;

namespace pastemyst.Controllers;

[ApiController]
[Route("/api/v3/images")]
public class ImageController(IImageService imageService) : ControllerBase
{
    [HttpGet("{imageId}")]
    public async Task<IActionResult> GetImage(string imageId)
    {
        var imageMeta = await imageService.FindByIdAsync(imageId);
        var imageBytes = await imageService.DownloadByIdAsync(imageId);

        if (imageMeta is null || imageBytes is null) return NotFound();

        return File(imageBytes, imageMeta.Metadata["Content-Type"].ToString()!);
    }
}
