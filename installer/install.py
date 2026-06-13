#!/usr/bin/env python3
"""
E2E Map Editor – Installer / Uninstaller
=========================================
Supports Windows and Linux (including Steam Deck).

Run with:  python install.py          (install)
           python install.py uninstall (uninstall mod + optionally BepInEx)

Think this tool should be extended and improved?
Vote here: https://github.com/anonymusdennis/E2E-Mapeditor-V2.0/issues/5
"""

import os
import sys
import platform
import re
import shutil
import zipfile
import urllib.request
import tempfile

# ---------------------------------------------------------------------------
# Constants
# ---------------------------------------------------------------------------

BEPINEX_WIN_URL  = "https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_x86_5.4.21.0.zip"
BEPINEX_UNIX_URL = "https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_unix_5.4.21.0.zip"

PLUGIN_SUBDIR = os.path.join("BepInEx", "plugins", "E2EMapEditor")

# DLLs live next to the installer in the release layout:
#   installer/install.py
#   BepInEx/plugins/E2EMapEditor/E2EApi.dll
#   BepInEx/plugins/E2EMapEditor/MapEditorMod.dll
_INSTALLER_DIR = os.path.dirname(os.path.abspath(__file__))
_RELEASE_ROOT  = os.path.dirname(_INSTALLER_DIR)
SOURCE_DLL_DIR = os.path.join(_RELEASE_ROOT, PLUGIN_SUBDIR)

# Common Steam install paths to probe
_IS_WIN = platform.system() == "Windows"

_STEAM_WIN_PATHS = [
    r"C:\Program Files (x86)\Steam\steamapps\common\The Escapists 2",
    r"C:\Program Files\Steam\steamapps\common\The Escapists 2",
    r"D:\SteamLibrary\steamapps\common\The Escapists 2",
    r"D:\Steam\steamapps\common\The Escapists 2",
]

_STEAM_LINUX_PATHS = [
    os.path.expanduser("~/.local/share/Steam/steamapps/common/The Escapists 2"),
    os.path.expanduser("~/.steam/steam/steamapps/common/The Escapists 2"),
    os.path.expanduser("~/snap/steam/common/.local/share/Steam/steamapps/common/The Escapists 2"),
    "/run/media/mmcblk0p1/SteamLibrary/steamapps/common/The Escapists 2",  # Steam Deck SD card
]

ISSUE_LINK = (
    "Think this tool should be extended and improved? "
    "Vote here: https://github.com/anonymusdennis/E2E-Mapeditor-V2.0/issues/5"
)

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def _print_header():
    print("=" * 60)
    print("  E2E Map Editor – Installer")
    print("=" * 60)
    print()

def _is_game_folder(path: str) -> bool:
    """Return True if path looks like the TE2 game folder."""
    exe_win   = os.path.join(path, "TheEscapists2.exe")
    exe_linux = os.path.join(path, "TheEscapists2.x86_64")
    return os.path.isfile(exe_win) or os.path.isfile(exe_linux)

def _detect_game_path():  # -> Optional[str]
    """Try to auto-detect the game installation folder."""
    candidates = _STEAM_WIN_PATHS if _IS_WIN else _STEAM_LINUX_PATHS
    for p in candidates:
        if _is_game_folder(p):
            return p
    return None

def _ask_game_path(detected):  # detected: Optional[str] -> str
    """Prompt the user to confirm the detected path or enter a custom one."""
    if detected:
        print(f"Game folder found:\n  {detected}\n")
        answer = input("Is this the right location? [Y/n]: ").strip().lower()
        if answer in ("", "y", "yes"):
            return detected
        print()

    while True:
        custom = input("Enter the full path to The Escapists 2 game folder: ").strip().strip('"').strip("'")
        if _is_game_folder(custom):
            return custom
        # Warn but still let them proceed if they insist
        print(f"  WARNING: Could not find TheEscapists2.exe / TheEscapists2.x86_64 in:\n    {custom}")
        confirm = input("  Use this path anyway? [y/N]: ").strip().lower()
        if confirm in ("y", "yes"):
            return custom
        print()

def _download(url: str, dest: str):
    """Download url to dest with a simple progress indicator."""
    print(f"  Downloading {url.split('/')[-1]} …", end="", flush=True)
    try:
        with urllib.request.urlopen(url) as response, open(dest, "wb") as f:
            total = int(response.headers.get("Content-Length", 0))
            downloaded = 0
            block = 8192
            while True:
                chunk = response.read(block)
                if not chunk:
                    break
                f.write(chunk)
                downloaded += len(chunk)
                if total:
                    pct = downloaded * 100 // total
                    print(f"\r  Downloading {url.split('/')[-1]} … {pct}%  ", end="", flush=True)
        print(" done.")
    except Exception as exc:
        print(f"\n  ERROR: download failed – {exc}")
        raise

def _extract_zip(zip_path: str, dest_dir: str):
    """Extract a zip into dest_dir."""
    with zipfile.ZipFile(zip_path, "r") as zf:
        zf.extractall(dest_dir)

def _patch_run_bepinex(game_path: str):
    """
    Apply the required LD_LIBRARY_PATH fix to run_bepinex.sh on Linux so
    Harmony patches don't crash with a NativeDetour NullReferenceException.
    """
    script = os.path.join(game_path, "run_bepinex.sh")
    if not os.path.isfile(script):
        print("  (run_bepinex.sh not found – skipping patch)")
        return

    with open(script, "r") as f:
        content = f.read()

    # Set correct executable name (only replace if it's blank or missing)
    exe_match = re.search(r'^executable_name=(.*)$', content, re.MULTILINE)
    if exe_match:
        current_val = exe_match.group(1).strip('"').strip("'")
        if current_val != "TheEscapists2.x86_64":
            content = content[:exe_match.start()] + \
                      'executable_name="TheEscapists2.x86_64"' + \
                      content[exe_match.end():]

    # Apply LD_LIBRARY_PATH fix if not already present
    mono_path = '"${BASEDIR}/TheEscapists2_Data/Mono/x86_64"'
    if mono_path not in content:
        old_export = 'export LD_LIBRARY_PATH="${doorstop_libs}":${LD_LIBRARY_PATH}'
        new_export = (
            'export LD_LIBRARY_PATH="${doorstop_libs}":'
            '"${BASEDIR}/TheEscapists2_Data/Mono/x86_64"'
            ':${LD_LIBRARY_PATH}'
        )
        if old_export in content:
            content = content.replace(old_export, new_export)
        else:
            print("  WARNING: Could not locate LD_LIBRARY_PATH export line to patch.")
            print("  Please add the Mono path manually – see docs/user-install.md.")

    with open(script, "w") as f:
        f.write(content)

    # Make executable
    os.chmod(script, 0o755)
    print("  run_bepinex.sh patched and made executable.")

# ---------------------------------------------------------------------------
# Install
# ---------------------------------------------------------------------------

def install():
    _print_header()
    print("This installer will:\n"
          "  1. Verify / locate The Escapists 2 game folder\n"
          "  2. Download and install BepInEx 5.4.21 (the required version)\n"
          "  3. Install E2EApi.dll and MapEditorMod.dll\n")

    # --- locate game ---
    detected = _detect_game_path()
    game_path = _ask_game_path(detected)
    print(f"\nInstalling into: {game_path}\n")

    # --- BepInEx ---
    bepinex_dir = os.path.join(game_path, "BepInEx")
    doorstop_marker = (
        os.path.join(game_path, "winhttp.dll")       # Windows
        if _IS_WIN else
        os.path.join(game_path, "run_bepinex.sh")    # Linux
    )
    if os.path.isdir(bepinex_dir) and os.path.isfile(doorstop_marker):
        print("[1/3] BepInEx already present – skipping download.")
    else:
        print("[1/3] Installing BepInEx 5.4.21 …")
        url = BEPINEX_WIN_URL if _IS_WIN else BEPINEX_UNIX_URL
        with tempfile.TemporaryDirectory() as tmp:
            zip_path = os.path.join(tmp, "bepinex.zip")
            _download(url, zip_path)
            print("  Extracting …", end="", flush=True)
            _extract_zip(zip_path, game_path)
            print(" done.")

        if not _IS_WIN:
            _patch_run_bepinex(game_path)
            print()
            print("  IMPORTANT – Steam launch options required on Linux / Steam Deck:")
            print("    Right-click The Escapists 2 → Properties → Launch Options:")
            print("      ./run_bepinex.sh %command%")
            print()

    # --- Plugin DLLs ---
    print("[2/3] Installing mod files …")
    if not os.path.isdir(SOURCE_DLL_DIR):
        print(f"\n  ERROR: Could not find mod DLLs in:\n    {SOURCE_DLL_DIR}")
        print("  Make sure you extracted the full release zip before running install.py")
        sys.exit(1)

    dest_plugin = os.path.join(game_path, PLUGIN_SUBDIR)
    os.makedirs(dest_plugin, exist_ok=True)

    for dll in ("E2EApi.dll", "MapEditorMod.dll"):
        src = os.path.join(SOURCE_DLL_DIR, dll)
        dst = os.path.join(dest_plugin, dll)
        shutil.copy2(src, dst)
        print(f"  copied {dll}")

    # --- Verify ---
    print("\n[3/3] Verifying …")
    ok = True
    for dll in ("E2EApi.dll", "MapEditorMod.dll"):
        dst = os.path.join(dest_plugin, dll)
        if os.path.isfile(dst):
            print(f"  ✓ {dst}")
        else:
            print(f"  ✗ MISSING: {dst}")
            ok = False

    if ok:
        print("\n✅ Installation complete!")
        print("\nStart The Escapists 2 once so BepInEx generates its folders,")
        print("then open http://127.0.0.1:8723 in your browser when in the level editor.")
    else:
        print("\n❌ Installation may be incomplete – see errors above.")

    print(f"\n{ISSUE_LINK}\n")

# ---------------------------------------------------------------------------
# Uninstall
# ---------------------------------------------------------------------------

def uninstall():
    _print_header()
    print("Uninstall – this will remove the E2E Map Editor mod files.\n")

    detected = _detect_game_path()
    game_path = _ask_game_path(detected)
    print(f"\nUninstalling from: {game_path}\n")

    # Remove plugin folder
    dest_plugin = os.path.join(game_path, PLUGIN_SUBDIR)
    if os.path.isdir(dest_plugin):
        confirm = input(f"Remove {dest_plugin}? [Y/n]: ").strip().lower()
        if confirm in ("", "y", "yes"):
            shutil.rmtree(dest_plugin)
            print("  Mod files removed.")
        else:
            print("  Skipped.")
    else:
        print(f"  Mod folder not found ({dest_plugin}) – nothing to remove.")

    # Optionally remove BepInEx
    bepinex_dir = os.path.join(game_path, "BepInEx")
    if os.path.isdir(bepinex_dir):
        print()
        answer = input("Also remove BepInEx entirely? This may break other mods. [y/N]: ").strip().lower()
        if answer in ("y", "yes"):
            shutil.rmtree(bepinex_dir)
            # Remove doorstop proxy
            for proxy in ("winhttp.dll", "run_bepinex.sh", "doorstop_config.ini", ".doorstop_version"):
                p = os.path.join(game_path, proxy)
                if os.path.isfile(p):
                    os.remove(p)
            print("  BepInEx removed.")
        else:
            print("  BepInEx kept.")

    print("\n✅ Uninstall complete.\n")
    print(f"{ISSUE_LINK}\n")

# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------

def main():
    if len(sys.argv) > 1 and sys.argv[1].lower() in ("uninstall", "--uninstall", "-u"):
        uninstall()
    else:
        install()

if __name__ == "__main__":
    main()
