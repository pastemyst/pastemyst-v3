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
    dependencies.register!UserService();
    dependencies.register!AuthService();

    dependencies.register!AuthController();
    dependencies.register!AuthWebController();

    auto router = new URLRouter();
    router.registerWebInterface(dependencies.resolve!AuthWebController());

    router.registerRestInterface(dependencies.resolve!AuthController());

    auto serverSettings = new HTTPServerSettings();
    serverSettings.bindAddresses = ["127.0.0.1"];
    serverSettings.port = configService.config.port;
    serverSettings.sessionOptions = SessionOption.noSameSiteStrict | SessionOption.httpOnly;
    serverSettings.sessionStore = new MemorySessionStore();

    listenHTTP(serverSettings, router);

    runApplication();
}
