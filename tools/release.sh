#!/bin/bash
# Build a release zip: BepInEx plugin layout that extracts straight into the
# game directory on Windows or Linux (game install must already have BepInEx 5).
set -e
cd "$(dirname "$0")/.."

export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"

VERSION=$(grep -oP 'public const string Version = "\K[^"]+' src/MapEditorMod/Plugin.cs)
OUT="dist"
STAGE="$OUT/stage"

dotnet build src/MapEditorMod/MapEditorMod.csproj -c Release

rm -rf "$STAGE"
mkdir -p "$STAGE/BepInEx/plugins/E2EMapEditor"
cp src/E2EApi/bin/Release/E2EApi.dll "$STAGE/BepInEx/plugins/E2EMapEditor/"
cp src/MapEditorMod/bin/Release/MapEditorMod.dll "$STAGE/BepInEx/plugins/E2EMapEditor/"
cp docs/user-install.md "$STAGE/README-INSTALL.md"

# Installer
mkdir -p "$STAGE/installer"
cp installer/install.py "$STAGE/installer/"
cp installer/install.bat "$STAGE/installer/"
cp installer/install.sh "$STAGE/installer/"
chmod +x "$STAGE/installer/install.sh"

ZIP="$OUT/E2EMapEditor-v$VERSION.zip"
rm -f "$ZIP"
(cd "$STAGE" && zip -qr "../$(basename "$ZIP")" .)
rm -rf "$STAGE"

echo "release: $ZIP"
unzip -l "$ZIP"
