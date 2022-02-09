module pastemyst.services.config;

import std.file;
import std.path;
import vibe.data.json;

@safe:

public struct Config
{
    public ushort port;
    public string connectionInfo;
}

public class ConfigService
{
    public const Config config;

    public this(string path)
    {
        if (!exists(path)) throw new Exception("Missing " ~ path);

        Json json = parseJsonString(readText(path));

        config = deserializeJson!Config(json);
    }
}