#!/bin/bash
# Build E2EApi + MapEditorMod and deploy into the game's BepInEx/plugins.
set -e
cd "$(dirname "$0")/.."

export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"

GAME="${GAMEPATH:-$HOME/.local/share/Steam/steamapps/common/The Escapists 2}"
PLUGINS="$GAME/BepInEx/plugins/E2EMapEditor"

dotnet build src/MapEditorMod/MapEditorMod.csproj -c Release "$@"

mkdir -p "$PLUGINS"
cp src/E2EApi/bin/Release/E2EApi.dll "$PLUGINS/"
cp src/MapEditorMod/bin/Release/MapEditorMod.dll "$PLUGINS/"

echo "deployed to $PLUGINS:"
ls -la "$PLUGINS"
