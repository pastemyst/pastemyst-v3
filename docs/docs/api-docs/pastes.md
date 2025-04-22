---
sidebar_position: 2
slug: /pastes
---

# Pastes

These are the docs for the pastes endpoint, which you can use to create and return pastes (and much more).

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
            "language": "text"
        }
    ]
}
```

These are the valid `expiresIn` values: `never`, `1h`, `2h`, `10h`, `1d`, `2d`, `1w`, `1m`, `1y`.

## Get a paste

Returns a single paste. 

<details>
    <summary><code>GET /api/v3/pastes/<b>:pasteId</b></code></summary>

    To view private pastes you must be logged in as the author of the paste.

    If you are not logged in or not logged in as the author of the paste the returned tags array will be empty.

    To view an encrypted paste, you must provide the decryption key as an HTTP header like so:

    ```http
    Encryption-Key: <key>
    ```

    ##### Required scope

    `paste:read` (if logged in)

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
                "language": "text"
            }
        ]
    }
    ```
</details>

## Create a paste

Creates a new paste.

<details>
    <summary><code>POST /api/v3/pastes</code></summary>

    To create a pinned, private, or tagged paste, you must be logged in.

    To create a paste that is not anonymous (`anonymous: false`) you must be logged in.

    To create an encrypted paste, provide the `Encryption-Key` header.

    A pinned paste can't be private or anonymous.

    A private paste can't be pinned or anonymous.

    A tagged paste can't be anonymous.

    ##### Required scope

    `paste` (if not anonymous)

    ##### Request

    ```json
    {
        "title": "some title", // optional
        "expiresIn": "never", // optional (defaults to never), allowed values: never, 1h, 2h, 10h, 1d, 2d, 1w, 1m, 1y
        "anonymous": false, // optional (defaults to false if logged in)
        "private": false, // optional (defaults to false)
        "pinned": false, // optional (defaults to false)
        "encrypted": false,  // optional (defaults to false)
        "tags": [], // optional,
        "pasties": [
            {
                "title": "some title", // optional
                "content": "some content",
                "language": "text" // optional (defaults to text)
            }
        ]
    }
    ```
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

Returns the list of all edits of paste, but only the ID of that edit, and when that edit was made.

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

Returns the paste at a specific point in time based on the history ID.

<details>
    <summary><code>GET /api/v3/pastes/<b>:pasteId</b>/history/<b>:historyId</b></code></summary>

    ##### Response

    It returns the exact same data as [getting a single paste](#get-a-paste).
</details>

## Get the diff of the paste at a certain edit

Returns the diff of the paste at a certain point in the history with the version before the specified edit.

<details>
    <summary><code>GET /api/v3/pastes/<b>:pasteId</b>/history/<b>:historyId/diff</b></code></summary>

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

Downloads the entire paste as a single zip file. You must be logged in if it's a private paste.

<details>
    <summary><code>GET /api/v3/pastes/<b>:pasteId</b>.zip</code></summary>

    ##### Response

    A single zip file.
</details>

## Check if the paste is encrypted

Checks if the paste is encrypted. If the paste is private you must be logged in as the author of the paste.

<details>
    <summary><code>GET /api/v3/pastes/<b>:pasteId</b>/encrypted</code></summary>

    ##### Response

    A single string: `true` or `false`
</details>

## Check if you starred a paste

Checks if you starred a paste. You must be logged in.

<details>
    <summary><code>GET /api/v3/pastes/<b>:pasteId</b>/star</code></summary>

    ##### Response

    A single string: `true` or `false`
</details>

## Star a paste

Stars a paste. If you already starred the paste, you wil unstar it. You must be logged in.

<details>
    <summary><code>POST /api/v3/pastes/<b>:pasteId</b>/star</code></summary>
</details>

## Pin a paste

Pins a paste on your profile. If you already starred the paste, you wil unstar it. You must be logged in as the author of the paste, since you can only pin your own pastes.

<details>
    <summary><code>POST /api/v3/pastes/<b>:pasteId</b>/pin</code></summary>

    You can't pin a paste that's private.

    ##### Required scope

    `user`
</details>

## Set a paste to be private

Sets a paste to be private. If the paste is already private, it will make it public. You must be logged in as the author of the paste.

<details>
    <summary><code>POST /api/v3/pastes/<b>:pasteId</b>/private</code></summary>

    You can't set a paste to be private if it's pinned.

    ##### Required scope

    `paste`
</details>

## Edit a paste

Edits a single paste. You must be logged in as the author of the paste.

<details>
    <summary><code>PATCH /api/v3/pastes/<b>:pasteId</b></code></summary>

    To edit an existing pasty, in the `pasties` array provide the ID of the existing pasty.

    If you don't provide a pasty ID that exists in the original paste, it will be deleted.

    If you provide a pasty without an ID, it will be created as a new one.

    ##### Required scope

    `paste`

    ##### Request

    ```json
    {
        "title": "new paste title",
        "pasties": [
            {
                "id": "id of an existing pasty",
                "title": "new pasty title",
                "content": "new pasty content",
                "language": "new pasty language"
            }
        ]
    }
    ```

    ##### Response

    Returns the new edited paste, same as [getting as single paste](#get-a-paste)
</details>

## Edit the tags of a paste

Edits the tags of a paste. You must be logged in as the author of the paste.

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
</details>

## Delete a paste

Delete a single paste. You must be logged in as the author of the paste.

<details>
    <summary><code>DELETE /api/v3/pastes/<b>:pasteId</b></code></summary>

    ##### Required scope

    `paste`
</details>