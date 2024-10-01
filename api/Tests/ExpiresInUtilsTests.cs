using pastemyst.Models;
using pastemyst.Utils;
using Xunit;

namespace pastemyst.Tests;

public class ExpiresInUtilsTests
{
    [Fact]
    public void TestToDeletesAt()
    {
        var start = DateTime.Now;
        var expiresIn = ExpiresIn.OneHour;

        var result = ExpiresInUtils.ToDeletesAt(start, expiresIn);

        Assert.Equal(start.AddHours(1), result);
    }
}
