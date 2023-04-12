namespace pastemyst.Models;

public class Page<T>
{
    public List<T> Items { get; set; } = new();

    public int TotalPages { get; set; }

    public int CurrentPage { get; set; }

    public int PageSize { get; set; }

    public bool HasNextPage { get; set; }
}
