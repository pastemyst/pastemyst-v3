---
sidebar_position: 1
slug: /v2/api-docs
---

# API V2 Docs (deprecated)

These are the docs of the deprecated V2 API. You should use the latest [V3 API](/) if possible.

The base endpoint for the V2 API is `https://paste.myst.rs/api/v2`.

The V2 API is a compatibility layer on top of PasteMyst V3. It tries to behave the same as the original V2 API, but there are some differences, listed [below](#differences-from-the-original-v2).

## Authorization

The old way of providing the API key (without the Bearer keyword) is still valid.

```sh
Authorization: <token>
```

The old API keys from PasteMyst V2 are migrated to V3 and are still valid. They are given the `paste` and `user` scopes, which cover everything the V2 API can do. It is not possible to view this token again, so if you rely on this token make sure you have it saved somewhere!

V3 access tokens also work with the V2 API, either with the Bearer keyword (`Authorization: Bearer <token>`) or without it. The scopes of the access token apply, so a token without the needed scope gets a `403` with a message like `Missing required scope paste.`.

## Errors

Errors are returned with the appropriate HTTP status code and a JSON body with a `statusMessage` field, same as in V2. This includes validation errors (like a missing required field).

```json
{
    "statusMessage": "Paste not found"
}
```

## Rate limiting

Requests are rate limited per IP address:

- every request: a burst of 100 requests, refilling at 30 requests per second
- creating a paste (`POST /api/v2/paste`), on top of the general limit: a burst of 5 pastes, refilling at 1 paste per minute

When you go over the limit you get a `429 Too Many Requests` response with a `Retry-After` header (in seconds, when available) and this body:

```json
{
    "statusMessage": "Too many requests, please slow down."
}
```

## Differences from the original v2

- **Tags** are only returned to the owner of the paste. For everyone else the `tags` array is empty.
- **Language names**: the V2 language names are accepted in requests. Where V3 renamed a language, the V2 name is returned in responses (`Plain Text`, `Vue.js Component`, `TypeScript-JSX`, `Properties files`, `PGP`). Pastes can still have languages that didn't exist in V2, and those are returned with their V3 names.
- **Encrypted pastes** can be fetched with `GET /api/v2/paste/:pasteId` by sending the key in the `Encryption-Key` header (without it you get a `400`). It's still not possible to create encrypted pastes with the V2 API, use the [V3 API](../api-docs/pastes.md) for that.
- **Edits**: the `edits` array is always empty. The V2 edit history wasn't carried over to V3, and the V3 edit history isn't exposed through the V2 API.
- **Rate limiting** applies, see [above](#rate-limiting).
- **Access tokens**: existing V2 API keys keep working with the `paste` and `user` scopes, and V3 access tokens work too, with their own scopes. See [Authorization](#authorization).
- **The V1 API** (`/api/paste`) was removed and answers every request with `410 Gone`.
- **Users**: `avatarUrl` now points to `/api/v3/images/:imageId`, and `supporterLength` is only `0` or `1` (whether the user is a supporter or not).
