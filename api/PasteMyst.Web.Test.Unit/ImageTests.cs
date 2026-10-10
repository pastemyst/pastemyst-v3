using System.Net;
using PasteMyst.Web.Controllers;
using PasteMyst.Web.Exceptions;
using PasteMyst.Web.Services;

namespace PasteMyst.Web.Test.Unit;

public sealed class ImageTests
{
    private readonly DatabaseFixture _fixture = new();

    [OneTimeTearDown]
    public void OneTimeTearDown() => _fixture.Dispose();

    [TestCase("not-an-object-id")]            // malformed
    [TestCase("000000000000000000000000")]    // well-formed, doesn't exist
    public void GetImage_Unknown_Returns404(string imageId)
    {
        var controller = new ImageController(new ImageService(_fixture.MongoService));

        var e = Assert.ThrowsAsync<HttpException>(async () => await controller.GetImage(imageId, CancellationToken.None));

        Assert.That(e!.Status, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetImage_Existing_ReturnsFileWithContentType()
    {
        var id = await _fixture.MongoService.Images.UploadFromBytesAsync("img", [1, 2, 3], new MongoDB.Driver.GridFS.GridFSUploadOptions
        {
            Metadata = new MongoDB.Bson.BsonDocument { { "Content-Type", "image/png" } }
        });
        var controller = new ImageController(new ImageService(_fixture.MongoService));

        var result = await controller.GetImage(id.ToString(), CancellationToken.None) as Microsoft.AspNetCore.Mvc.FileContentResult;

        Assert.Multiple(() =>
        {
            Assert.That(result!.ContentType, Is.EqualTo("image/png"));
            Assert.That(result.FileContents, Is.EqualTo(new byte[] { 1, 2, 3 }));
        });
    }
}
