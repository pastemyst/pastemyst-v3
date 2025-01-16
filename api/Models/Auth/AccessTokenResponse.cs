namespace pastemyst.Models.Auth;

public class AccessTokenResponse
{
    public string Description { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? ExpiresAt { get; init; }

    public Scope[] Scopes { get; init; }
}
