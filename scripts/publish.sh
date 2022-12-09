#!/usr/bin/env bash

cd "$(dirname "$0")/.." || exit

if ! [[ "$(dotnet nuget list source)" == *"Local [Enabled]"* ]]
then
    mkdir -p ./NugetLocal
    dotnet nuget add source "$(realpath ./NugetLocal)" -n "Local"
fi

find . -name "*.nupkg" | while read pkg
do
    if ! [[ "$pkg" == *"nocompile"* ]] && ! [[ "$pkg" == *"NugetLocal"* ]]
    then
        dotnet nuget push "$pkg" -s "Local"
    fi
done
