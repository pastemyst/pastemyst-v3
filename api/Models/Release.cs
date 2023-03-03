namespace pastemyst.Models;

public class Release
{
    public string Url { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public bool IsPrerelease { get; set; }

    public DateTimeOffset? ReleasedAt { get; set; }
}
