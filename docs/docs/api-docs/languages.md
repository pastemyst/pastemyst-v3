---
sidebar_position: 6
slug: /languages
---

# Languages

These are the docs for the languages endpoints. Used for getting information about supported languages on PasteMyst, and autodetecting languages based on content.

## List all languages

Lists all of the languages that are supported on PasteMyst.

<details>
    <summary><code>GET /api/v3/langs</code></summary>

    ##### Response

    ```json
    [
        {
            "name": "Autodetect",
            "type": null,
            "aliases": [
                "autodetect"
            ],
            "codemirrorMode": null,
            "codemirrorMimeType": null,
            "wrap": false,
            "extensions": [],
            "color": null,
            "tmScope": null
        },
        {
            "name": "1C Enterprise",
            "type": "programming",
            "aliases": null,
            "codemirrorMode": null,
            "codemirrorMimeType": null,
            "wrap": false,
            "extensions": [
                ".bsl",
                ".os"
            ],
            "color": "#814CCC",
            "tmScope": "source.bsl"
        },
        {
            "name": "2-Dimensional Array",
            "type": "data",
            "aliases": null,
            "codemirrorMode": null,
            "codemirrorMimeType": null,
            "wrap": false,
            "extensions": [
                ".2da"
            ],
            "color": "#38761D",
            "tmScope": "source.2da"
        },
        {
            "name": "4D",
            "type": "programming",
            "aliases": null,
            "codemirrorMode": null,
            "codemirrorMimeType": null,
            "wrap": false,
            "extensions": [
                ".4dm"
            ],
            "color": "#004289",
            "tmScope": "source.4dm"
        },
        {
            "name": "ABAP",
            "type": "programming",
            "aliases": null,
            "codemirrorMode": null,
            "codemirrorMimeType": null,
            "wrap": false,
            "extensions": [
                ".abap"
            ],
            "color": "#E8274B",
            "tmScope": "source.abap"
        },
        {
            "name": "ABAP CDS",
            "type": "programming",
            "aliases": null,
            "codemirrorMode": null,
            "codemirrorMimeType": null,
            "wrap": false,
            "extensions": [
                ".asddls"
            ],
            "color": "#555e25",
            "tmScope": "source.abapcds"
        },
        // ...
    ]
    ```
</details>

## List all popular languages

Lists the names of only the popular (most commonly used) languages.

<details>
    <summary><code>GET /api/v3/langs/popular</code></summary>

    ##### Response

    ```json
    [
        "Autodetect",
        "Text",
        "C",
        "C#",
        "C++",
        "CSS",
        "D",
        "Dart",
        "Go",
        "Haskell",
        "HTML",
        "Java",
        "JavaScript",
        "JSON",
        "Kotlin",
        "Markdown",
        "Objective-C",
        "Perl",
        "PHP",
        "PowerShell",
        "Python",
        "Ruby",
        "Rust",
        "Scala",
        "Shell",
        "Swift",
        "TypeScript",
        "Yaml"
    ]
    ```
</details>

## Get a language by name

Finds a language by name. For the name it will accept any of the following: name, alias, extension (without the leading dot). The matching is case-insensitive. A name match has the highest priority, otherwise the first language with a matching alias or extension is returned (in the order of the list returned by `GET /api/v3/langs`).

<details>
    <summary><code>GET /api/v3/langs/<b>:name</b></code></summary>

    Example request: `GET /api/v3/langs/ts`

    ##### Response

    ```json
    {
        "aliases": [
            "ts"
        ],
        "codemirrorMimeType": "application/typescript",
        "codemirrorMode": "javascript",
        "color": "#3178c6",
        "extensions": [
            ".ts",
            ".cts",
            ".mts"
        ],
        "name": "TypeScript",
        "tmScope": "source.ts",
        "type": "programming",
        "wrap": false
    }
    ```

    If no language matches, it returns `404 Not Found`:

    ```json
    {
        "statusCode": 404,
        "message": "The language was not found."
    }
    ```
</details>

## Autodetect the language

Tries to autodetect the language of the provided content.

<details>
    <summary><code>POST /api/v3/langs/autodetect</code></summary>

    The request body is the content to detect, sent as a JSON string (with `Content-Type: application/json`).

    If the detection fails, the `Text` language is returned.

    ##### Request

    ```json
    "#include <stdio.h>\n\nint main() {\n\treturn 1;\n}"
    ```

    ##### Response

    ```json
    {
        "aliases": null,
        "codemirrorMimeType": "text/x-csrc",
        "codemirrorMode": "clike",
        "color": "#555555",
        "extensions": [
            ".c",
            ".cats",
            ".h",
            ".idc"
        ],
        "name": "C",
        "tmScope": "source.c",
        "type": "programming",
        "wrap": false
    }
    ```
</details>