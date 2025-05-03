---
sidebar_position: 5
slug: /v2/users
---

# Users

These are the docs for the user endpoints. Used for getting info about users.

## Check if a user exists

Checs if a user with the specified username exists.

<details>
    <summary><code>GET /api/v2/user/<b>:username</b>/exists</code></summary>

    Returns the status `200 OK` if the user exists.
</details>

## Get a user

Returns a single user.

<details>
    <summary><code>GET /api/v2/user/<b>:username</b></code></summary>

    ##### Response
    
    ```json
    {
        "username": "CodeMyst",
        "contributor": true,
        "_id": "bcfu7961",
        "avatarUrl": "https://paste.myst.rs/static/assets/avatars/m6jqcf0e.png",
        "publicProfile": true,
        "defaultLang": "D",
        "supporterLength": 0
    }
    ```
</details>

## Get the current user

Returns the currently logged in user as identified by the token.

<details>
    <summary><code>GET /api/v2/user/self</code></summary>

    ##### Required scope

    `user:read`

    ##### Response
    
    ```json
    {
        "username": "CodeMyst",
        "contributor": true,
        "_id": "bcfu7961",
        "avatarUrl": "https://paste.myst.rs/static/assets/avatars/m6jqcf0e.png",
        "publicProfile": true,
        "defaultLang": "D",
        "supporterLength": 0,
        "stars": ["hbygfhts"],
        "serviceIds": {
            "github": "111111111"
        }
    }
    ```
</details>

## Get the current user's pastes

Returns the currently logged in user's pastes.

<details>
    <summary><code>GET /api/v2/user/self/pastes</code></summary>

    ##### Required scope

    `paste:read`

    ##### Response
    
    ```json
    [
        "xexhemlu",
        "sr7zm4hp",
        "jhiro31p",
        "7lp5gze9",
        "cw09wh9y"
    ]
    ```
</details>