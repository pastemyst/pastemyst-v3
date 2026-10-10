using System.Security.Cryptography;
using PasteMyst.Web.Utils;

namespace PasteMyst.Web.Test.Unit;

/// <summary>
/// The samples were encrypted with v2's actual D code (the "crypto" 0.2.18 and scrypt.d packages, exactly
/// as source/pastemyst/paste/create.d does it, with a raw non-UTF-8 salt), then base64-encoded like the
/// migrator's preprocess does. So these check compatibility with v2, not just a round trip.
/// </summary>
public sealed class V2DecryptorTests
{
    // encryptedData, encryptedKey (stored as Iv), salt, password, expected JSON
    private static readonly object[] V2Samples =
    [
        new object[]
        {
            "zhdWplrkOTEoQYAm2KM6ncuvGxDLq9XUrQt2kCoFdax52a1rYTWeB3cnYGsoZnN7BOk7IJZoeA9IeOMq89Wa8PyTJ3II0c3sjEp2Ym9VBDe8X9Ud0IdJmL/HgljCNZH0wxoTZEJ2BzxVfpFLFgdqxQ==",
            "9khYYrF6zFXBZ+M+fqmR2ORgZTAJ7j8QNv2ysY+dSrxMNH35IrKCG4J4l7L2YQV9",
            "g4iNkpecoaarsLW6v8TJzg==",
            "hunter2",
            "{\"title\":\"v2 sample\",\"pasties\":[{\"_id\":\"aaaaaaaa\",\"title\":\"main.d\",\"language\":\"D\",\"code\":\"void main() {}\"}]}"
        },
        new object[]
        {
            "3DANJcW1IBNe00F3F+QFYAFBk3bMccbiaP5XYc58YpVkUVGK+hsDeSsZhabx6gPtiJMn5U2t5I9idPyC4aR0CAfePvX5rS/gTRyjjuFZ3a4CkGjW70rquSsjeZ5/f17DFItU8rsCf8M5pmxW3db7aCNkwvJn6IJxQk7Oyz5Rq1Y=",
            "4GMyx4o6ASimo73NDYtvBPfZ7EDfWwwM1rrk8Qu5mfFdLll4MzoboYCT8z2fI8sN",
            "i5CVmp+kqa6zuL3Cx8zR1g==",
            "pässwörd 🙂",
            "{\"title\":\"unicode\",\"pasties\":[{\"_id\":\"bbbbbbbb\",\"title\":\"\",\"language\":\"Plain Text\",\"code\":\"ünïcödé\\nline 2\"}]}"
        }
    ];

    [TestCaseSource(nameof(V2Samples))]
    public void Decrypt_V2EncryptedPaste(string encryptedData, string encryptedKey, string salt, string password, string expected)
    {
        Assert.That(V2Decryptor.Decrypt(encryptedData, encryptedKey, salt, password), Is.EqualTo(expected));
    }

    [Test]
    public void Decrypt_WrongPassword_Throws()
    {
        var sample = (object[])V2Samples[0];

        Assert.Throws(Is.InstanceOf<CryptographicException>(),
            () => V2Decryptor.Decrypt((string)sample[0], (string)sample[1], (string)sample[2], "wrong password"));
    }
}
