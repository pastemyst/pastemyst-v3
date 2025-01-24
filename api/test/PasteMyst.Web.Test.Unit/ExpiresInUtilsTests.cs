using PasteMyst.Web.Models;
using PasteMyst.Web.Utils;

namespace PasteMyst.Web.Test.Unit;

public class ExpiresInUtilsTests
{
    [Test]
    public void TestToDeletesAt()
    {
        var start = DateTime.Now;
        var expiresIn = ExpiresIn.OneHour;

        var result = ExpiresInUtils.ToDeletesAt(start, expiresIn);

        Assert.That(result, Is.EqualTo(start.AddHours(1)));
    }
}
