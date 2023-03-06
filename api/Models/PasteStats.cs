namespace pastemyst.Models;

public class PasteStats : Stats
{
    public Dictionary<string, Stats> Pasties { get; set; } = new();
}

public class Stats
{
    public int Lines { get; set; }
    public int Words { get; set; }
    public int Bytes { get; set; }
}