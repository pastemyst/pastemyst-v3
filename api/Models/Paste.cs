namespace pastemyst.Models;

public class Paste
{
    public string Id { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public ExpiresIn ExpiresIn { get; set; }
    
    public DateTime? DeletesAt { get; set; }
    
    public string Title { get; set; }
    
    public List<Pasty> Pasties { get; set; }
    
    public User Owner { get; set; }
    
    public bool Private { get; set; }
    
    public int Stars { get; set; }
}