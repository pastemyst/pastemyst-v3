<p align="center" style="position: relative">
    <a href="https://paste.myst.rs">
        <img width="500" src="./.github/assets/pastemyst-banner.png" />
    </a>
    <br/>
    a powerful website for storing and sharing text and code snippets. completely free and open source.
</p>

<p align="center">
    <a href="./LICENSE">license</a> •
    <a href="https://paste.myst.rs/legal">legal</a> •
    <a href="https://paste.myst.rs/donate">donations</a> •
    <a href="https://paste.myst.rs/changelog">changelog</a> •
    <a href="https://paste.myst.rs/api-docs">api</a> •
    <a href="https://paste.myst.rs/pastry">pastry-cli</a>
</p>

<p align="center">
    <a href="https://discord.gg/SdKbcbq">
        <img src="https://discordapp.com/api/guilds/298510542535000065/widget.png"/>
    </a>
    <a href="https://paste.myst.rs/donate">
        <img src="https://img.shields.io/badge/-donate-blueviolet" width="49"/>
    </a>
    <a href="https://github.com/pastemyst/pastemyst-v3/actions">
        <img src="https://github.com/pastemyst/pastemyst-v3/workflows/Svelte/badge.svg"/>
    </a>
</p>

---

this is a complete rewrite, with more functionality and a better ui.

if you want to see the source code of v2 go to [codemyst/pastemyst](https://github.com/CodeMyst/pastemyst).

## building

this is a guide on how to set the project up, build it and get it running.

### api

the api is written in c# with asp.net core, so you need the dotnet sdk (and the asp.net targeting pack). check the [dotnet download page](https://dotnet.microsoft.com/en-us/download) for a guide on how to install it.

you also need a mongodb database.

set the db connection string as a dotnet secret:

```
dotnet user-secrets set ConnectionStrings:DefaultDb "mongodb://127.0.0.1:27017"
```

now you can build and run the api with:

```
dotnet run
```

to test that everything is fine, you can open https://localhost:5000/swagger or `get /ping`.

#### authentification

to enable auth (login), you need to set secrets for github and gitlab oauth applications, you also need to add the secret used for jwt keys.

```
dotnet user-secrets set GitHub:ClientId "..."
dotnet user-secrets set GitHub:ClientSecret "..."
dotnet user-secrets set GitLab:ClientId "..."
dotnet user-secrets set GitLab:ClientSecret "..."

dotnet user-secrets set JwtSecret "..."
```

### client

the client is written with sveltekit, so install node and npm (or yarn).

you will also need ruby, bundler and docker installed to be able to generate the grammars for all the languages, which is done by running `generate-grammars.sh`.

install all dependencies:

```
npm i
```

copy the `.env.example` file to `.env` in the `/client` directory and change the variables.

run the client with:

```
npm run dev
```

## docker

you can also run the entire project (db, api and client) using docker.

copy the `.env.example` file to `.env` and change the variables.

if you're running this locally only (no outside internet access), docker will use the host.docker.internal domain to make requests to the api from the browser, so you need to add an entry to /etc/hosts (linux and mac only) so it properly redirects to localhost:

```
127.0.0.1 host.docker.internal
```

otherwise the api url should be set to the public domain you're using to host the project.

finally run `docker-compose up`.
