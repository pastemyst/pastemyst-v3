---
sidebar_position: 2
slug: /cli
---

# CLI Tool

Pastry is a command line tool to create pastes on [PasteMyst](https://paste.myst.rs)

You can find the source here: [github.com/pastemyst/pastry](https://github.com/pastemyst/pastry)

## Download

You can download the binaries from the release page on the github repo: [pastry/releases](https://github.com/pastemyst/pastry/releases)

Download from the terminal on linux:

```sh
wget https://github.com/CodeMyst/pastry/releases/download/v1.0.0/pastry-linux-64.tar.gz
```

After downloading just extract the archive file, and place it in some directory thats in your path

There is also a package for arch on the aur: [pastry-aur](https://aur.archlinux.org/packages/pastry/)

To build from source you will need dmd and dub, then just run `dub build`

## Usage

Create a paste from files and/or directories

```sh
pastry file1.txt file2.txt someDir/
```

Set title

```sh
pastry file1.txt -t "paste title"
```

Set language of all files

```sh
pastry file1 -l markdown
```

Set expires in

```sh
pastry file1 -e oneHour
```

Setting the default expires in time, this value will be used when you dont specify the `--expires|-e` option

```sh
pastry --set-default-expires oneDay
```

Setting the language to be used for files without an extension, default is plaintext

```sh
pastry --set-no-extension markdown
```

Set the token, you can get your token on your PasteMyst profile settings page. once you set the token you can create private pastes, and all pastes you make will show on your profile

```sh
pastry --set-token <YOUR_TOKEN>

# create private paste
pastry file1.txt -p
```
