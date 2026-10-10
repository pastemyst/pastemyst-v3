---
sidebar_position: 1
slug: /
---

# Getting Started

[PasteMyst](https://paste.myst.rs/) has an open and free API that allows you to do anything that you can do through the UI (pretty much).

This is the documentation for the latest version of the API (V3). Version 2 is deprecated and will be removed at some point in the future, and version 1 is now removed.

The base URL for the API is `https://paste.myst.rs/api/v3`.

You don't need an API key to access the API, this of course means you will be able to do only what anonymous users can do. To be able to create private pastes, edit pastes, and so on, you will need to get an API key. To get one, log in on the website, go to settings, and select the "access tokens" menu (https://paste.myst.rs/settings/access_tokens). Access tokens are scoped, so you can give it the permissions you need exactly.

In these docs, whenever it says that you must be logged in to do something, it means you must provide an access token.

The V2 API is still working, but it is deprecated. You can view the docs [here](v2-docs/v2-api-docs.md).

The V1 API is now **removed**.

## Conventions

### Authorization

To use the access token send it in the `Authorization` header like so:

```http
Authorization: Bearer <token>
```

V3 access tokens look like `<8 character id>-<secret>`.

Sending just the token without `Bearer` also works (that's needed for API keys migrated from V2). Note that `Bearer` is matched case-sensitively, so `bearer <token>` won't work.

An invalid or expired token doesn't return an error, the request is just treated as if you're not logged in. So if you get an unexpected `401` or `404`, double check your token.

### Scopes

These are the scopes an access token can have:

- `paste` - read and write access to your pastes
- `paste:read` - read access to your pastes
- `user` - read and write access to your user info
- `user:read` - read access to your user info
- `user:access_tokens` - read and write access to your access tokens

`paste` includes everything `paste:read` allows, and `user` includes everything `user:read` allows.

A missing scope usually returns `403` with the message `Missing required scope <scope>.` (reading a private paste without the scope returns `404` instead, so it's not exposed that the paste exists).

### Errors

Errors are returned as JSON with the HTTP status code and a message:

```json
{
    "statusCode": 404,
    "message": "Paste not found"
}
```

Invalid request bodies (like a title that's too long, or a missing required field) return `400` with the same shape, the message contains the validation errors.

Unexpected errors return `500` with the message `Internal server error.`.

The V2 API has its own error format, see the V2 docs.

### Rate limits

Requests are rate limited per client IP. The limits work as token buckets:

- all requests: you can make a burst of up to 100 requests, refilling at 30 requests per second
- creating pastes: on top of the general limit, a burst of up to 5 pastes, refilling at 1 paste per minute

After exceeding a rate limit you will get a `429 (Too Many Requests)` response, with a `Retry-After` header (in seconds) and this body:

```json
{
    "statusCode": 429,
    "message": "Too many requests, please slow down."
}
```

### Encrypted pastes

To read (or edit) an encrypted paste, send the encryption key in the `Encryption-Key` header:

```http
Encryption-Key: <key>
```

(The website instead uses a `pastemyst-encryption-key-<pasteId>` cookie, which works too.)

- no key: `400` with `Missing encryption key`
- wrong key: `400` with `Invalid encryption key.`
- pastes encrypted back on V2 can return `503` with `Too many encrypted pastes are being opened right now, please try again in a moment.` when a lot of them are being decrypted at once, just retry a bit later
