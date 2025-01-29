namespace PasteMyst.Web.Models.Auth;

public class GenerateAccessTokenResponse
{
    public string AccessToken { get; init; }

    public DateTime? ExpiresAt { get; init; }
}
