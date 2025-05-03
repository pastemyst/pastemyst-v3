---
sidebar_position: 3
slug: /v2/time
---

# Time

These are the docs for the time endpoints. Used for converting an `expiresIn` value to a unix timestamp.

## Expires in to a unix timestamp

Converts an `expiresIn` value to a specific time when a paste should expire.

<details>
    <summary><code>GET /api/v2/time/expiresInToUnixTime?createdAt=<b>:createdAt</b>&expiresIn=<b>:expiresIn</b></code></summary>

    List of possible `expiresIn` value: `never`, `1h`, `2h`, `10h`, `1d`, `2d`, `1w`, `1m`, `1y`.

    Example request: `GET /api/v2/time/expiresInToUnixTime?createdAt=1588441258&expiresIn=1w`

    ##### Response

    ```json
    {
        "result": 1589046058
    }
    ```
</details>