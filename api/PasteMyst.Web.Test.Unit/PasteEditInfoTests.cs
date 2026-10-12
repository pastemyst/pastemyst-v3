using System.ComponentModel.DataAnnotations;
using PasteMyst.Web.Models;

namespace PasteMyst.Web.Test.Unit;

/// <summary>
/// ASP.NET validates request bodies with these attributes before the controller runs, so an invalid
/// edit becomes a 400 instead of reaching the service.
/// </summary>
public sealed class PasteEditInfoTests
{
    private static bool IsValid(PasteEditInfo info) =>
        Validator.TryValidateObject(info, new ValidationContext(info), [], validateAllProperties: true);

    [Test]
    public void Pasties_Missing_IsInvalid() => Assert.That(IsValid(new PasteEditInfo { Title = "t" }), Is.False);

    [Test]
    public void Pasties_Empty_IsInvalid() => Assert.That(IsValid(new PasteEditInfo { Title = "t", Pasties = [] }), Is.False);

    [Test]
    public void Pasties_Present_IsValid() =>
        Assert.That(IsValid(new PasteEditInfo { Title = "t", Pasties = [new PastyEditInfo { Title = "", Content = "x" }] }), Is.True);
}
