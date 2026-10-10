---
sidebar_position: 3
slug: /users
---

# Users

These are the docs for the users endpoint, which you can use fetch information about registered users.

Unless noted otherwise, errors are returned in the usual v3 error format described in [getting started](../getting-started.md).

Usernames in all of these endpoints are matched as a whole name, ignoring case. `codemyst` finds the user `CodeMyst`, but `Code` doesn't.

## Get a user

Returns a single user by username or ID.

<details>
    <summary><code>GET /api/v3/users/<b>:username</b></code></summary>

    You can also get a user by ID by making the following request:

    `GET /api/v3/users?id=userId`

    If both a username and an `id` are given, the username is used.

    To get the avatar of the user as an image, check out the docs for the [images endpoint](/docs/api-docs/images.md).

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

    - `404 Not Found` with `User not found.` if the user doesn't exist, or if neither a username nor an `id` was given.
</details>

## Get pastes of a user

Returns the pastes of a single user, newest first.

<details>
    <summary><code>GET /api/v3/users/<b>:username</b>/pastes</code></summary>

    This endpoint has pagination. Page is 0-indexed, and `pageSize` must be between 1 and 30 (defaults to 15). Use it like so:

    `GET /api/v3/users/:username/pastes?page=3&pageSize=5`

    You can also return all of the pastes with a specific tag like so:

    `GET /api/v3/users/:username/pastes?tag=epik`

    You must be logged in as your own user to fetch pastes with a tag, and have the `user` or `user:read` scope.

    ---

    If getting the pastes of a user that is not yours, and that user's setting "show all pastes on your profile" is disabled, you will get an empty page (`"items": []`, with `currentPage`, `pageSize` and `totalPages` all set to `0`). This is not the case for pinned pastes (see the endpoint below this one).

    Private pastes are only returned if you are logged in as the user, and have the `paste` or `paste:read` scope. Without the scope, private pastes are simply left out.

    Tags of the pastes are only returned to the owner. For everyone else `tags` is always an empty list.

    The list includes encrypted pastes. For those, `encrypted` is `true` and `languageStats` is `null`, since the server can't read their contents.

    ##### Required scope

    - `paste` or `paste:read` (only if getting your own pastes, and want to fetch private pastes as well)
    - `user` or `user:read` (only if getting your own pastes and want to search by tag)

    ##### Response

    The response contains paging meta information. Each item contains a stripped down paste object, along with language statistics.

    ```json
    {
        "currentPage": 0,
        "hasNextPage": false,
        "pageSize": 15,
        "totalPages": 1,
        "items": [
            {
                "encrypted": false,
                "languageStats": [
                    {
                        "language": {
                            "aliases": [
                                "ts"
                            ],
                            "codemirrorMimeType": "application/typescript",
                            "codemirrorMode": "javascript",
                            "color": "#3178c6",
                            "extensions": [
                                ".ts",
                                ".cts",
                                ".mts"
                            ],
                            "name": "TypeScript",
                            "tmScope": "source.ts",
                            "type": "programming",
                            "wrap": false
                        },
                        "percentage": 66.66667
                    },
                    {
                        "language": {
                            "aliases": [
                                "csharp",
                                "cake",
                                "cakescript"
                            ],
                            "codemirrorMimeType": "text/x-csharp",
                            "codemirrorMode": "clike",
                            "color": "#178600",
                            "extensions": [
                                ".cs",
                                ".cake",
                                ".cs.pp",
                                ".csx",
                                ".linq"
                            ],
                            "name": "C#",
                            "tmScope": "source.cs",
                            "type": "programming",
                            "wrap": false
                        },
                        "percentage": 33.333336
                    }
                ],
                "paste": {
                    "createdAt": "2025-04-22T21:22:32.761Z",
                    "deletesAt": null,
                    "expiresIn": "never",
                    "id": "00cr3zik",
                    "ownerId": "0vzj9trh",
                    "pinned": false,
                    "private": false,
                    "stars": 0,
                    "tags": [],
                    "title": "test"
                }
            },
            {
                "encrypted": true,
                "languageStats": null,
                "paste": {
                    "createdAt": "2025-03-26T18:23:18.941Z",
                    "deletesAt": null,
                    "expiresIn": "never",
                    "id": "1m8aa9xq",
                    "ownerId": "0vzj9trh",
                    "pinned": false,
                    "private": false,
                    "stars": 0,
                    "tags": [],
                    "title": "secret stuff"
                }
            }
        ]
    }
    ```

    ##### Errors

    - `400 Bad Request` if `pageSize` is not between 1 and 30.
    - `404 Not Found` (`User not found.`) if the user doesn't exist.
    - `401 Unauthorized` (`You must be authorized to view paste tags.`) if using `tag` while not logged in as the user.
    - `403 Forbidden` if using `tag` without the `user` or `user:read` scope.
</details>

## Get pinned pastes of a user

Returns only the pinned pastes of a single user, newest first.

<details>
    <summary><code>GET /api/v3/users/<b>:username</b>/pastes/pinned</code></summary>

    This endpoint has pagination. Page is 0-indexed, and `pageSize` must be between 1 and 30 (defaults to 15). Use it like so:

    `GET /api/v3/users/:username/pastes/pinned?page=3&pageSize=5`

    ---

    Pinned pastes are always returned, even if the user's setting "show all pastes on your profile" is disabled.

    Same as above, private pastes are only returned if you are logged in as the user and have the `paste` or `paste:read` scope, tags are only returned to the owner, and encrypted pastes have `"encrypted": true` and `"languageStats": null`.

    ##### Required scope

    - `paste` or `paste:read` (only if getting your own pastes, and want to fetch private pastes as well)

    ##### Response

    The response contains paging meta information. Each item contains a stripped down paste object, along with language statistics.

    ```json
    {
        "currentPage": 0,
        "hasNextPage": false,
        "pageSize": 15,
        "totalPages": 1,
        "items": [
            {
                "encrypted": false,
                "languageStats": [
                    {
                        "language": {
                            "aliases": [
                                "ts"
                            ],
                            "codemirrorMimeType": "application/typescript",
                            "codemirrorMode": "javascript",
                            "color": "#3178c6",
                            "extensions": [
                                ".ts",
                                ".cts",
                                ".mts"
                            ],
                            "name": "TypeScript",
                            "tmScope": "source.ts",
                            "type": "programming",
                            "wrap": false
                        },
                        "percentage": 100
                    }
                ],
                "paste": {
                    "createdAt": "2025-04-22T21:22:32.761Z",
                    "deletesAt": null,
                    "expiresIn": "never",
                    "id": "00cr3zik",
                    "ownerId": "0vzj9trh",
                    "pinned": true,
                    "private": false,
                    "stars": 0,
                    "tags": [],
                    "title": "test"
                }
            }
        ]
    }
    ```

    ##### Errors

    - `400 Bad Request` if `pageSize` is not between 1 and 30.
    - `404 Not Found` (`User not found.`) if the user doesn't exist.
</details>

## Get the user's tags

Returns a list of all tags used on the user's pastes. You must be logged in as your own user.

<details>
    <summary><code>GET /api/v3/users/<b>:username</b>/tags</code></summary>

    Tags are collected from all of your pastes (including private ones), except encrypted pastes. Each tag appears only once.

    ##### Required scope

    `user` or `user:read`

    ##### Response

    ```json
    ["epik", "more epik"]
    ```

    ##### Errors

    - `401 Unauthorized` (`You must be authorized to get your own tags.`) if not logged in.
    - `401 Unauthorized` (`You can only fetch your own tags.`) if the username is not your own.
    - `403 Forbidden` if missing the `user` or `user:read` scope.
</details>

## Download user data as a zip file

Downloads all the data associated with a user as a zip file. You must be logged in as your own user.

<details>
    <summary><code>GET /api/v3/users/<b>:username</b>.zip</code></summary>

    The file is named `<username>_<yyyy-MM-dd_HH-mm-ss>.zip` (the time of the download, in UTC), and contains:

    - `pastes.json` - all of your pastes (not encrypted ones)
    - `encrypted_pastes.json` - all of your encrypted pastes, as they are stored (the contents stay encrypted)
    - `access_tokens.json` - all of your access tokens. The tokens themselves are only stored as SHA-512 hashes, so these can't be used to log in
    - `action_logs.json` - action logs about your user and your pastes
    - `avatar.<ext>` - your avatar image
    - `user.json` - your profile

    ##### Required scope

    `user`

    ##### Response

    Zip file (`application/zip`).

    ##### Errors

    - `401 Unauthorized` (`You must be authorized to download your own user data.`) if not logged in.
    - `403 Forbidden` if missing the `user` scope.
    - `401 Unauthorized` (`You can download user data only for your account.`) if the username is not your own.
</details>

## Delete a user

Deletes a single user. You must be logged in as your own user, or as an admin.

<details>
    <summary><code>DELETE /api/v3/users/<b>:username</b></code></summary>

    This will also delete the user's avatar, all of their pastes (including encrypted ones) and access tokens, and remove all of the stars they gave to other pastes.

    ##### Required scope

    `user`

    ##### Errors

    - `401 Unauthorized` (`You must be authorized to delete your account.`) if not logged in.
    - `403 Forbidden` if missing the `user` scope.
    - `401 Unauthorized` (`You can delete only your account.`) if the username is not your own and you're not an admin.
</details>
