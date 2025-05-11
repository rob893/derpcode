#!/bin/bash

echo "Script started" 
ls -al /home/runner/submission 

mkdir -p /home/runner/App

echo "Building and running code..."

cp /home/runner/submission/UserCode.txt /home/runner/App/solution.js
cp /home/runner/submission/DriverCode.txt /home/runner/App/index.js
cp /home/runner/submission/input.txt /home/runner/input.txt

cd /home/runner/App

timeout 20s node index.js < /home/runner/input.txt > /home/runner/submission/output.txt 2>> /home/runner/submission/error.txt

echo "Done"
