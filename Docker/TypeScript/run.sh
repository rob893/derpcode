#!/bin/bash

echo "Script started" 

cp /home/runner/submission/UserCode.txt /home/runner/solution.ts
cp /home/runner/submission/DriverCode.txt /home/runner/index.ts
cp /home/runner/submission/input.json /home/runner/input.json
cp /home/runner/submission/expectedOutput.json /home/runner/expectedOutput.json

echo "Building..."
# Type check (catches TS errors)
BUILD_OUTPUT=$(timeout 15s ./node_modules/.bin/tsc --noEmit 2>&1)
BUILD_EXIT=$?

if [ $BUILD_EXIT -ne 0 ]; then
  echo "$BUILD_OUTPUT" >> /home/runner/submission/error.txt
  echo "Compilation failed" >> /home/runner/submission/error.txt
  exit 1
fi

# Bundle (fast transpile, no type checking)
./node_modules/.bin/esbuild index.ts --bundle --platform=node --outfile=dist/index.js 2>/dev/null

echo "Running..."
timeout 20s node dist/index.js /home/runner/input.json /home/runner/expectedOutput.json /home/runner/submission/results.json >> /home/runner/submission/output.txt 2>> /home/runner/submission/error.txt

echo "Done"
