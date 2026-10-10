---
sidebar_position: 8
slug: /images
---

# Images

These are the docs for the images endpoint. Used for fetching images (mainly user's avatars).

## Get an image

Returns an image based on its ID. To get the user's avatar, use the `avatarId` field.

<details>
    <summary><code>GET /api/v3/images/<b>:id</b></code></summary>

    ##### Response

    The raw image bytes. The `Content-Type` header is set to the stored image's type (e.g. `image/png`).

    If the image doesn't exist (or the ID is malformed), it returns `404 Not Found`:

    ```json
    {
        "statusCode": 404,
        "message": "Image not found."
    }
    ```
</details>
