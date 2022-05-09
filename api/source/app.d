import pastemyst.controllers;
import pastemyst.services;
import poodinis;
import vibe.d;

version (unittest)
{
}
else
{
    void main()
    {
        auto configService = new ConfigService("config.json");

        auto dependencies = new shared DependencyContainer();
        dependencies.register!ConfigService().existingInstance(configService);
        dependencies.register!MongoService();
        dependencies.register!UserService();
        dependencies.register!AuthService();
        dependencies.register!PasteService();
        dependencies.register!ChangelogService();

        dependencies.register!AuthController();
        dependencies.register!AuthWebController();
        dependencies.register!UserController();
        dependencies.register!MetaController();
        dependencies.register!PasteController();

        auto router = new URLRouter();
        router.registerWebInterface(dependencies.resolve!AuthWebController());

        router.registerRestInterface(dependencies.resolve!AuthController());
        router.registerRestInterface(dependencies.resolve!UserController());
        router.registerRestInterface(dependencies.resolve!MetaController());
        router.registerRestInterface(dependencies.resolve!PasteController());

        auto serverSettings = new HTTPServerSettings();
        serverSettings.bindAddresses = ["127.0.0.1"];
        serverSettings.port = configService.config.port;
        serverSettings.sessionOptions = SessionOption.noSameSiteStrict | SessionOption.httpOnly;
        serverSettings.sessionStore = new MemorySessionStore();

        listenHTTP(serverSettings, router);

        runApplication();
    }
}
