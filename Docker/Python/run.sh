#!/bin/bash

echo "Script started" 

mkdir -p /home/runner/App

echo "Building and running code..."

cp /home/runner/submission/UserCode.txt /home/runner/App/solution.py
cp /home/runner/submission/DriverCode.txt /home/runner/App/index.py
cp /home/runner/submission/input.json /home/runner/input.json
cp /home/runner/submission/expectedOutput.json /home/runner/expectedOutput.json

cd /home/runner/App

timeout 20s python index.py /home/runner/input.json /home/runner/expectedOutput.json /home/runner/submission/results.json >> /home/runner/submission/output.txt 2>> /home/runner/submission/error.txt

echo "Done"
