namespace PasteMyst.Web.Services;

public class EncryptionContext
{
    public string EncryptionKey { get; set; } = null;

    public Dictionary<string, string> EncryptionKeys { get; set; } = [];
}