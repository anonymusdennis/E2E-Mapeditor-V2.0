#!/bin/bash
# Launch The Escapists 2 with BepInEx + E2E Map Editor.
#
# Native Linux (legacy x86_64 binary + Linux BepInEx):
#   ./tools/launch_modded.sh
#
# Proton / Windows PE32 build (current install):
#   - Install Windows BepInEx (winhttp.dll + doorstop_config.ini) into the game dir
#   - Steam Launch Options must be:
#       WINEDLLOVERRIDES="winhttp=n,b" %command%
#     (helper: ./tools/set_proton_bepinex_launch.sh — quit Steam first)
#   - Prefer launching via Steam so Proton + overlays work:
#       ./tools/launch_modded.sh
#     or: steam steam://rungameid/641990
set -euo pipefail

GAME="${GAMEPATH:-$HOME/.local/share/Steam/steamapps/common/The Escapists 2}"
APP_ID="${SteamAppId:-641990}"
export SteamAppId="$APP_ID"
export SteamGameId="${SteamGameId:-$APP_ID}"

# Doorstop / BepInEx on Wine/Proton: load game-dir winhttp.dll
export WINEDLLOVERRIDES="${WINEDLLOVERRIDES:-winhttp=n,b}"

cd "$GAME"

NATIVE_BIN=""
for cand in TheEscapists2.x86_64 TheEscapists2; do
  if [ -x "./$cand" ] && file "./$cand" 2>/dev/null | grep -q 'ELF.*x86-64'; then
    NATIVE_BIN="./$cand"
    break
  fi
done

if [ -n "$NATIVE_BIN" ] && [ -x ./run_bepinex.sh ]; then
  echo "Native Linux binary found ($NATIVE_BIN); launching via run_bepinex.sh"
  exec ./run_bepinex.sh "$@"
fi

# Proton / Windows build path
if [ -f ./TheEscapists2.exe ]; then
  if [ ! -f ./winhttp.dll ] || [ ! -f ./doorstop_config.ini ]; then
    echo "Proton/Windows build detected but winhttp.dll or doorstop_config.ini is missing." >&2
    echo "Install Windows BepInEx (x86) into: $GAME" >&2
    exit 1
  fi
  echo "Windows PE32 / Proton build detected."
  echo "WINEDLLOVERRIDES=$WINEDLLOVERRIDES"
  echo "Launching via Steam (app $APP_ID) so Proton applies launch options + Wine DLL overrides."
  if command -v steam >/dev/null 2>&1; then
    exec steam -applaunch "$APP_ID" "$@"
  fi
  # Fallback: xdg / steam URL protocol
  if command -v xdg-open >/dev/null 2>&1; then
    exec xdg-open "steam://rungameid/$APP_ID"
  fi
  echo "steam not found. Set Launch Options to:" >&2
  echo '  WINEDLLOVERRIDES="winhttp=n,b" %command%' >&2
  echo "then start the game from Steam, or install steam CLI." >&2
  exit 1
fi

echo "No native Linux binary and no TheEscapists2.exe under: $GAME" >&2
exit 1
