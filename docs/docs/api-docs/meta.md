---
sidebar_position: 7
slug: /meta
---

# Meta

These are the docs for the meta endpoints. Used for fetching some stats and info about PasteMyst itself.

## Get the active paste count

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

## Get the PasteMyst version

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

## Get all releases

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

## Get the PasteMyst stats

Returns the number of active/total pastes and active/total users, along with weekly paste stats.

<details>
    <summary><code>GET /api/v3/meta/stats</code></summary>

    The stats are calculated from the action log (paste created/deleted/expired and user created/deleted events), so `activePastes` can differ slightly from the count returned by `/api/v3/meta/active_pastes`, which counts the paste documents currently stored (including encrypted pastes).

    `weeklyPasteStats` contains one entry per ISO week that had any paste activity, sorted from oldest to newest. Weeks without any activity are left out. Each entry contains:

    - `date` - the Monday of the ISO week
    - `created` - number of pastes created that week
    - `deleted` - number of pastes deleted that week
    - `expired` - number of pastes that expired that week
    - `total` - cumulative number of pastes created up to and including that week
    - `active` - cumulative number of active pastes (created minus deleted minus expired) up to and including that week

    ##### Response

    ```json
    {
        "activePastes": 10,
        "totalPastes": 12,
        "activeUsers": 1,
        "totalUsers": 1,
        "weeklyPasteStats": [
            {
                "date": "2025-02-24T00:00:00Z",
                "created": 3,
                "deleted": 0,
                "expired": 0,
                "total": 3,
                "active": 3
            },
            {
                "date": "2025-03-17T00:00:00Z",
                "created": 3,
                "deleted": 1,
                "expired": 0,
                "total": 6,
                "active": 5
            },
            {
                "date": "2025-03-24T00:00:00Z",
                "created": 6,
                "deleted": 0,
                "expired": 1,
                "total": 12,
                "active": 10
            }
        ]
    }
    ```
</details>
