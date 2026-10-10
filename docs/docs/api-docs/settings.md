---
sidebar_position: 5
slug: /settings
---

# Settings

These are the docs for the settings endpoints. Used for getting and changing the settings of your user, as well as local browser (cookie-based) settings.

Errors are returned in the usual v3 error format described in [getting started](../getting-started.md).

## Get the settings

Returns the settings. If requesting as the logged in user, it will return settings saved on the user. If not logged in it will return the settings for the browser session, identified by the `pastemyst_session_settings` cookie.

<details>
    <summary><code>GET /api/v3/settings</code></summary>

    If not logged in, and the request doesn't have a valid `pastemyst_session_settings` cookie, a new session is created with the default settings (the ones shown below), and the response sets the `pastemyst_session_settings` cookie. Session settings that haven't been accessed in 30 days get deleted.

    ##### Required scope

    `user` or `user:read` (if logged in)

    ##### Response

    ```json
    {
        "copyLinkOnCreate": false,
        "darkTheme": "myst",
        "defaultIndentationUnit": "spaces",
        "defaultIndentationWidth": 4,
        "defaultLanguage": "Autodetect",
        "followSystemTheme": false,
        "pasteView": "tabbed",
        "textWrap": true,
        "theme": "myst"
    }
    ```

    ##### Errors

    - `403 Forbidden` if logged in and missing the `user` or `user:read` scope.
</details>

## Update the settings

Updates the settings. If requesting as the logged in user, it will update the settings saved on the user. If not logged in it will update the settings of the browser session from the `pastemyst_session_settings` cookie.

<details>
    <summary><code>PATCH /api/v3/settings</code></summary>

    The whole settings object gets replaced, any field you leave out is reset to its default value. Always send all of the fields.

    This endpoint doesn't create the session cookie. If not logged in, call `GET /api/v3/settings` first to get one.

    ##### Required scope

    `user` (if logged in)

    ##### Request

    ```json
    {
        "copyLinkOnCreate": false,
        "darkTheme": "myst",
        "defaultIndentationUnit": "spaces",
        "defaultIndentationWidth": 4,
        "defaultLanguage": "Autodetect",
        "followSystemTheme": false,
        "pasteView": "tabbed",
        "textWrap": true,
        "theme": "myst"
    }
    ```

    ##### Errors

    - `403 Forbidden` if logged in and missing the `user` scope.
    - `400 Bad Request` (`Can't update the settings since the user is not logged in and the session settings cookie is missing.`) if not logged in and there's no `pastemyst_session_settings` cookie.
    - `400 Bad Request` (`The session settings cookie has probably expired.`) if not logged in and the session from the cookie doesn't exist anymore.
</details>

## Get the user settings

Returns the user settings of the currently logged in user.

<details>
    <summary><code>GET /api/v3/settings/user</code></summary>

    ##### Required scope

    `user` or `user:read`

    ##### Response

    ```json
    {
        "showAllPastesOnProfile": true
    }
    ```

    ##### Errors

    - `401 Unauthorized` (`You must be authorized to fetch user settings.`) if not logged in.
    - `403 Forbidden` if missing the `user` or `user:read` scope.
</details>

## Update the user settings

Updates the user settings of the currently logged in user.

<details>
    <summary><code>PATCH /api/v3/settings/user</code></summary>

    The whole user settings object gets replaced, any field you leave out is reset to its default value.

    ##### Required scope

    `user`

    ##### Request

    ```json
    {
        "showAllPastesOnProfile": true
    }
    ```

    ##### Errors

    - `401 Unauthorized` (`You must be authorized to update user settings.`) if not logged in.
    - `403 Forbidden` if missing the `user` scope.
</details>

## Update the username

Updates the currently logged in user's username (if available).

<details>
    <summary><code>PATCH /api/v3/settings/username</code></summary>

    The new username must contain only letters, digits and the `.`, `-`, `_` symbols.

    The maximum length is 20 characters.

    Usernames are compared ignoring case, so you can't take a username that differs from an existing one only in case, and you can't change only the case of your own username.

    ##### Required scope

    `user`

    ##### Request

    ```json
    {
        "username": "NewUsername"
    }
    ```

    ##### Errors

    - `400 Bad Request` if the username is missing, too long, or contains characters that aren't allowed.
    - `400 Bad Request` (`Same username.`) if the new username is the same as your current one.
    - `400 Bad Request` (`Username already taken.`) if another user already has that username.
    - `401 Unauthorized` (`You need to be authorized to change settings.`) if not logged in.
    - `403 Forbidden` if missing the `user` scope.
</details>

## Update the avatar

Updates the currently logged in user's avatar.

<details>
    <summary><code>PATCH /api/v3/settings/avatar</code></summary>

    You should make the request as a `multipart/form-data` request, with the image attached in a field named `file`.

    ##### Required scope

    `user`

    ##### Errors

    - `400 Bad Request` if the `file` field is missing.
    - `401 Unauthorized` (`You need to be authorized to change settings.`) if not logged in.
    - `403 Forbidden` if missing the `user` scope.
</details>
