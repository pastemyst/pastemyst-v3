---
sidebar_position: 1
slug: /
---

# Getting Started

[PasteMyst](https://paste.myst.rs/) has an open and free API that allows you to do anything that you can do through the UI (pretty much).

This is the documentation for the latest version of the API (V3). Version 2 is deprecated and will be removed at some point in the future, and version 1 is now removed.

The base URL for the API is `https://paste.myst.rs/api/v3`.

You don't need an API key to access the API, this of course means you will be able to do only what anonymous users can do. To be able to create private pastes, edit pastes, and so on, you will need to get an API key. To get one, go to your profile settings, and select the "access tokens" menu (https://paste.myst.rs/settings/access_tokens). Access tokens are scoped, so you can give it the permissions you need exactly.

In these docs, whenever it says that you must be logged in to do something, it means you must provide an access token.

The API is rate limited to 5 requests per second. After exceeding the rate limit you will get a `429 (Too Many Requests)` response.

To use the access token send it in the `Authorization` header like so:

```http
Authorization: Bearer <token>
```
