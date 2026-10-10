---
sidebar_position: 4
slug: /v2/pastes
---

# Pastes

These are the docs for the paste endpoints. Used for creating and fetching pastes.

Encrypted pastes can be fetched by providing the key in the `Encryption-Key` header, but it's not possible to create encrypted pastes with this API (use the [V3 API](../api-docs/pastes.md) for this).

## Get a paste

Returns a single paste.

<details>
    <summary><code>GET /api/v2/paste/<b>:pasteId</b></code></summary>

    To view private pastes you must be logged in as the author of the paste.

    If you are not logged in or not logged in as the author of the paste the returned tags array will be empty.

    To fetch an encrypted paste provide the encryption key in the `Encryption-Key` header. The paste is returned decrypted. The `encrypted` field in the response is always `false`.

    The `edits` array is always empty: the edit history from V2 wasn't carried over to V3, and the V3 edit history isn't exposed through the V2 API.

    ##### Required scope

    `paste:read` (only for private pastes)

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
        "encrypted": false,
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

    `ownerId` is an empty string for anonymous pastes, and `deletesAt` is `0` for pastes that never expire.

    ##### Errors

    - `404` with `"Paste not found"` if the paste doesn't exist, has expired, or is private and you're not its owner
    - `400` with `"Missing encryption key"` if the paste is encrypted and the `Encryption-Key` header is missing
    - `400` with `"Invalid encryption key."` if the encryption key is wrong
</details>

## Create a paste

Creates a single paste.

<details>
    <summary><code>POST /api/v2/paste</code></summary>

    If you want the paste to be tied to your account, or create a private/public paste, or want to use tags you need to be logged in. If you are logged in, the paste is always tied to your account.

    `tags` is a comma-separated string (like `"tag1,tag2"`), not an array. Responses return the tags as an array.

    All fields except `pasties` are optional:

    | Field | Default | Notes |
    | --- | --- | --- |
    | `title` | `""` | max 128 characters |
    | `expiresIn` | `"never"` | one of `never`, `1h`, `2h`, `10h`, `1d`, `2d`, `1w`, `1m`, `1y` |
    | `isPrivate` | `false` | requires being logged in |
    | `isPublic` | `false` | shows the paste on your profile, requires being logged in |
    | `tags` | `""` | comma-separated, requires being logged in |
    | `pasties` | | at least 1 pasty is required |

    For every pasty, `code` is required, `title` is optional (max 50 characters), and `language` is optional (defaults to `Plain Text`, `Autodetect` detects the language from the code).

    ##### Required scope

    `paste` (if logged in)

    ##### Request

    ```json
    {
        "title": "epik paste",
        "expiresIn": "1w",
        "isPrivate": false,
        "isPublic": false,
        "tags": "tag1,tag2",
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

    ##### Errors

    - `403` with `"can't create a paste using account features without providing a valid Authorization header."` if `isPrivate`, `isPublic` or `tags` is used without a valid API key
    - `403` with `"Missing required scope paste."` if your token doesn't have the `paste` scope
    - `400` with `"the paste can't be private and shown on the profile"` if both `isPrivate` and `isPublic` are `true`
    - `400` with `"invalid language value."` if a pasty's language doesn't exist
    - `400` for invalid requests (missing `pasties` or `code`, too long titles, invalid `expiresIn`, ...)
    - `429` if you're creating pastes too quickly, see [rate limiting](./v2-api-docs.md#rate-limiting)
</details>

## Edit a paste

Edits a single paste.

<details>
    <summary><code>PATCH /api/v2/paste/<b>:pasteId</b></code></summary>

    You must be logged in as the owner of the paste.

    Every field is optional, fields you leave out stay unchanged. You can edit `title`, `isPrivate`, `isPublic`, `tags` (comma-separated string, like when creating) and `pasties`.

    If you provide `pasties`, it's the new full list of pasties:

    - To edit an existing pasty, provide its `_id`.
    - A pasty without an `_id` (or with an empty one) is added as a new pasty.
    - Existing pasties that aren't in the list are deleted.

    The same limits as when creating apply (title max 128 characters, pasty title max 50 characters, `code` is required, at least 1 pasty). A pasty without `language` is set to `Plain Text`.

    To edit an encrypted paste provide the encryption key in the `Encryption-Key` header.

    ##### Required scope

    `paste` (and `user` for changing `isPublic`)

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

    ##### Response

    Returns the full paste object, same as getting a paste.

    ##### Errors

    - `404` with `"Not Found"` if you're not logged in, not the owner of the paste, or the paste is anonymous
    - `404` with `"Paste not found"` if the paste doesn't exist (or is private and not yours)
    - `400` with `"paste can't be private and shown on the profile at the same time"` if the paste would end up both private and public
    - `400` with `"invalid language value."` if a pasty's language doesn't exist
    - `403` with `"Missing required scope ..."` if your token doesn't have the required scope
</details>

## Delete a paste

Deletes a single paste.

<details>
    <summary><code>DELETE /api/v2/paste/<b>:pasteId</b></code></summary>

    You can only delete your own pastes, so you must be logged in. This action is irreversible.

    On success returns `200 OK` with an empty body.

    ##### Required scope

    `paste`

    ##### Errors

    - `404` with `"Not Found"` if you're not logged in, not the owner of the paste, or your token doesn't have the `paste` scope
    - `404` with `"Paste not found"` if the paste doesn't exist (or is private and not yours)
</details>
