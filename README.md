# E2E Mapeditor V2.0

> **🚀 Quick install:** grab the latest zip from [Releases](../../releases), extract it,
> then run the **one-click installer**:
>
> | Platform | How to run |
> |----------|-----------|
> | **Windows** | Double-click `installer\install.bat` |
> | **Linux / Steam Deck** | `cd installer && ./install.sh` |
>
> The installer auto-detects your game folder, downloads the right BepInEx version,
> and drops the mod files in the correct place — no manual steps needed.
> To uninstall: `install.bat uninstall` / `./install.sh uninstall`
>
> *Think the installer should be extended or improved?
> [Vote / comment on the issue](https://github.com/anonymusdennis/E2E-Mapeditor-V2.0/issues/5)*

---

A browser-based map editor extension for **The Escapists 2** (Steam, Windows and
Linux/Steam Deck), built as a BepInEx 5 mod. It hooks into the game's built-in
level editor and adds a full web UI plus a set of editor and gameplay features
the vanilla editor doesn't have — while keeping your maps 100% compatible with
unmodded players.

The mod ships as two DLLs:

- `E2EApi.dll` — a reusable modding API layer for The Escapists 2 (other mods
  can build on it; see [docs/api.md](docs/api.md))
- `MapEditorMod.dll` — the map editor itself, serving its UI at
  <http://127.0.0.1:8723>

## Features

- **Web UI** at `http://127.0.0.1:8723` — block browser with search and
  filters, map overview, settings, all in your normal browser next to the
  game window (the game starts windowed by default to make room).
- **Custom tools** — electric fence painting and trigger linking (button →
  target), with in-game hotkeys as well.
- **Editor unlocks** — dev-only blocks in the spawnlist, raised guard/inmate
  caps, optional "ignore all restrictions" master switch (place anything
  anywhere, save/upload/play unfinished maps).
- **DLC content** — item and recipe unlocks for installed DLC, and custom
  tilesets including DLC art. *DLC content only unlocks if you own and have
  the DLC installed.*
- **Camera lock** and other quality-of-life editor helpers.
- **Playtesting** — teleport and play-mode cheats via the Gameplay tab while
  testing your map.
- **Vanilla-compatible persistence** — mod extras (fences, trigger links) are
  stored in a small `Level.e2e` sidecar file next to the map's `Level.dat`.
  Vanilla never reads it, so your maps stay fully playable for unmodded
  players; modded players get the extras back automatically. An optional
  vanilla fallback "disclaimer map" can be embedded for players without the
  mod.
- **Workshop support** — upload and subscribe like any other custom map.
- **In-game window** (F10) as a lighter alternative to the browser UI.

## Installation

Open your game folder (Steam → The Escapists 2 → Manage → Browse local files)
and look at the executable:

| You see… | Use… |
|----------|------|
| `TheEscapists2.exe` | **BepInEx for Windows** (this is the usual Steam / Proton case) |
| `TheEscapists2.x86_64` | BepInEx for Linux — see [docs/user-install.md](docs/user-install.md) |

If the file is a **Windows** `.exe`, you must use **Windows BepInEx**.
Do **not** install the Linux (`*nix`) BepInEx package next to it.
More info: [BepInEx + Proton/Wine](https://docs.bepinex.dev/articles/advanced/steam_interop.html#protonwine)

### Quick install (Windows exe / Steam Proton)

1. **Download BepInEx 5.4.21 for Windows (32-bit)**  
   [`BepInEx_x86_5.4.21.0.zip`](https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_x86_5.4.21.0.zip)  
   (Use **x86**, not x64. Do not use a newer 5.4.23 build.)

2. **Extract that zip into the game folder** (same folder as `TheEscapists2.exe`).  
   You should now see:
   - `winhttp.dll`
   - `doorstop_config.ini`
   - `BepInEx/`

3. **Install this mod** — put the two DLLs here:
   ```
   BepInEx/plugins/E2EMapEditor/E2EApi.dll
   BepInEx/plugins/E2EMapEditor/MapEditorMod.dll
   ```
   (Release zip from [Releases](../../releases), or `./tools/build.sh` when developing.)

4. **Steam → Properties → Launch Options** (required for Proton Experimental so
   Windows BepInEx / UnityDoorstop can load — not a custom launch script).
   Paste this EXACTLY:

   WINEDLLOVERRIDES="winhttp=n,b" %command%

   On real Windows you can leave Launch Options empty.
   See also `tools/STEAM_LAUNCH_OPTIONS.txt` for backup forms.


5. **Launch the game from Steam** as usual. No extra scripts.

6. **Check it worked** — open `BepInEx/LogOutput.log` in the game folder.
   You want a line like:
   ```
   [Info   :E2E Map Editor] E2E Map Editor … loading
   ```

### After it loads

- Web UI: <http://127.0.0.1:8723>
- In-game mod window: **F10**

| Key | Where | What |
|-----|-------|------|
| F10 | anywhere | toggle the in-game mod window |
| F6  | level editor | toggle an electric fence under the cursor |
| F7  | level editor | trigger link (button tile → target tile) |
| E   | playing | activate the trigger you are standing on |

Config file (created on first run):

```
BepInEx/config/org.anonymusdennis.e2e.mapeditor.cfg
```

More detail / native Linux / troubleshooting: [docs/user-install.md](docs/user-install.md).

## Building from source

Requirements:

- .NET SDK 8 (the projects target .NET Framework 3.5 / Unity Mono, but build
  with the modern SDK toolchain)
- The Escapists 2 installed via Steam, with BepInEx 5.4.21 set up as above

```sh
bash tools/build.sh
```

The build references the game's assemblies (a publicized
`Assembly-CSharp.dll` is generated from your local game install — game
assemblies are **not** included in this repository for copyright reasons,
which is why the game must be installed). The script compiles both projects
and deploys the DLLs straight into
`BepInEx/plugins/E2EMapEditor/` of your local game install.
`tools/release.sh` packages a distributable zip into `dist/`.

See [docs/dev-setup.md](docs/dev-setup.md) for the full development setup and
[docs/api.md](docs/api.md) for the E2EApi surface and HTTP endpoints.

## Documentation

- [docs/user-install.md](docs/user-install.md) — full install guide with troubleshooting
- [docs/api.md](docs/api.md) — E2EApi + HTTP endpoint reference
- [docs/dev-setup.md](docs/dev-setup.md) — development environment
- [docs/leveldat-format.md](docs/leveldat-format.md) — `Level.dat` format notes
- [docs/game-architecture.md](docs/game-architecture.md) — game internals notes

## Disclaimer

This project is not affiliated with Team17 or Mouldy Toof Studios. It contains
no game code or assets; building from source requires your own legitimate copy
of The Escapists 2.
