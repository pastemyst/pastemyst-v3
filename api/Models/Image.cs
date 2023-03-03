namespace pastemyst.Models;

public class Image
{
    public string Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string ContentType { get; set; }

    public byte[] Bytes { get; set; }
}