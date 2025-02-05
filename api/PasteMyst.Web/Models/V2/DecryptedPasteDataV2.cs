namespace PasteMyst.Web.Models.V2;

public class DecryptedPasteDataV2
{
    public string Title { get; set; }

    public List<PastyV2> Pasties { get; set; }
}