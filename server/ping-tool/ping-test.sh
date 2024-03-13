#!/usr/bin/env bash

BASEDIR=$(dirname "$0")
PROJECT_DIR="$(realpath "${BASEDIR}")"
cd $PROJECT_DIR

cargo build --release

for i in {1..10}; do
    echo "Running test $i"
    nohup $PROJECT_DIR/target/release/client $i >"${PROJECT_DIR}/test${i}.log" 2>&1 &
done

sleep 5

echo "test results:"
find $PROJECT_DIR -name "test*.log" -exec cat {} \; | grep "TimeSync: "
