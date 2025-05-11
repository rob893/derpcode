#!/bin/bash

echo "Script started" 
ls -al /home/runner/submission 

mkdir -p /home/runner/App

echo "Building and running code..."

timeout 20s dotnet new console -o App --no-restore > /dev/null 2>&1
cp /home/runner/submission/UserCode.txt /home/runner/App/Solution.cs
cp /home/runner/submission/DriverCode.txt /home/runner/App/Program.cs
cp /home/runner/submission/input.txt /home/runner/input.txt

cd /home/runner/App
timeout 20s dotnet build #--nologo > /dev/null 2>> /home/runner/submission/error.txt

if [ $? -eq 0 ]; then
    timeout 20s dotnet run --no-build < /home/runner/input.txt > /home/runner/submission/output.txt 2>> /home/runner/submission/error.txt
else
    echo "Build failed" 
fi

echo "Done"
