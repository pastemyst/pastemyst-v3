namespace pastemyst.Models;

public class Image
{
    public string Id { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public byte[] Bytes { get; set; } = null!;
}