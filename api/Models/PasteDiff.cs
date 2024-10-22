namespace pastemyst.Models;

public class PasteDiff
{
    public Paste CurrentPaste { get; init; }
    public PasteHistory OldPaste { get; init; }
    public PasteHistory NewPaste { get; init; }
}
