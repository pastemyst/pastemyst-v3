module pastemyst.config;

import std.file;
import std.path;
import vibe.data.json;

public struct Config
{
    public ushort port;
}

public Config loadConfig(string path)
{
    if (!exists(path)) throw new Exception("Missing " ~ path);

    Json json = parseJsonString(readText(path));

    return deserializeJson!Config(json);
}
