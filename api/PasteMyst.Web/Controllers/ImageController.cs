using Microsoft.AspNetCore.Mvc;
using PasteMyst.Web.Services;

namespace PasteMyst.Web.Controllers;

[ApiController]
[Route("/api/v3/images")]
public sealed class ImageController(ImageService imageService) : ControllerBase
{
    [HttpGet("{imageId}")]
    public async Task<IActionResult> GetImage(string imageId, CancellationToken cancellationToken)
    {
        var imageMeta = await imageService.FindByIdAsync(imageId, cancellationToken);
        var imageBytes = await imageService.DownloadByIdAsync(imageId, cancellationToken);

        if (imageMeta is null || imageBytes is null) return NotFound();

        return File(imageBytes, imageMeta.Metadata["Content-Type"].ToString()!);
    }
}
