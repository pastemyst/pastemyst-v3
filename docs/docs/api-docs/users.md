---
sidebar_position: 3
slug: /users
---

# Users

These are the docs for the users endpoint, which you can use fetch information about registered users.

## Get a user

Returns a single user by username or ID.

<details>
    <summary><code>GET /api/v3/users/<b>:username</b></code></summary>

    You can also get a user by ID by making the following request:

    `GET /api/v3/users?id=userId`

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
</details>

## Get pastes of a user

Returns all of the pastes of a single user.

<details>
    <summary><code>GET /api/v3/users/<b>:username</b>/pastes</code></summary>

    This endpoint has pagination. It defaults to 15 items per page. Page is 0-indexed. Use it like so:

    `GET /api/v3/users/:username/pastes?page=3&pageSize=5`

    You can also return all of the pastes with a specific tag like so:

    `GET /api/v3/users/:username/pastes?tag=epik`

    You must be logged in as your own user to fetch pastes with a tag, and have the `user:read` scope.

    ---

    If getting the pastes of a user that is not yours, and that user's setting "show all pastes on your profile" is disabled, you will get an empty list. This is not the case for pinned pastes (see the endpoint below this one).

    If not logged in as your own user, it won't return private pastes. But if you are logged in as your own user, you must have the `user` scope.

    ##### Required scope

    - `paste:read` (only if getting your own pastes, and want to fetch private pastes as well)
    - `user:read` (only if getting your own pastes and want to search by tag)

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
                    "createdAt": "2025-03-26T18:23:18.941Z",
                    "deletesAt": null,
                    "expiresIn": "never",
                    "id": "1m8aa9xq",
                    "ownerId": "0vzj9trh",
                    "pinned": false,
                    "private": false,
                    "stars": 0,
                    "tags": [],
                    "title": ""
                }
            }
        ]
    }
    ```
</details>

## Get pinned pastes of a user

Returns only the pinned pastes of a single user.

<details>
    <summary><code>GET /api/v3/users/<b>:username</b>/pastes/pinned</code></summary>

    This endpoint has pagination. It defaults to 15 items per page. Page is 0-indexed. Use it like so:

    `GET /api/v3/users/:username/pastes/pinned?page=3&pageSize=5`

    ---

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
                    "createdAt": "2025-03-26T18:23:18.941Z",
                    "deletesAt": null,
                    "expiresIn": "never",
                    "id": "1m8aa9xq",
                    "ownerId": "0vzj9trh",
                    "pinned": false,
                    "private": false,
                    "stars": 0,
                    "tags": [],
                    "title": ""
                }
            }
        ]
    }
    ```
</details>

## Get the user's tags

Returns a lit of all tags of a user. You must be logged in as your own user.

<details>
    <summary><code>GET /api/v3/users/<b>:username</b>/tags</code></summary>

    This will also delete everything associated with the user like pastes, and stars.

    ##### Required scope

    `user:read`

    ##### Response

    ```json
    ["epik", "more epik"]
    ```
</details>

## Download user data as a zip file

Downloads all the data associated with a user as a zip file. You must be logged in as your own user.

<details>
    <summary><code>GET /api/v3/users/<b>:username</b>.zip</code></summary>

    The zip file will contain all of the pastes, encrypted pastes, access tokens (encrypted), action logs, avatars and the profile itself.

    ##### Required scope

    `user`

    ##### Response

    Zip file.
</details>

## Delete a user

Deletes a single user. You must be logged in as your own user.

<details>
    <summary><code>DELETE /api/v3/users/<b>:username</b></code></summary>

    This will also delete everything associated with the user like pastes, and stars.

    ##### Required scope

    `user`
</details>