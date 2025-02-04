namespace PasteMyst.Web.Models;

public class EncryptedPaste : BasePaste
{
    public string EncryptedData { get; set; }

    public string Iv { get; set; }

    public string Salt { get; set; }

    public int EncryptionVersion { get; set; } = 3;
}
