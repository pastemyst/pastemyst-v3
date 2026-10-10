---
sidebar_position: 2
slug: /v2/data
---

# Data

These are the docs for the data endpoints. Used for getting info about languages and the number of active pastes.

V2 language names are accepted, and where V3 renamed a language the V2 name is returned (for example `Plain Text` instead of `Text`). See [the differences from the original V2](./v2-api-docs.md#differences-from-the-original-v2).

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

    ##### Errors

    - `404` with `"The language was not found."` if there's no such language
</details>

## Get a language by extension

Get the language information for a specific language.

<details>
    <summary><code>GET /api/v2/data/languageExt?extension=<b>:languageExtension</b></code></summary>

    The extension is given without the leading dot, for example `?extension=c`.

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

    ##### Errors

    - `404` with `"The language was not found."` if no language has this extension
</details>

## Get the number of currently active pastes

Returns the number of currently active pastes, including encrypted pastes.

<details>
    <summary><code>GET /api/v2/data/numPastes</code></summary>

    ##### Response

    ```json
    {
        "numPastes": 111
    }
    ```
</details>
