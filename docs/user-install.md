# E2E Map Editor — Installation Guide

The E2E Map Editor is a BepInEx 5 mod for **The Escapists 2** (Steam). It adds
a browser-based map editor UI, dev-only blocks, electric fences, trigger links,
playtest helpers and play-mode cheats. The same package works on Windows and
Linux (including Steam Deck) — the game is a native build on both.

The mod ships as two DLLs:

```
BepInEx/plugins/E2EMapEditor/
├── E2EApi.dll          # the modding API (other mods can use it too)
└── MapEditorMod.dll    # the map editor itself
```

## 1. Install BepInEx 5

You need **BepInEx 5.4.21, x64**. Newer 5.4.23.x builds crash in the preloader
on this game (MonoMod `NativeDetour` NullReferenceException) — stick to 5.4.21.

Download from the [BepInEx releases page](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21):

- Windows: `BepInEx_x64_5.4.21.0.zip`
- Linux / Steam Deck: `BepInEx_unix_5.4.21.0.zip`

### Windows

1. Find the game folder: Steam → right-click The Escapists 2 → Manage →
   Browse local files (usually
   `C:\Program Files (x86)\Steam\steamapps\common\The Escapists 2`).
2. Extract the BepInEx zip **into the game folder**, next to
   `TheEscapists2.exe`. You should now have a `BepInEx` folder and
   `winhttp.dll` there.
3. Start the game once and quit. BepInEx generates its folder structure
   (`BepInEx/plugins`, `BepInEx/config`, `BepInEx/LogOutput.log`).

### Linux / Steam Deck

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

## 2. Install the mod

Extract the release zip (`E2EMapEditor-vX.Y.Z.zip`) **into the game folder**.
It only contains the `BepInEx/plugins/E2EMapEditor/` directory, so it merges
into the BepInEx install from step 1. That's it.

To verify: start the game and check `BepInEx/LogOutput.log` in the game folder
for a line like

```
[Info   :E2E Map Editor] E2E Map Editor 2.0.0 loading (E2EApi 0.1.0)
```

## 3. First run

- **Windowed mode:** the game starts windowed at 1600×900 by default (so the
  editor UI in your browser fits next to it). Change or disable this in the
  config (see below).
- **Web UI:** the mod serves its editor UI at **<http://127.0.0.1:8723>**.
  When you first enter the level editor it opens that page in your default
  browser automatically (configurable). You can also just open the URL
  yourself at any time — it only listens on localhost.
- **In-game window:** press **F10** for the in-game mod window
  (settings + mapping tabs). The browser UI is the more capable of the two.

### Default hotkeys

| Key | Where | What |
|-----|-------|------|
| F10 | anywhere | toggle the in-game mod window |
| F6  | level editor | toggle an electric fence on the tile under the cursor |
| F7  | level editor | trigger link: press on the button tile, then on the target tile |
| E   | playing a map | activate the trigger button you are standing on |

All hotkeys are rebindable in the config file.

## 4. Configuration

BepInEx writes the config file on first run:

```
<game folder>/BepInEx/config/org.anonymusdennis.e2e.mapeditor.cfg
```

Plain text, editable with any editor (game restart picks changes up; most
settings can also be flipped live from the web UI's Settings tab). Highlights:

- `[Editor] UnlockDevBlocks` — dev-only blocks in the spawnlist (default on)
- `[Editor] IgnoreAllRestrictions` — master switch: place anything anywhere,
  save/upload/play unfinished maps (default off)
- `[Editor] GuardInmateCap` — guards/inmates available in the editor (default 24)
- `[Display] ForceWindowed` / `WindowedWidth` / `WindowedHeight`
- `[WebUI] Port` (default 8723) and `AutoOpenBrowser`
- `[Extras]` — the hotkeys listed above

## 5. Custom maps and the sidecar file

Mod extras (electric fences, trigger links) are saved in a small text file
`Level.e2e` next to the map's `Level.dat` save. Vanilla never reads it, so:

- your maps stay **100% vanilla-compatible** — players without the mod simply
  get the map without the extras;
- players **with** the mod get the fences/triggers back automatically;
- a map with no extras gets no sidecar file at all.

## Troubleshooting

- **No BepInEx folder contents / no log:** the loader didn't run. Windows:
  check `winhttp.dll` sits next to the exe. Linux: check the launch options
  and that `run_bepinex.sh` is executable.
- **Plugin loads but patches fail (Linux):** `NativeDetour` errors in
  `BepInEx/LogOutput.log` mean the `LD_LIBRARY_PATH` fix from step 1 is
  missing.
- **Web UI doesn't open:** browse to <http://127.0.0.1:8723> manually. If the
  port is taken, change `[WebUI] Port` in the config.
- Logs: `BepInEx/LogOutput.log` (mod + loader), and the Unity player log
  (`~/.config/unity3d/Team 17 Digital ltd_/The Escapists 2/Player.log` on
  Linux).
