namespace pastemyst.Models;

public class DecryptedPasteData
{
    public List<Pasty> Pasties { get; set; }

    public List<PasteHistory> History { get; set; } = [];
}