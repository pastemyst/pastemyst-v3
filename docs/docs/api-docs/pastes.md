---
description: "PasteMyst v3 API docs for pastes: create, get, edit, delete, star, pin and make pastes private, plus stats, history, diffs and zip downloads."
sidebar_position: 2
slug: /pastes
---

# Pastes

These are the docs for the pastes endpoint, which you can use to create and return pastes (and much more).

For the error format, rate limits, scopes and how to send the encryption key, see the [conventions](../getting-started.md#conventions).

## Paste object

This is the structure of a single paste object.

```json
{
    "id": "1m8aa9xq",
    "title": "api test paste",
    "createdAt": "2025-03-26T18:23:18.941Z",
    "editedAt": "2025-03-26T18:23:18.941Z", // nullable
    "deletesAt": "2025-03-27T18:23:18.941Z", // timestamp when the paste expires, nullable
    "expiresIn": "1d",
    "ownerId": "0vzj9trh", // nullable
    "pinned": false,
    "private": false,
    "stars": 0,
    "tags": ["epik"],
    "pasties": [
        {
            "id": "96lbou8v",
            "title": "untitled",
            "content": "some paste content",
            "language": "Text"
        }
    ]
}
```

These are the valid `expiresIn` values: `never`, `1h`, `2h`, `10h`, `1d`, `2d`, `1w`, `1m`, `1y`.

The `language` of a pasty is always the full language name (like `Text`, `C#`, `TypeScript`), even if you created it with an alias or an extension.

## Get a paste

Returns a single paste. 

<details>
    <summary><code>GET /api/v3/pastes/<b>:pasteId</b></code></summary>

    Public pastes can be read by anyone, no token needed.

    To view a private paste you must be logged in as the author of the paste, with the `paste` or `paste:read` scope. Otherwise you get a `404` with `Paste not found`, same as for a paste that doesn't exist or has expired.

    If you are not logged in or not logged in as the author of the paste the returned tags array will be empty.

    To view an encrypted paste, you must provide the encryption key as an HTTP header like so:

    ```http
    Encryption-Key: <key>
    ```

    Missing key returns `400` with `Missing encryption key`, a wrong key returns `400` with `Invalid encryption key.`.

    ##### Required scope

    None for public pastes, `paste` or `paste:read` for private pastes.

    ##### Response

    ```json
    {
        "id": "1m8aa9xq",
        "title": "api test paste",
        "createdAt": "2025-03-26T18:23:18.941Z",
        "editedAt": "2025-03-26T18:23:18.941Z", // nullable
        "deletesAt": "2025-03-27T18:23:18.941Z", // timestamp when the paste expires, nullable
        "expiresIn": "1d",
        "ownerId": "0vzj9trh", // nullable
        "pinned": false,
        "private": false,
        "stars": 0,
        "tags": ["epik"],
        "pasties": [
            {
                "id": "96lbou8v",
                "title": "untitled",
                "content": "some paste content",
                "language": "Text"
            }
        ]
    }
    ```
</details>

## Create a paste

Creates a new paste.

<details>
    <summary><code>POST /api/v3/pastes</code></summary>

    Creating pastes has its own, stricter [rate limit](../getting-started.md#rate-limits) (a burst of 5, then 1 per minute) on top of the general one.

    Pastes created without a token are always anonymous, sending `anonymous: false` while logged out is not an error.

    To create a pinned, private, or tagged paste, you must be logged in.

    A pinned paste can't be private or anonymous.

    A private paste can't be anonymous.

    A tagged paste can't be anonymous.

    To create an encrypted paste, set `encrypted: true` and provide the key in the `Encryption-Key` header.

    The paste title can be at most 128 characters, and a pasty title at most 50 characters. There must be at least 1 pasty.

    The pasty `language` is matched against language names, aliases and extensions (case-insensitive), so `Text`, `ts`, `py` all work. `Autodetect` will detect the language from the content. If it's not provided (or `null`) it will be `Text`.

    Tags get trimmed, and empty tags are dropped.

    ##### Required scope

    `paste` (only if logged in and not anonymous)

    ##### Request

    ```json
    {
        "title": "some title", // optional, max 128 characters
        "expiresIn": "never", // optional (defaults to never), allowed values: never, 1h, 2h, 10h, 1d, 2d, 1w, 1m, 1y
        "anonymous": false, // optional (defaults to false)
        "private": false, // optional (defaults to false)
        "pinned": false, // optional (defaults to false)
        "encrypted": false,  // optional (defaults to false)
        "tags": [], // optional
        "pasties": [ // at least 1
            {
                "title": "some title", // optional, max 50 characters
                "content": "some content", // optional (defaults to "")
                "language": "Text" // optional (defaults to Text)
            }
        ]
    }
    ```

    ##### Response

    `201 Created`, with the `Location` header set to `/api/v3/pastes/<id>`, and the created paste in the body, same as [getting a single paste](#get-a-paste).

    ##### Errors

    These are checked in this order:

    - `401` `Can't create a pinned paste while unauthorized.` - pinned while logged out
    - `401` `Can't create a private or anonymous pinned paste.` - pinned and private or anonymous
    - `401` `Can't create a private paste while unauthorized.` - private while logged out
    - `400` `Can't create a private anonymous paste.` - private and anonymous
    - `401` `Can't create a tagged paste while unauthorized.` - tags while logged out
    - `400` `Can't create a tagged anonymous paste.` - tags and anonymous
    - `403` `Missing required scope paste.` - logged in, not anonymous, and the token doesn't have the `paste` scope
    - `404` `The language was not found.` - unknown pasty language
    - `400` `Missing encryption key` - `encrypted: true` without the `Encryption-Key` header
    - `400` - invalid request body (title too long, no pasties, invalid `expiresIn`, etc.)
    - `429` - over the rate limit
</details>

## Get paste stats

Returns the line count, word count and total bytes of a paste. All rules apply as getting a single paste.

<details>
    <summary><code>GET /api/v3/pastes/<b>:pasteId</b>/stats</code></summary>

    ##### Response

    ```json
    {
        "bytes": 891,
        "lines": 21,
        "pasties": {
            "96lbou8v": {
                "bytes": 891,
                "lines": 21,
                "words": 80
            }
        },
        "words": 80
    }
    ```
</details>

## Get paste language stats

Returns the language statistics of a single paste. All rules apply as getting a single paste.

<details>
    <summary><code>GET /api/v3/pastes/<b>:pasteId</b>/langs</code></summary>

    ```json
    [
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
            "percentage": 67.00337
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
            "percentage": 32.996635
        }
    ]
    ```
</details>

## Get the compact history of a paste

Returns the list of all edits of paste, but only the ID of that edit, and when that edit was made. The newest edit is first. All rules apply as getting a single paste.

<details>
    <summary><code>GET /api/v3/pastes/<b>:pasteId</b>/history_compact</code></summary>

    ##### Response

    ```json
    [
        {
            "editedAt": "2025-04-22T21:29:10.134Z",
            "id": "c6xfkaes"
        },
        {
            "editedAt": "2025-04-22T21:29:06.534Z",
            "id": "smu5ui1w"
        }
    ]
    ```
</details>

## Get the paste at a specific edit

Returns the paste at a specific point in time based on the history ID. All rules apply as getting a single paste.

<details>
    <summary><code>GET /api/v3/pastes/<b>:pasteId</b>/history/<b>:historyId</b></code></summary>

    The `title` and `pasties` are the ones the paste had right before that edit was made, everything else is the same as the current paste.

    If there's no edit with that ID, it returns `404` with `Edit not found.`.

    ##### Response

    It returns the exact same data as [getting a single paste](#get-a-paste).
</details>

## Get the diff of the paste at a certain edit

Returns the diff of the paste at a certain point in the history with the version before the specified edit. All rules apply as getting a single paste.

<details>
    <summary><code>GET /api/v3/pastes/<b>:pasteId</b>/history/<b>:historyId/diff</b></code></summary>

    `oldPaste` is the paste right before the edit, `newPaste` is the paste right after the edit. If it's the latest edit, `newPaste` is the current version of the paste (its `id` is `null`).

    If there's no edit with that ID, it returns `404` with `Edit not found.`.

    ##### Response

    ```json
    {
        "currentPaste": {}, // the latest paste data irrelevant of the edits, same object as getting a single paste
        "newPaste": {
            "editedAt": "...",
            "id": "...",
            "title": "...",
            "pasties": []

        },
        "oldPaste": {
            "editedAt": "...",
            "id": "...",
            "title": "...",
            "pasties": []
        }
    }
    ```
</details>

## Download the paste as a zip file

Downloads the entire paste as a single zip file. All rules apply as getting a single paste (so private pastes need you to be logged in as the author, and encrypted pastes need the key).

<details>
    <summary><code>GET /api/v3/pastes/<b>:pasteId</b>.zip</code></summary>

    ##### Response

    A single zip file (`application/zip`), named `<paste title>.zip`.

    Each pasty is a file in the zip, named as the pasty title plus the first extension of its language (e.g. `untitled.txt` for a `Text` pasty titled `untitled`).
</details>

## Check if the paste is encrypted

Checks if the paste is encrypted. If the paste is private you must be logged in as the author of the paste (with the `paste` or `paste:read` scope). You don't need the encryption key for this.

<details>
    <summary><code>GET /api/v3/pastes/<b>:pasteId</b>/encrypted</code></summary>

    ##### Response

    A JSON boolean: `true` or `false`
</details>

## Check if you starred a paste

Checks if you starred a paste. You must be logged in. All rules apply as getting a single paste.

<details>
    <summary><code>GET /api/v3/pastes/<b>:pasteId</b>/star</code></summary>

    ##### Required scope

    `user:read` (stars belong to your user)

    ##### Response

    A JSON boolean: `true` or `false`

    ##### Errors

    - `401` `You must be authorized to star pastes.` - logged out
    - `403` `Missing required scope user:read.` - the token doesn't have `user` or `user:read`
</details>

## Star a paste

Stars a paste. If you already starred the paste, you will unstar it. You must be logged in. All rules apply as getting a single paste.

<details>
    <summary><code>POST /api/v3/pastes/<b>:pasteId</b>/star</code></summary>

    ##### Required scope

    `user` (stars belong to your user)

    ##### Errors

    - `401` `You must be authorized to star pastes.` - logged out
    - `403` `Missing required scope user.` - the token doesn't have the `user` scope
</details>

## Pin a paste

Pins a paste on your profile. If the paste is already pinned, you will unpin it. You must be logged in as the author of the paste, since you can only pin your own pastes.

<details>
    <summary><code>POST /api/v3/pastes/<b>:pasteId</b>/pin</code></summary>

    You can't pin a paste that's private.

    All rules apply as getting a single paste.

    ##### Required scope

    `user`

    ##### Errors

    - `401` `You must be authorized to pin/unpin pastes.` - logged out
    - `403` `Missing required scope user.` - the token doesn't have the `user` scope
    - `400` `Only owned pastes can be pinned.` - the paste is anonymous
    - `401` `You can only pin/unpin your own pastes.` - you're not the author
    - `400` `You can't pin private pastes.` - the paste is private
</details>

## Set a paste to be private

Sets a paste to be private. If the paste is already private, it will make it public. You must be logged in as the author of the paste.

<details>
    <summary><code>POST /api/v3/pastes/<b>:pasteId</b>/private</code></summary>

    You can't set a paste to be private if it's pinned.

    All rules apply as getting a single paste.

    ##### Required scope

    `paste`

    ##### Errors

    - `401` `You must be authorized to change the private status of pastes.` - logged out
    - `403` `Missing required scope paste.` - the token doesn't have the `paste` scope
    - `400` `Only owned pastes can be set/unset to private.` - the paste is anonymous
    - `401` `You can only change the private status of your own pastes.` - you're not the author
    - `400` `You can't private pinned pastes.` - the paste is pinned
</details>

## Edit a paste

Edits a single paste. You must be logged in as the author of the paste.

<details>
    <summary><code>PATCH /api/v3/pastes/<b>:pasteId</b></code></summary>

    The `pasties` array is required, and it replaces the pasties of the paste:

    - to edit an existing pasty, provide its ID
    - a pasty without an ID will be created as a new one
    - existing pasties that aren't in the array will be removed

    The title can be at most 128 characters (leaving it out clears the title), and a pasty title at most 50 characters.

    The pasty `language` works the same as when [creating a paste](#create-a-paste) (name, alias or extension, `Autodetect`, defaults to `Text`).

    To edit an encrypted paste you must provide the encryption key. All rules apply as getting a single paste.

    The previous version of the paste is saved in the history.

    ##### Required scope

    `paste`

    ##### Request

    ```json
    {
        "title": "new paste title",
        "pasties": [
            {
                "id": "id of an existing pasty", // optional, leave out for a new pasty
                "title": "new pasty title",
                "content": "new pasty content",
                "language": "new pasty language"
            }
        ]
    }
    ```

    ##### Response

    Returns the new edited paste, same as [getting as single paste](#get-a-paste)

    ##### Errors

    - `401` `You must be authorized to edit pastes.` - logged out
    - `403` `Missing required scope paste.` - the token doesn't have the `paste` scope
    - `400` `Only owned pastes can be edited.` - the paste is anonymous
    - `401` `You can only edit your own pastes.` - you're not the author
    - `404` `The language was not found.` - unknown pasty language
    - `400` - invalid request body (title too long, etc.)
</details>

## Edit the tags of a paste

Edits the tags of a paste. You must be logged in as the author of the paste. The tags get trimmed, and empty tags are dropped. All rules apply as getting a single paste.

<details>
    <summary><code>PATCH /api/v3/pastes/<b>:pasteId</b>/tags</code></summary>

    ##### Required scope

    `paste`

    ##### Request

    ```json
    ["tag1", "tag2"]
    ```

    ##### Response

    Returns the new edited paste, same as [getting as single paste](#get-a-paste)

    ##### Errors

    - `401` `You must be authorized to edit tags.` - logged out
    - `403` `Missing required scope paste.` - the token doesn't have the `paste` scope
    - `400` `Only owned pastes can have their tags edited.` - the paste is anonymous
    - `401` `You can only edit tags of your own pastes.` - you're not the author
</details>

## Delete a paste

Delete a single paste. You must be logged in as the author of the paste. Admins can also delete any paste that isn't private.

<details>
    <summary><code>DELETE /api/v3/pastes/<b>:pasteId</b></code></summary>

    You don't need the encryption key to delete an encrypted paste.

    ##### Required scope

    `paste`

    ##### Errors

    - `401` `You must be authorized to delete pastes.` - logged out
    - `403` `Missing required scope paste.` - the token doesn't have the `paste` scope
    - `404` `Paste not found` - the paste doesn't exist, has expired, or is someone else's private paste
    - `401` `You can only delete your own pastes.` - you're not the author (and not an admin)
</details>
