namespace pastemyst.Models;

public class AppStats
{
    public int ActivePastes { get; set; }
    public int TotalPastes { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalUsers { get; set; }

    public SortedDictionary<DateTime, int> ActivePastesOverTime { get; set; }
    public SortedDictionary<DateTime, int> TotalPastesOverTime { get; set; }
}
