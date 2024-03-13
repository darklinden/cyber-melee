#!/usr/bin/env bash

BASEDIR=$(dirname "$0")
FBS_DIR="$(realpath "${BASEDIR}")"
FLATC_DIR="$(realpath "${FBS_DIR}/flatc")"
FLATC_BIN="${FLATC_DIR}/flatc"
PROJECT_DIR="$(realpath "${FBS_DIR}/../")"

echo "Generate fbs files for server ..."
SERVER_DIR="$(realpath "${PROJECT_DIR}/server/")"
WS_SERVER_DIR="$(realpath "${SERVER_DIR}/ws-server/")"
PROTOCOLS_DIR=$(realpath "${WS_SERVER_DIR}/protocols/src")

echo "Protocols dir: $PROTOCOLS_DIR"
echo "Remove old generated files ..."
rm -rf $PROTOCOLS_DIR
mkdir -p $PROTOCOLS_DIR

echo "Generate Rust files from FlatBuffers ..."

$FLATC_BIN --rust \
    --rust-module-root-file --gen-object-api --gen-name-strings \
    -o $PROTOCOLS_DIR \
    $FBS_DIR/*.fbs

mv $PROTOCOLS_DIR/mod.rs $PROTOCOLS_DIR/lib.rs

echo "Copy fbs files for client protocol project ..."
CLIENT_DIR="$(realpath "${PROJECT_DIR}/client/")"
CLIENT_PROTOCOL_DIR="$(realpath "${CLIENT_DIR}/proto/fbs/")"

cp -r $FBS_DIR/*.fbs $CLIENT_PROTOCOL_DIR

CLIENT_PROTO_DIR="$(realpath "${PROJECT_DIR}/client/proto")"

cd $CLIENT_PROTO_DIR || exit -1

yarn build

echo "Done!"
