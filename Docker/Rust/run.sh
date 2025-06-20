#!/bin/bash

echo "Script started" 

cp /home/runner/submission/UserCode.txt /home/runner/src/solution.rs
cp /home/runner/submission/DriverCode.txt /home/runner/src/main.rs
cp /home/runner/submission/input.json /home/runner/input.json
cp /home/runner/submission/expectedOutput.json /home/runner/expectedOutput.json

cd /home/runner

echo "Building..."
cargo build --release --quiet 2>> /home/runner/submission/error.txt

echo "Running..."
timeout 20s ./target/release/rust_runner /home/runner/input.json /home/runner/expectedOutput.json /home/runner/submission/results.json \
  >> /home/runner/submission/output.txt 2>> /home/runner/submission/error.txt

echo "Done"
