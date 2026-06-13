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

The same package works on Windows and Linux (including Steam Deck) — the game
is a native build on both. Full details with troubleshooting live in
[docs/user-install.md](docs/user-install.md); the short version follows.

### 1. Install BepInEx 5

You need **BepInEx 5.4.21, x64**. Newer 5.4.23.x builds crash in the preloader
on this game (MonoMod `NativeDetour` NullReferenceException) — stick to 5.4.21.

Download from the
[BepInEx releases page](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21):

- Windows: `BepInEx_x64_5.4.21.0.zip`
- Linux / Steam Deck: `BepInEx_unix_5.4.21.0.zip`

#### Windows

1. Find the game folder: Steam → right-click The Escapists 2 → Manage →
   Browse local files (usually
   `C:\Program Files (x86)\Steam\steamapps\common\The Escapists 2`).
2. Extract the BepInEx zip **into the game folder**, next to
   `TheEscapists2.exe`. You should now have a `BepInEx` folder and
   `winhttp.dll` there.
3. Start the game once and quit. BepInEx generates its folder structure
   (`BepInEx/plugins`, `BepInEx/config`, `BepInEx/LogOutput.log`).

#### Linux / Steam Deck

1. Find the game folder
   (`~/.local/share/Steam/steamapps/common/The Escapists 2`; on Steam Deck use
   Desktop Mode).
2. Extract the BepInEx **unix** zip into the game folder, next to
   `TheEscapists2.x86_64`. You should now have a `BepInEx` folder and
   `run_bepinex.sh` there.
3. Edit `run_bepinex.sh`:
   - set the executable name near the top:

     ```sh
     executable_name="TheEscapists2.x86_64"
     ```

   - **required fix for this game:** the game's bundled mono directory must be
     on `LD_LIBRARY_PATH`, or every Harmony patch dies with a `NativeDetour`
     NullReferenceException. Find the line that exports `LD_LIBRARY_PATH` and
     make it:

     ```sh
     export LD_LIBRARY_PATH="${doorstop_libs}":"${BASEDIR}/TheEscapists2_Data/Mono/x86_64":${LD_LIBRARY_PATH}
     ```

4. Make the script executable: `chmod +x run_bepinex.sh`
5. In Steam: right-click The Escapists 2 → Properties → Launch Options:

   ```
   ./run_bepinex.sh %command%
   ```

6. Start the game once and quit so BepInEx generates its folders.

### 2. Install the mod

Extract the release zip (`E2EMapEditor-vX.Y.Z.zip` from the
[Releases](../../releases) page) **into the game folder**. It only contains

```
BepInEx/plugins/E2EMapEditor/
├── E2EApi.dll
└── MapEditorMod.dll
```

so it merges into the BepInEx install from step 1.

To verify: start the game and check `BepInEx/LogOutput.log` in the game folder
for a line like

```
[Info   :E2E Map Editor] E2E Map Editor 2.0.0 loading (E2EApi 0.1.0)
```

### 3. First run

- The game starts **windowed at 1600×900** by default so the editor UI fits
  next to it (configurable).
- Open **<http://127.0.0.1:8723>** in your browser — when you first enter the
  level editor the mod opens it automatically (configurable). It only listens
  on localhost.
- Press **F10** for the in-game mod window.

Default hotkeys (all rebindable in the config):

| Key | Where | What |
|-----|-------|------|
| F10 | anywhere | toggle the in-game mod window |
| F6  | level editor | toggle an electric fence on the tile under the cursor |
| F7  | level editor | trigger link: press on the button tile, then on the target tile |
| E   | playing a map | activate the trigger button you are standing on |

### 4. Configuration

BepInEx writes the config on first run to

```
<game folder>/BepInEx/config/org.anonymusdennis.e2e.mapeditor.cfg
```

Plain text; most settings can also be flipped live from the web UI's Settings
tab. Highlights: `[Editor] UnlockDevBlocks`, `[Editor] IgnoreAllRestrictions`,
`[Editor] GuardInmateCap`, `[Display] ForceWindowed` / `WindowedWidth` /
`WindowedHeight`, `[WebUI] Port` / `AutoOpenBrowser`, `[Extras]` hotkeys.

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
