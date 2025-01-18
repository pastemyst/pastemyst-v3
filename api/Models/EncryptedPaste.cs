namespace pastemyst.Models;

public class EncryptedPaste : BasePaste
{
    public string EncryptedData { get; set; }

    public string Iv { get; set; }

    public string Salt { get; set; }
}
