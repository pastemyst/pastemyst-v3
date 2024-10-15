using System.ComponentModel.DataAnnotations;

namespace pastemyst.Models;

public class PasteEditInfo
{
    [MaxLength(128)]
    public string Title { get; set; }

    public List<PastyEditInfo> Pasties { get; set; }
}
