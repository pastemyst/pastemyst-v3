using System.ComponentModel.DataAnnotations;

namespace pastemyst.Models.Auth;

public class RegisterRequest
{
    [Required]
    [MaxLength(20)]
    [RegularExpression(@"^[\w\d\.\-_]*$")]
    public string Username { get; set; }
}