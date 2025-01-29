namespace PasteMyst.Web.Models;

public class AppStats
{
    public long ActivePastes { get; set; }
    public long TotalPastes { get; set; }
    public long ActiveUsers { get; set; }
    public long TotalUsers { get; set; }

    public SortedDictionary<DateTime, long> ActivePastesOverTime { get; set; }
    public SortedDictionary<DateTime, long> TotalPastesOverTime { get; set; }
}
