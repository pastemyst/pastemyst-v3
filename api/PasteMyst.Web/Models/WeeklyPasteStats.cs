namespace PasteMyst.Web.Models;

public class WeeklyPasteStats
{
    public DateTime Date { get; set; }

    public int Created { get; set; }
    public int Deleted { get; set; }
    public int Expired { get; set; }

    public int Total { get; set; }
    public int Active { get; set; }
}