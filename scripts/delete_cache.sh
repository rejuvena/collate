#!/usr/bin/env bash

if [[ "$OSTYPE" == "linux-gnu"* ]]
then
    path="${XDG_CACHE_HOME:-$HOME/.cache}/NuGetPackages"
elif [[ "$OSTYPE" == "msys" ]]
then
    path="$HOME/.nuget/packages"
else
    echo "Unknown OSTYPE $OSTYPE! Contribute additional paths at https://github.com/rejuvena/collate/"
    exit 1
fi

if [[ -d $path ]]
then
    echo "Using NuGet cache $path"

    files=( "rejuvena.collate" "rejuvena.collate.cli" "rejuvena.collate.msbuild" )

    for file in "${files[@]}"
    do
        [[ -d "$path/$file" ]] && rm -rfv "${path:?}/${file:?}" || echo "$path/$file does not exist"
    done
else
    echo "Unable to locate NuGet cache at $path"
    exit 1
fi
