namespace PasteMyst.Web.Models.V2;

public class PasteV2 : BasePasteV2
{
    public List<EditV2> Edits { get; set; } = [];

    public string Title { get; set; } = "untitled";

    public List<PastyV2> Pasties { get; set; } = [];
}
