#!/usr/bin/env bash

if [[ "$OSTYPE" == "linux-gnu"* ]]
then
    path="${XDG_CACHE_HOME:-$HOME/.cache}/NuGetPackages"
elif [[ "$OSTYPE" == "msys" ]]
then
    path="$HOME/.nuget/packages"
else
    echo "Unknown OSTYPE $OSTYPE"
    exit 1
fi

if [[ -d $path ]]
then
    echo "Using NuGet cache $path"

    [[ -d "$path/rejuvena.collate" ]] && rm -rfv "$path/rejuvena.collate" || echo "$path/rejuvena.collate does not exist"
    [[ -d "$path/rejuvena.collate.cli" ]] && rm -rfv "$path/rejuvena.collate.cli" || echo "$path/rejuvena.collate.cli does not exist"
    [[ -d "$path/rejuvena.collate.msbuild" ]] && rm -rfv "$path/rejuvena.collate.msbuild" || echo "$path/rejuvena.collate.msbuild does not exist"
else
    echo "Unable to locate NuGet cache at $path"
    exit 1
fi
