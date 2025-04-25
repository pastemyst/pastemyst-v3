---
sidebar_position: 5
slug: /settings
---

# Settings

These are the docs for the settings endpoints. Used for getting and changing the settings of your user, as well as local browser (cookie-based) settings.

## Get the settings

Returns the settings. If requesting as the logged in user, it will return settings saved on the user. If not logged in it will return the settings saved in the browser as a cookie (or default settings if the cookie isn't set, and sets one).

<details>
    <summary><code>GET /api/v3/settings</code></summary>

    ##### Required scope

    `user:read` (if logged in)

    ##### Response

    It will also set a new session cookie with settings if one didn't exist on the request.

    ```json
    {
        "copyLinkOnCreate": false,
        "defaultIndentationUnit": "spaces",
        "defaultIndentationWidth": 4,
        "defaultLanguage": "Autodetect",
        "pasteView": "tabbed",
        "textWrap": true,
        "theme": "myst"
    }
    ```
</details>

## Update the settings

Updates the settings. If requesting as the logged in user, it will update the settings saved on the user. If not logged in it will update the settings saved in the browser as a cookie.

<details>
    <summary><code>PATCH /api/v3/settings</code></summary>

    ##### Required scope

    `user` (if logged in)

    ##### Request

    It will also set a new session cookie with settings if one didn't exist on the request.

    ```json
    {
        "copyLinkOnCreate": false,
        "defaultIndentationUnit": "spaces",
        "defaultIndentationWidth": 4,
        "defaultLanguage": "Autodetect",
        "pasteView": "tabbed",
        "textWrap": true,
        "theme": "myst"
    }
    ```
</details>

## Get the user settings

Returns the user settings of the currently logged in user.

<details>
    <summary><code>GET /api/v3/settings/user</code></summary>

    ##### Required scope

    `user:read`

    ##### Response

    ```json
    {
        "showAllPastesOnProfile": true
    }
    ```
</details>

## Update the user settings

Updates the user settings of the currently logged in user.

<details>
    <summary><code>PATCH /api/v3/settings/user</code></summary>

    ##### Required scope

    `user`

    ##### Request

    ```json
    {
        "showAllPastesOnProfile": true
    }
    ```
</details>

## Update the username

Updates the currently logged in user's username (if available).

<details>
    <summary><code>PATCH /api/v3/settings/username</code></summary>

    The new username must contain only alphanumeric characters and the `.`, `-`, `_` symbols.

    The maximum length is 20 characters.

    ##### Required scope

    `user`

    ##### Request

    ```json
    {
        "username": "NewUsername"
    }
    ```

    ##### Request

    You will get the status `400 Bad Request` if the username is already taken (or trying to set the same username).
</details>

## Update the avatar

Updates the currently logged in user's avatar.

<details>
    <summary><code>PATCH /api/v3/settings/avatar</code></summary>

    You should make the request as a `multipart/form-data` request with the file attached.

    ##### Required scope

    `user`
</details>