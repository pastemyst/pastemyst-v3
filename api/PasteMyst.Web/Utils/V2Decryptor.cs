using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Generators;

namespace PasteMyst.Web.Utils;

/// <summary>
/// Decrypts pastes encrypted by pastemyst v2 (fields base64-encoded by the migrator).
///
/// v2 (source/pastemyst/paste/create.d) used scrypt.d and the D "crypto" package:
/// - passwordHash = uppercase hex of scrypt(password, salt, N=524288, r=8, p=1, 90 bytes), followed by
///   "$salt$..." which doesn't matter here
/// - the paste key (32 random bytes) is AES-256-CBC encrypted with the first 32 characters of that hash
///   string as the key, zero IV, PKCS#5 padding
/// - the paste JSON is AES-256-CBC encrypted with the paste key, zero IV, PKCS#5 padding
/// </summary>
public static class V2Decryptor
{
    private const int ScryptN = 524_288;
    private const int ScryptR = 8;
    private const int ScryptP = 1;
    private const int ScryptOutputLength = 90;

    /// <summary>
    /// Returns the decrypted paste JSON, or throws <see cref="CryptographicException"/> for a wrong password.
    /// </summary>
    public static string Decrypt(string encryptedData, string encryptedKey, string salt, string password)
    {
        var hash = SCrypt.Generate(Encoding.UTF8.GetBytes(password), Convert.FromBase64String(salt),
            ScryptN, ScryptR, ScryptP, ScryptOutputLength);

        // D's toHexString is uppercase; the crypto package keys AES-256 with the first 32 chars
        var passwordKey = Encoding.ASCII.GetBytes(Convert.ToHexString(hash)[..32]);

        var pasteKey = DecryptAes(passwordKey, Convert.FromBase64String(encryptedKey));

        // A wrong password usually fails the padding check above; when it happens to pass, the key is
        // garbage and has the wrong length
        if (pasteKey.Length != 32) throw new CryptographicException("Invalid key length.");

        return Encoding.UTF8.GetString(DecryptAes(pasteKey, Convert.FromBase64String(encryptedData)));
    }

    private static byte[] DecryptAes(byte[] key, byte[] cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        return aes.DecryptCbc(cipherText, new byte[16], PaddingMode.PKCS7);
    }
}
