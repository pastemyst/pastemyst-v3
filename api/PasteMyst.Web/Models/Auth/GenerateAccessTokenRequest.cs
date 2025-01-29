using System.ComponentModel.DataAnnotations;

namespace PasteMyst.Web.Models.Auth;

public class GenerateAccessTokenRequest
{
    [Required]
    public Scope[] Scopes { get; set; }

    public string Description { get; set; }

    [Required]
    public ExpiresIn ExpiresIn { get; set; }
}
