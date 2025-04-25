---
sidebar_position: 7
slug: /meta
---

# Meta

These are the docs for the meta endpoints. Used for fetching some stats and info about PasteMyst itself.

# Get the active paste count

Returns the number of currently active pastes.

<details>
    <summary><code>GET /api/v3/meta/active_pastes</code></summary>

    ##### Response

    ```json
    {
        "count": 118563
    }
    ```
</details>

# Get the PasteMyst version

Returns the current running PasteMyst version.

<details>
    <summary><code>GET /api/v3/meta/version</code></summary>

    ##### Response

    ```json
    {
        "version": "3.0.0"
    }
    ```
</details>

# Get all releases

Returns the list of all releases ever released on PasteMyst, along with their own changelogs.

<details>
    <summary><code>GET /api/v3/meta/releases</code></summary>

    The content is markdown text.

    The field `isPrerelease` marks alpha/beta releases.

    ##### Response

    ```json
    [
        {
            "content": "## added\r\n- v2 to v3 database migrator\r\n- decrypting v2 pastes\r\n- custom toast messages\r\n- announcement system\r\n- loader when creating pastes\r\n- embedding pastes\r\n\r\n## changed\r\n- moved backend tests to a separate project by [@pippinmole](https://github.com/pippinmole)\r\n- refactored backend to use CancellationToken by [@pippinmole](https://github.com/pippinmole)\r\n- upgraded mongo driver to version 3\r\n- faster rendering of version and active pastes\r\n\r\n## fixed\r\n- removed hosted service from ChangelogProvider by [@pippinmole](https://github.com/pippinmole)\r\n- guesslang-bun fixed in docker by [@pippinmole ](https://github.com/pippinmole)\r\n- case insensitive matching of oauth providers\r\n- username unique index\r\n- version provider using the wrong git path\r\n- semver version ordering\r\n- reordering of tabs is no longer only visual",
            "isPrerelease": true,
            "releasedAt": "2025-02-27T20:59:29+00:00",
            "title": "v3.0.0-alpha.13",
            "url": "https://github.com/pastemyst/pastemyst-v3/releases/tag/3.0.0-alpha.13"
        },
        {
            "content": "## added\r\n- headings in markdown now have anchor links\r\n- code blocks in markdown now have syntax highlighting\r\n- showing when a paste has been edited on user pages\r\n- replaced linguist grammars with tm-grammars\r\n- pre-commit hook for svelte\r\n- migrated to svelte 5\r\n- added scopes to access tokens\r\n- you can now generate custom access tokens to access the api in the profile settings\r\n- encrypted pastes\r\n\r\n## fixed\r\n- viewing raw pastes\r\n- github actions node version\r\n- bad quartz version",
            "isPrerelease": true,
            "releasedAt": "2025-01-23T12:05:49+00:00",
            "title": "v3.0.0-alpha.12",
            "url": "https://github.com/pastemyst/pastemyst-v3/releases/tag/3.0.0-alpha.12"
        },
        {
            "content": "## fixed\r\n- made all the http methods in the frontend uppercase",
            "isPrerelease": true,
            "releasedAt": "2024-10-22T20:17:02+00:00",
            "title": "v3.0.0-alpha.11.1",
            "url": "https://github.com/pastemyst/pastemyst-v3/releases/tag/3.0.0-alpha.11.1"
        },
        // ...
    ]
    ```
</details>

# Get the PasteMyst stats

Returns the stats of active/total pastes and active/total users on PasteMyst over time.

<details>
    <summary><code>GET /api/v3/meta/stats</code></summary>

    ##### Response

    ```json
    {
        "activePastes": 12,
        "activePastesOverTime": {
            "2025-03-02T19:05:22.577Z": 1,
            "2025-03-02T19:06:09.023Z": 2,
            "2025-03-02T19:06:20.357Z": 3,
            "2025-03-17T19:31:19.782Z": 4,
            "2025-03-17T19:45:12.127Z": 5,
            "2025-03-18T16:14:33.872Z": 6,
            "2025-03-24T10:39:40.183Z": 7,
            "2025-03-24T10:45:09.287Z": 8,
            "2025-03-26T18:04:32.714Z": 9,
            "2025-03-26T18:23:18.942Z": 10,
            "2025-03-31T08:56:34.635Z": 11,
            "2025-04-22T21:22:32.762Z": 12
        },
        "activeUsers": 1,
        "totalPastes": 12,
        "totalPastesOverTime": {
            "2025-03-02T19:05:22.577Z": 1,
            "2025-03-02T19:06:09.023Z": 2,
            "2025-03-02T19:06:20.357Z": 3,
            "2025-03-17T19:31:19.782Z": 4,
            "2025-03-17T19:45:12.127Z": 5,
            "2025-03-18T16:14:33.872Z": 6,
            "2025-03-24T10:39:40.183Z": 7,
            "2025-03-24T10:45:09.287Z": 8,
            "2025-03-26T18:04:32.714Z": 9,
            "2025-03-26T18:23:18.942Z": 10,
            "2025-03-31T08:56:34.635Z": 11,
            "2025-04-22T21:22:32.762Z": 12
        },
        "totalUsers": 1
    }
    ```
</details>