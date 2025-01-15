namespace pastemyst.Models.Auth;

public class AccessTokenResponse
{
    public string AccessToken { get; init; }

    public DateTime? ExpiresAt { get; init; }
}
