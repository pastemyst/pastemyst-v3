#!/usr/bin/env bash

GIT_DIR=$(git rev-parse --git-dir)

echo "Installing git hooks..."
ln -s ../../client/.git-scripts/pre-commit.sh $GIT_DIR/hooks/pre-commit
echo "Done!"
