#!/bin/bash
# Launch The Escapists 2 with BepInEx + E2E Map Editor on Linux.
set -e
GAME="${GAMEPATH:-$HOME/.local/share/Steam/steamapps/common/The Escapists 2}"
cd "$GAME"
export SteamAppId="${SteamAppId:-641990}"
export SteamGameId="${SteamGameId:-641990}"
exec ./run_bepinex.sh "$@"
