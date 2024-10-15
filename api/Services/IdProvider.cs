namespace pastemyst.Services;

public class IdProvider
{
    private const string Base36Chars = "abcdefghijklmnopqrstuvwxyz0123456789";

    /// <summary>
    /// Generates a random base36 8 character ID.
    /// </summary>
    public string GenerateId()
    {
        var random = new Random();

        var id = "";

        for (var i = 0; i < 8; i++)
        {
            id += Base36Chars[random.Next(0, Base36Chars.Length)];
        }

        return id;
    }

    /// <summary>
    /// Keeps generating random base36 8 character IDs while the provided predicate returns true.
    /// Use case: generating IDs while the previous generate ID already exists in the DB.
    /// </summary>
    public string GenerateId(Predicate<string> predicate)
    {
        var id = GenerateId();
        while (predicate(id)) id = GenerateId();
        return id;
    }

    /// <summary>
    /// Keeps generating random base36 8 character IDs while the provided predicate returns true.
    /// Use case: generating IDs while the previous generate ID already exists in the DB.
    /// </summary>
    public async Task<string> GenerateId(Func<string, Task<bool>> predicate)
    {
        var id = GenerateId();
        while (await predicate(id)) id = GenerateId();
        return id;
    }
}
