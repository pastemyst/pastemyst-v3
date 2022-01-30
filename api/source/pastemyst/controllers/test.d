module pastemyst.controllers.test;

import vibe.d;
import pastemyst.services.postgres;

@path("/api/v3/test")
public interface ITestController
{
    void get() @safe;
}

public class TestController : ITestController
{
    private PostgresService postgres;

    public this(PostgresService postgres)
    {
        this.postgres = postgres;
    }

    public void get()
    {
        postgres.test();
    }
}
