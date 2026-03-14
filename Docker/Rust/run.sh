#!/bin/bash

echo "Script started" 

cp /home/runner/submission/UserCode.txt /home/runner/src/solution.rs
cp /home/runner/submission/DriverCode.txt /home/runner/src/main.rs
cp /home/runner/submission/input.json /home/runner/input.json
cp /home/runner/submission/expectedOutput.json /home/runner/expectedOutput.json

cd /home/runner

echo "Building..."
BUILD_OUTPUT=$(timeout 15s cargo build --release --quiet 2>&1)
BUILD_EXIT=$?

if [ $BUILD_EXIT -ne 0 ]; then
  echo "$BUILD_OUTPUT" >> /home/runner/submission/error.txt
  echo "Compilation failed" >> /home/runner/submission/error.txt
  exit 1
fi

echo "Running..."
timeout 20s ./target/release/rust_runner /home/runner/input.json /home/runner/expectedOutput.json /home/runner/submission/results.json \
  >> /home/runner/submission/output.txt 2>> /home/runner/submission/error.txt

echo "Done"
