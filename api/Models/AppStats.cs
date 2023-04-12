namespace pastemyst.Models;

public class AppStats
{
    public int ActivePastes { get; set; }
    public int TotalPastes { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalUsers { get; set; }

    public Dictionary<DateTime, int> ActivePastesOverTime { get; set; }
    public Dictionary<DateTime, int> TotalPastesOverTime { get; set; }

    public Dictionary<DateTime, int> ActiveUsersOverTime { get; set; }
    public Dictionary<DateTime, int> TotalUsersOverTime { get; set; }
}
