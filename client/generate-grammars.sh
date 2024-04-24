#!/bin/sh

# Script for generating textmate grammars from various sources

git clone --single-branch --branch v7.29.0 https://github.com/github/linguist.git

./linguist/script/bootstrap

./linguist/script/build-grammars-tarball

rm -rf static/grammars

mv linguist/linguist-grammars static/grammars

rm -rf linguist
