#!/usr/bin/env bash

BASEDIR=$(dirname "$0")
DOCKER_DIR="$(realpath "${BASEDIR}")"
ROOT_DIR="$(realpath "${DOCKER_DIR}/..")"
CLIENT_DIR="$(realpath "${ROOT_DIR}/client")"
PROTO_DIR="$(realpath "${CLIENT_DIR}/proto")"

echo "Load environment variables ..."

if [ -f $DOCKER_DIR/.env ]; then
    echo "Loading environment variables from $DOCKER_DIR/.env ..."
    set -a
    source $DOCKER_DIR/.env
    set +a
fi

if [ -z "$UNITY_PATH" ]; then
    echo "UNITY_PATH is not set"
    exit 1
fi

cd $PROTO_DIR || exit
echo "Using proto path: $PROTO_DIR"

# generate proto files
echo "Generating proto files ..."
yarn install
yarn run build

PROJECT_DIR="$(realpath "${CLIENT_DIR}/unity")"

cd $PROJECT_DIR || exit

echo "Using Unity path: $UNITY_PATH"
echo "Using project path: $PROJECT_DIR"

python3 -u $PROJECT_DIR/Tools/build.py -unity $UNITY_PATH \
    -project $PROJECT_DIR \
    -method "Wtf.BuildTool.BuildWebGL"

if [ ! $? -eq 0 ]; then
    echo "Failure: build script failed"
    exit 1
fi

echo 'Done'
