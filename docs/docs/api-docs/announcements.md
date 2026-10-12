---
description: "PasteMyst v3 API docs for announcements: get the latest or all announcements, and manage them as an admin."
sidebar_position: 9
slug: /announcements
---

# Announcements

These are the docs for the announcements endpoint, which is used for fetching announcements made on PasteMyst. These announcements are shown on the front page, usually to inform users of new releases.

## Get the latest announcement

Returns the most recent announcement.

<details>
    <summary><code>GET /api/v3/announcements/latest</code></summary>

    The content is markdown text.

    ##### Response

    ```json
    {
        "id": "67c4ad25b1e8a4f5c2d3e4f6",
        "createdAt": "2025-03-02T19:06:29.538Z",
        "title": "new pastemyst release",
        "content": "a **new** pastemyst version has just been released!"
    }
    ```

    If there are no announcements, it returns `404 Not Found`:

    ```json
    {
        "statusCode": 404,
        "message": "No announcements found."
    }
    ```
</details>

## Get all announcements

Returns all of the announcements, sorted from newest to oldest.

<details>
    <summary><code>GET /api/v3/announcements</code></summary>

    The content is markdown text.

    ##### Response

    ```json
    [
        {
            "id": "67c4ad25b1e8a4f5c2d3e4f6",
            "createdAt": "2025-03-02T19:06:29.538Z",
            "title": "new pastemyst release",
            "content": "a **new** pastemyst version has just been released!"
        }
    ]
    ```
</details>

## Create a new announcement

Creates a new announcement to be shown on the front page. You must be logged in as an admin user to create announcements.

<details>
    <summary><code>POST /api/v3/announcements</code></summary>

    The content is markdown text. Both `title` and `content` are required.

    ##### Request

    ```json
    {
        "title": "new pastemyst release",
        "content": "a **new** pastemyst version has just been released!"
    }
    ```

    ##### Response

    Status `200 OK` with an empty body.

    If you're not logged in as an admin, it returns `401 Unauthorized`:

    ```json
    {
        "statusCode": 401,
        "message": "Only admins can create new announcements."
    }
    ```
</details>

## Edit an announcement

Edits a specific announcement. You must be logged in as an admin user to edit announcements.

<details>
    <summary><code>PATCH /api/v3/announcements/<b>:id</b></code></summary>

    The content is markdown text. Both `title` and `content` are required, and both are replaced.

    ##### Request

    ```json
    {
        "title": "new pastemyst release",
        "content": "a **new** pastemyst version has just been released!"
    }
    ```

    ##### Response

    Status `200 OK` with an empty body.

    If you're not logged in as an admin, it returns `401 Unauthorized`:

    ```json
    {
        "statusCode": 401,
        "message": "Only admins can edit announcements."
    }
    ```
</details>

## Delete an announcement

Deletes a single announcement. You must be logged in as an admin user to delete announcements.

<details>
    <summary><code>DELETE /api/v3/announcements/<b>:id</b></code></summary>

    ##### Response

    Status `200 OK` with an empty body.

    If you're not logged in as an admin, it returns `401 Unauthorized`:

    ```json
    {
        "statusCode": 401,
        "message": "Only admins can delete announcements."
    }
    ```
</details>
