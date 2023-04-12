namespace pastemyst.Models;

public class ActionLog
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ActionLogType Type { get; set; }

    public string ObjectId { get; set; }
}
