#!/bin/bash

echo "Script started" 

mkdir -p /home/runner/App

echo "Building and running code..."

cp /home/runner/submission/UserCode.txt /home/runner/App/Solution.java
cp /home/runner/submission/DriverCode.txt /home/runner/App/Program.java
cp /home/runner/submission/input.json /home/runner/input.json
cp /home/runner/submission/expectedOutput.json /home/runner/expectedOutput.json

cd /home/runner/App

timeout 15s javac -cp "/home/runner/lib/gson-2.10.1.jar" Solution.java Program.java >> /home/runner/submission/output.txt 2>> /home/runner/submission/error.txt

if [ $? -ne 0 ]; then
  echo "Compilation failed" >> /home/runner/submission/error.txt
  exit 1
fi

timeout 20s java -cp "/home/runner/lib/gson-2.10.1.jar:/home/runner/App" Program /home/runner/input.json /home/runner/expectedOutput.json /home/runner/submission/results.json >> /home/runner/submission/output.txt 2>> /home/runner/submission/error.txt

echo "Done"
