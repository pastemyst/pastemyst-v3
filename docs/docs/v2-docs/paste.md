---
sidebar_position: 4
slug: /v2/pastes
---

# Pastes

These are the docs for the paste endpoints. Used for creating and fetching pastes.

It's not possible to fetch or create encrypted pastes with this API (use the [V3 API](../api-docs/pastes.md) for this).

## Get a paste

Returns a single paste.

<details>
    <summary><code>GET /api/v2/paste/<b>:pasteId</b></code></summary>

    To view private pastes you must be logged in as the author of the paste.

    If you are not logged in or not logged in as the author of the paste the returned tags array will be empty.

    The `edits` array will always return an empty list, since the way the edits are handled in V3 is different, and it would be too much work to make it backwards compatible (and editing wasn't used much in V2).

    ##### Required scope

    `paste:read` (if logged in)

    ##### Response

    ```json
    {
        "_id": "jchfytgr",
        "ownerId": "",
        "title": "epik paste",
        "createdAt": 1680780502,
        "expiresIn": "never",
        "deletesAt": 0,
        "stars": 0,
        "isPrivate": false,
        "isPublic": false,
        "tags": [],
        "edits": [],
        "pasties": [
            {
                "_id": "fq1339ar",
                "language": "TypeScript",
                "title": "",
                "code": "..."
            }
        ]
    }
    ```
</details>

## Create a paste

Creates a single paste.

<details>
    <summary><code>POST /api/v2/paste</code></summary>

    If you want the paste to be tied to your account, or create a private/public paste, or want to use tags you need to be logged in.

    ##### Required scope

    `paste` (if logged in)

    ##### Request

    ```json
    {
        "title": "epik paste",
        "expiresIn": "1w",
        "isPrivate": false,
        "isPublic": false,
        "tags": [],
        "pasties": [
            {
                "language": "TypeScript",
                "title": "",
                "code": "epik code" 
            }
        ]
    }
    ```

    ##### Response

    Returns the full paste object, same as getting a paste.
</details>

## Edit a paste

Edits a single paste.

<details>
    <summary><code>PATCH /api/v2/paste/<b>:pasteId</b></code></summary>

    You must be logged in as the owner of the paste.

    To edit an existing pasty, in the pasties array provide the ID of the existing pasty.

    If you don't provide a pasty ID that exists in the original paste, it will be deleted.

    If you provide a pasty without an ID, it will be created as a new one.

    ##### Required scope

    `paste` (if logged in)

    ##### Request

    ```json
    {
        "title": "epik paste",
        "pasties": [
            {
                "_id": "hgbugyshb",
                "language": "TypeScript",
                "title": "",
                "code": "epik code" 
            }
        ]
    }
    ```

    ##### Respone

    Returns the full paste object, same as getting a paste.
</details>

## Delete a paste

Deletes a single paste.

<details>
    <summary><code>DELETE /api/v2/paste/<b>:pasteId</b></code></summary>

    You can only deletes your own pastes, so you must be logged in. This action is irreversible.

    ##### Required scope

    `paste` (if logged in)
</details>