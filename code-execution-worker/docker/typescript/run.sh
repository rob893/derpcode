#!/bin/bash

echo "Script started" 

echo "Building and running code..."

cp /home/runner/submission/UserCode.txt /home/runner/solution.ts
cp /home/runner/submission/DriverCode.txt /home/runner/index.ts
cp /home/runner/submission/input.json /home/runner/input.json
cp /home/runner/submission/expectedOutput.json /home/runner/expectedOutput.json

echo "Running TypeScript code..."
timeout 20s ./node_modules/.bin/ts-node index.ts /home/runner/input.json /home/runner/expectedOutput.json /home/runner/submission/results.json >> /home/runner/submission/output.txt 2>> /home/runner/submission/error.txt

echo "Done"
