#!/bin/bash

set -e

if [ $# -ne 1 ]; then
    echo "$0": usage: quick-release.sh ENVIRONMENT
    echo "$0": eg: quick-release.sh dev
    exit 1
fi

ENVIRONMENT=$1
export VERSION="0.0.0"
export DEPLOYED_DATE="0.0.0"

rm -rf "package.zip"
dotnet lambda package "package.zip" -pl src -c Release
yarn run sls deploy --stage "$ENVIRONMENT" --verbose