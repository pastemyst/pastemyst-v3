#!/usr/bin/env bash

echo "Running pre-commit hooks"
./client/.git-scripts/run-check.sh
./client/.git-scripts/run-lint.sh

if [ $? -ne 0 ]; then
 echo "You need to fix the reported issues before commiting!"
 exit 1
fi
