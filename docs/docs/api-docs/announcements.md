---
sidebar_position: 7
slug: /announcements
---

# Announcements

These are the docs for the announcements endpoint, which is used for fetching announcements made on PasteMyst. These announcements are shown on the front page, usually to inform users of new releases.

## Get the latest announcement

Returns the most recent announcement.

<details>
    <summary><code>GET /api/v3/announcements/latest</code></summary>

    The content is actually markdown text.

    ##### Response

    ```json
    {
        "id": "hguyfnvh",
        "createdAt": "2025-03-02T19:06:29.538Z",
        "title": "new pastemyst release",
        "content": "a **new** pastemyst version has just been released!"
    }
    ```
</details>

## Get all announcements

Returns all of the announcements.

<details>
    <summary><code>GET /api/v3/announcements</code></summary>

    The content is actually markdown text.

    ##### Response

    ```json
    [
        {
            "id": "hguyfnvh",
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

    The content is actually markdown text.

    ##### Request

    ```json
    {
        "title": "new pastemyst release",
        "content": "a **new** pastemyst version has just been released!"
    }
    ```
</details>

## Edit an announcement

Edits a specific announcement. You must be logged in as an admin user to create announcements.

<details>
    <summary><code>PATCH /api/v3/announcements/<b>:id</b></code></summary>

    The content is actually markdown text.

    ##### Request

    ```json
    {
        "title": "new pastemyst release",
        "content": "a **new** pastemyst version has just been released!"
    }
    ```
</details>


## Delete an announcement

Deletes a single announcement. You must be logged in as an admin user to create announcements.

<details>
    <summary><code>DELET /api/v3/announcements/<b>:id</b></code></summary>

    The content is actually markdown text.
</details>