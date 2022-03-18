import vibe.d;
import poodinis;
import pastemyst.controllers;
import pastemyst.services;

void main()
{
    auto configService = new ConfigService("config.json");

    auto dependencies = new shared DependencyContainer();
    dependencies.register!ConfigService().existingInstance(configService);
    dependencies.register!MongoService();

    auto router = new URLRouter();

    auto serverSettings = new HTTPServerSettings();
    serverSettings.bindAddresses = ["127.0.0.1"];
    serverSettings.port = configService.config.port;

    listenHTTP(serverSettings, router);

    runApplication();
}
