---
description: "Pastry, the command line tool for creating pastes on PasteMyst: download, setup and usage."
sidebar_position: 2
slug: /cli
---

# CLI Tool

Pastry is a command line tool to create pastes on [PasteMyst](https://paste.myst.rs).

You can find the source here: [github.com/pastemyst/pastry](https://github.com/pastemyst/pastry).

Pastry uses the deprecated v2 API, which still works through PasteMyst's `/api/v2` compatibility layer. Because of that it can't create encrypted pastes.

:::info

Pastry will be updated at some point to use the v3 API, which will bring more features (like encrypted pastes) and bug fixes.

:::

## Download

You can download the binaries from the releases page on the GitHub repo: [pastry/releases](https://github.com/pastemyst/pastry/releases). v1.0.0 is the latest release with prebuilt binaries (a `.tar.gz` for Linux and a `.zip` for Windows).

Download from the terminal on Linux:

```sh
wget https://github.com/pastemyst/pastry/releases/download/v1.0.0/pastry-linux-64.tar.gz
```

After downloading, extract the archive and place the `pastry` binary in a directory that's on your `PATH`.

There is also a package for Arch Linux on the AUR: [pastry-aur](https://aur.archlinux.org/packages/pastry/).

To build from source you will need dmd and dub, then run `dub build`.

## Usage

Create a paste from files and/or directories:

```sh
pastry file1.txt file2.txt someDir/
```

Set the title:

```sh
pastry file1.txt -t "paste title"
```

Set the language of all files:

```sh
pastry file1 -l markdown
```

Set when the paste expires:

```sh
pastry file1 -e oneHour
```

Valid values are `never`, `oneHour`, `twoHours`, `tenHours`, `oneDay`, `twoDays`, `oneWeek`, `oneMonth` and `oneYear`.

:::note

Because of a bug in pastry, the `-e/--expires` option currently has no effect. Only the default set with `--set-default-expires` is used.

:::

Set the default expiry time. This value is used when you don't specify the `--expires/-e` option:

```sh
pastry --set-default-expires oneDay
```

Set the language used for files without an extension. The default is `plain text`, and `autodetect` is also accepted:

```sh
pastry --set-no-extension markdown
```

Set the token. Create an access token in PasteMyst under Settings -> access tokens ([paste.myst.rs/settings/access_tokens](https://paste.myst.rs/settings/access_tokens)) with the `paste` scope. Once the token is set you can create private pastes, and all pastes you make will show on your profile. The token is stored in plain text in `~/.config/pastry/config.yml`.

```sh
pastry --set-token <YOUR_TOKEN>

# create private paste
pastry file1.txt -p
```
