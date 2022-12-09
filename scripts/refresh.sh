#!/usr/bin/env bash

cd "$(dirname "$0")" || exit

./uninstall_tool.sh
./delete_cache.sh
./build.sh
./publish.sh
./install_tool.sh
