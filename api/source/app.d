import vibe.d;
import pastemyst.config;

void main()
{
    Config config = loadConfig("config.json");

    auto router = new URLRouter();

    auto serverSettings = new HTTPServerSettings();
    serverSettings.bindAddresses = ["127.0.0.1"];
    serverSettings.port = config.port;

    listenHTTP(serverSettings, router);

    runApplication();
}
