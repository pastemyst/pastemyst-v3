using PasteMyst.Web.Exceptions;
using System.Net;
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
        // Check before downloading: downloading a missing image throws
        var imageMeta = await imageService.FindByIdAsync(imageId, cancellationToken)
            ?? throw new HttpException(HttpStatusCode.NotFound, "Image not found.");

        var imageBytes = await imageService.DownloadByIdAsync(imageId, cancellationToken);

        return File(imageBytes, imageMeta.Metadata["Content-Type"].ToString()!);
    }
}
