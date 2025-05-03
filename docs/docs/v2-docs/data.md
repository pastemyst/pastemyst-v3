---
sidebar_position: 2
slug: /v2/data
---

# Data

These are the docs for the data endpoints. Used for getting info about languages and the number of active pastes.

## Get a language by name

Get the language information for a specific language. Note that the language name must be [percent encoded](https://en.wikipedia.org/wiki/Percent-encoding).

<details>
    <summary><code>GET /api/v2/data/language?name=<b>:languageName</b></code></summary>

    ##### Response

    ```json
    {
        "name": "C",
        "mode": "clike",
        "mimes": [
            "text/x-csrc"
        ],
        "ext": [
            "c",
            "cats",
            "h",
            "idc"
        ],
        "color": "#555555"
    }
    ```
</details>

## Get a language by extension

Get the language information for a specific language.

<details>
    <summary><code>GET /api/v2/data/language?extension=<b>:languageExtension</b></code></summary>

    ##### Response

    ```json
    {
        "name": "C",
        "mode": "clike",
        "mimes": [
            "text/x-csrc"
        ],
        "ext": [
            "c",
            "cats",
            "h",
            "idc"
        ],
        "color": "#555555"
    }
    ```
</details>

## Get the number of currently active pastes

Returns the number of currently active pastes.

<details>
    <summary><code>GET /api/v2/data/numPastes</code></summary>

    ##### Response

    ```json
    {
        "numPastes": 111
    }
    ```
</details>