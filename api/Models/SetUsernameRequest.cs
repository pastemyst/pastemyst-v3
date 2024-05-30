using System.ComponentModel.DataAnnotations;

namespace pastemyst.Models;

public class SetUsernameRequest
{
    [Required]
    [MaxLength(20)]
    [RegularExpression(@"^[\w\d\.\-_]*$")]
    public string Username { get; set; }
}
