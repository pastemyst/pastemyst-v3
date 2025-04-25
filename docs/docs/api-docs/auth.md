---
sidebar_position: 4
slug: /auth
---

# Auth

These are the docs for various authentication endpoints. Used for getting the currently logged in user, as well as fetching and creating access tokens.

## Get self

Returns the currently logged in user.

<details>
    <summary><code>GET /api/v3/auth/self</code></summary>

    ##### Required scope

    `user:read`

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
</details>

## Generate a new access token

Generates a new access token with the specified scopes. The access tokens will get created for the user you're logged in as.

<details>
    <summary><code>POST /api/v3/auth/self/access_tokens</code></summary>

    The list of available scopes: `paste`, `paste:read`, `user`, `user:read`, `user:access_tokens`

    Once you get the access token, you will never be able to see the actual secret token again.

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
        "accessToken": "...",
        "expiresAt": null
    }
    ```
</details>

## Get all access tokens

Returns the list of all of your access tokens. This response doesn't actually return the access token strings, since they're encrypted.

<details>
    <summary><code>GET /api/v3/auth/self/access_tokens</code></summary>

    ##### Required scope

    `user:access_tokens`

    ##### Response

    ```json
    [
        {
            "id": "juhbgytr",
            "description": "my access token",
            "createdAt": "2025-03-02T19:06:29.538Z",
            "expiresAt": "never",
            "scopes": ["paste:read", "user:read"]
        }
    ]
    ```
</details>

## Delete an access token

Deletes a single access token by its ID.

<details>
    <summary><code>DELETE /api/v3/auth/self/access_tokens/<b>:id</b></code></summary>

    ##### Required scope

    `user:access_tokens`
</details>