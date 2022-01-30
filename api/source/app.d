import vibe.d;
import poodinis;
import pastemyst.controllers.test;
import pastemyst.services.config;
import pastemyst.services.postgres;

void main()
{
    auto configService = new ConfigService("config.json");

    auto dependencies = new shared DependencyContainer();
    dependencies.register!ConfigService().existingInstance(configService);
    dependencies.register!PostgresService();

    dependencies.register!TestController();

    auto router = new URLRouter();
    router.registerRestInterface(dependencies.resolve!TestController());

    auto serverSettings = new HTTPServerSettings();
    serverSettings.bindAddresses = ["127.0.0.1"];
    serverSettings.port = configService.config.port;

    listenHTTP(serverSettings, router);

    runApplication();
}
