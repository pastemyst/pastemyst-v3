---
description: "PasteMyst v3 API docs for authentication: get the logged in user and manage access tokens."
sidebar_position: 4
slug: /auth
---

# Auth

These are the docs for various authentication endpoints. Used for getting the currently logged in user, as well as fetching and creating access tokens.

Errors are returned in the usual v3 error format described in [getting started](../getting-started.md).

There are also login, register and logout endpoints (`/api/v3/login/...`, `/api/v3/auth/register`, `/api/v3/auth/logout`), but these are only meant to be used by the website. For the API, use access tokens.

## Get self

Returns the currently logged in user.

<details>
    <summary><code>GET /api/v3/auth/self</code></summary>

    ##### Required scope

    `user` or `user:read`

    ##### Response

    ```json
    {
        "avatarId": "67c4ac35f751623ec8fd366e",
        "createdAt": "2025-03-02T19:06:29.538Z",
        "id": "0vzj9trh",
        "isAdmin": true,
        "isContributor": true,
        "isSupporter": true,
        "username": "CodeMyst"
    }
    ```

    ##### Errors

    - `401 Unauthorized` (`You must be authorized to get self.`) if not logged in.
    - `403 Forbidden` if missing the `user` or `user:read` scope.
</details>

## Generate a new access token

Generates a new access token with the specified scopes. The access tokens will get created for the user you're logged in as.

<details>
    <summary><code>POST /api/v3/auth/self/access_tokens</code></summary>

    The list of available scopes: `paste`, `paste:read`, `user`, `user:read`, `user:access_tokens`

    - `scopes` - required, the list of scopes the token will have.
    - `description` - optional, text to remind you what the token is for.
    - `expiresIn` - when the token expires. Possible values are the same as for pastes: `never`, `1h`, `2h`, `10h`, `1d`, `2d`, `1w`, `1m`, `1y`.

    The returned token has the format `<8 character token id>-<secret>`. The secret is only ever shown in this response. PasteMyst only stores a SHA-512 hash of it, so you will never be able to see the actual token again.

    `expiresAt` is `null` for tokens that never expire.

    ##### Required scope

    `user:access_tokens`

    ##### Request

    ```json
    {
        "scopes": ["paste:read", "user:read"],
        "description": "text to remind you what the access token is for",
        "expiresIn": "never"
    }
    ```

    ##### Response

    ```json
    {
        "accessToken": "a1b2c3d4-...",
        "expiresAt": null
    }
    ```

    ##### Errors

    - `403 Forbidden` (`You must be authorized to generate new access tokens.`) if not logged in.
    - `403 Forbidden` if missing the `user:access_tokens` scope.
</details>

## Get all access tokens

Returns the list of all of your access tokens. This response doesn't return the actual tokens, since only their hashes are stored.

<details>
    <summary><code>GET /api/v3/auth/self/access_tokens</code></summary>

    The tokens the website creates for your browser sessions when you log in aren't included.

    `expiresAt` is `null` for tokens that never expire.

    ##### Required scope

    `user:access_tokens`

    ##### Response

    ```json
    [
        {
            "id": "juhbgytr",
            "description": "my access token",
            "createdAt": "2025-03-02T19:06:29.538Z",
            "expiresAt": null,
            "scopes": ["paste:read", "user:read"]
        }
    ]
    ```

    ##### Errors

    - `403 Forbidden` (`You must be authorized to generate new access tokens.`) if not logged in.
    - `403 Forbidden` if missing the `user:access_tokens` scope.
</details>

## Delete an access token

Deletes a single access token by its ID. You can only delete your own access tokens.

<details>
    <summary><code>DELETE /api/v3/auth/self/access_tokens/<b>:id</b></code></summary>

    ##### Required scope

    `user:access_tokens`

    ##### Errors

    - `403 Forbidden` (`You must be authorized to generate new access tokens.`) if not logged in.
    - `403 Forbidden` if missing the `user:access_tokens` scope.
    - `404 Not Found` (`Access token not found.`) if the token doesn't exist, or belongs to another user.
</details>
