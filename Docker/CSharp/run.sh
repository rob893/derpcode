#!/bin/bash

echo "Script started" 

mkdir -p /home/runner/App

cp /home/runner/submission/UserCode.txt /home/runner/App/Solution.cs
cp /home/runner/submission/DriverCode.txt /home/runner/App/Program.cs
cp /home/runner/submission/input.json /home/runner/input.json
cp /home/runner/submission/expectedOutput.json /home/runner/expectedOutput.json

cd /home/runner/App

echo "Building..."
BUILD_OUTPUT=$(timeout 15s dotnet build --no-restore -v:q -nologo 2>&1)
BUILD_EXIT=$?

if [ $BUILD_EXIT -ne 0 ]; then
  echo "$BUILD_OUTPUT" >> /home/runner/submission/error.txt
  echo "Compilation failed" >> /home/runner/submission/error.txt
  exit 1
fi

echo "Running..."
timeout 20s dotnet bin/Debug/net10.0/App.dll /home/runner/input.json /home/runner/expectedOutput.json /home/runner/submission/results.json >> /home/runner/submission/output.txt 2>> /home/runner/submission/error.txt

echo "Done"
