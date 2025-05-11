#!/bin/bash

echo "Script started" 
ls -al /home/runner/submission 

echo "Building and running code..."

cp /home/runner/submission/UserCode.txt /home/runner/solution.ts
cp /home/runner/submission/DriverCode.txt /home/runner/index.ts
cp /home/runner/submission/input.txt /home/runner/input.txt

echo "Running TypeScript code..."
timeout 20s ./node_modules/.bin/ts-node index.ts < /home/runner/input.txt > /home/runner/submission/output.txt 2>> /home/runner/submission/error.txt

echo "Done"
