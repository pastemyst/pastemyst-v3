---
sidebar_position: 5
slug: /v2/users
---

# Users

These are the docs for the user endpoints. Used for getting info about users.

## Check if a user exists

Checks if a user with the specified username exists.

<details>
    <summary><code>GET /api/v2/user/<b>:username</b>/exists</code></summary>

    Returns the status `200 OK` with the JSON string `""` as the body if the user exists, and `404 Not Found` if it doesn't.
</details>

## Get a user

Returns a single user.

<details>
    <summary><code>GET /api/v2/user/<b>:username</b></code></summary>

    `avatarUrl` points to the V3 image endpoint (`/api/v3/images/:imageId`). `supporterLength` is `1` if the user is a supporter and `0` otherwise.

    ##### Response

    ```json
    {
        "username": "CodeMyst",
        "contributor": true,
        "_id": "bcfu7961",
        "avatarUrl": "https://paste.myst.rs/api/v3/images/m6jqcf0e",
        "publicProfile": true,
        "defaultLang": "D",
        "supporterLength": 0
    }
    ```

    ##### Errors

    - `404` with `"Not Found"` if the user doesn't exist or doesn't have a public profile
</details>

## Get the current user

Returns the currently logged in user as identified by the token.

<details>
    <summary><code>GET /api/v2/user/self</code></summary>

    `serviceIds` contains the one provider the account is linked to. Accounts migrated from V2 keep the V2 provider name (like `github`), accounts created on V3 use `GitHub` or `GitLab`.

    ##### Required scope

    `user:read` and `paste:read` (`user` and `paste` work too, the paste scope is needed for the `stars` list)

    ##### Response

    ```json
    {
        "username": "CodeMyst",
        "contributor": true,
        "_id": "bcfu7961",
        "avatarUrl": "https://paste.myst.rs/api/v3/images/m6jqcf0e",
        "publicProfile": true,
        "defaultLang": "D",
        "supporterLength": 0,
        "stars": ["hbygfhts"],
        "serviceIds": {
            "github": "111111111"
        }
    }
    ```

    ##### Errors

    - `400` with `"invalid token."` if no valid token is provided
    - `403` with `"Missing required scope user:read."` or `"Missing required scope paste:read."` if your token doesn't have the required scopes
</details>

## Get the current user's pastes

Returns the IDs of the currently logged in user's pastes (including encrypted ones), newest first.

<details>
    <summary><code>GET /api/v2/user/self/pastes</code></summary>

    ##### Required scope

    `paste:read`

    ##### Response

    ```json
    [
        "xexhemlu",
        "sr7zm4hp",
        "jhiro31p",
        "7lp5gze9",
        "cw09wh9y"
    ]
    ```

    ##### Errors

    - `400` with `"invalid token."` if no valid token is provided
    - `403` with `"Missing required scope paste:read."` if your token doesn't have the `paste` or `paste:read` scope
</details>
