using System.Text.Json.Serialization;

namespace PasteMyst.Web.Models;

public class Paste : BasePaste
{
    public List<Pasty> Pasties { get; set; }

    [JsonIgnore] public List<PasteHistory> History { get; set; } = [];

    public DateTime? EditedAt {
        get {
            if (History.Count == 0) return null;
            return History[^1].EditedAt;
        }
    }
}
