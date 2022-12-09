#!/usr/bin/env bash

cd "$(dirname "$0")/.." || { echo "Unable to cd into $(realpath "$(dirname "$0")/..")"; exit; }

find . -name "*.csproj" | while read proj
do
    if ! [[ "$proj" == *"nocompile"* ]] && ! [[ "$proj" == *"ExampleMod"* ]]
    then
        dotnet build "$proj" -c "Release"
    fi
done
