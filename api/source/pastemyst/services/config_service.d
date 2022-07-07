module pastemyst.services.config_service;

import std.file;
import std.path;
import vibe.data.json;
import vibe.data.serialization;

@safe:

/**
 * Application wide configuration.
 */
public struct Config
{
    /**
     * On which port the app should be hosted on.
     */
    public ushort port;

    /**
     * On what host is the app hosted on.
     */
    public string host;

    /**
     * On what host is the client app (frontend, svelte) hosted on.
     */
    public string clientHost;

    /**
     * General purpose secret. Should be very strong.
     */
    public string secret;

    /**
     * Secret for JWT related things. Should be very strong.
     */
    @optional public string jwtSecret;

    /**
     * MongoDB connection string.
     */
    public string mongoConnectionString;

    /**
     * Name of the mongo database to connect to.
     */
    public string mongoDatabase;

    /**
     * Settings for github OAuth provider.
     */
    public GitHubConfig github;
}

///
public struct GitHubConfig
{
    ///
    public string clientId;

    ///
    public string clientSecret;
}

/**
 * Service that loads the configuration from file and serves it.
 */
public class ConfigService
{
    ///
    public const Config config;

    ///
    public alias config this;

    ///
    public this(string path)
    {
        if (!exists(path)) throw new Exception("Missing " ~ path);

        Json json = parseJsonString(readText(path));

        if (json["jwtSecret"].type == Json.Type.null_ || json["jwtSecret"].type == Json.Type.undefined)
        {
            json["jwtSecret"] = json["secret"].to!string;
        }

        config = deserializeJson!Config(json);
    }
}
