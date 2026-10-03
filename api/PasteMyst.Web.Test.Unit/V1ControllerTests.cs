using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using PasteMyst.Web.Controllers.V1;

namespace PasteMyst.Web.Test.Unit;

public sealed class V1ControllerTests
{
    [Test]
    public void AnyV1Call_Returns410Gone()
    {
        var controller = new PasteControllerV1(NullLogger<PasteControllerV1>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = controller.Removed() as ObjectResult;

        Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status410Gone));
    }
}
