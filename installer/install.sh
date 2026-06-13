#!/bin/bash
# E2E Map Editor Installer – Linux / Steam Deck launcher
# Run:  ./install.sh            to install
#       ./install.sh uninstall  to uninstall

set -e
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

# Ensure Python 3 is available
if ! command -v python3 &>/dev/null; then
    echo "Python 3 is required but was not found."
    echo "Install it with your package manager, e.g.:"
    echo "  sudo apt install python3    # Debian / Ubuntu"
    echo "  sudo pacman -S python       # Arch / Steam Deck (Desktop Mode)"
    exit 1
fi

python3 "$SCRIPT_DIR/install.py" "$@"
