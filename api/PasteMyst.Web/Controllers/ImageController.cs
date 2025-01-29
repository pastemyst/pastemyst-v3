using Microsoft.AspNetCore.Mvc;
using PasteMyst.Web.Services;

namespace PasteMyst.Web.Controllers;

[ApiController]
[Route("/api/v3/images")]
public sealed class ImageController(ImageService imageService) : ControllerBase
{
    [HttpGet("{imageId}")]
    public async Task<IActionResult> GetImage(string imageId, CancellationToken token)
    {
        var imageMeta = await imageService.FindByIdAsync(imageId, token);
        var imageBytes = await imageService.DownloadByIdAsync(imageId, token);

        if (imageMeta is null || imageBytes is null) return NotFound();

        return File(imageBytes, imageMeta.Metadata["Content-Type"].ToString()!);
    }
}
