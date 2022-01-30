module pastemyst.services.postgres;

import std.datetime;
import dpq2;
import pastemyst.services.config;

public class PostgresService
{
    private Connection conn;

    public this(ConfigService ConfigService)
    {
        conn = new Connection(ConfigService.config.connectionInfo);
    }

    public void test() @trusted
    {
        QueryParams p;
        p.sqlCommand = "insert into pastes (id, created_at) " ~
                       "values ($1, $2);";

        p.argsVariadic("12345678", Clock.currTime());

        auto r = conn.execParams(p);
        scope (exit) destroy(r);
    }
}
