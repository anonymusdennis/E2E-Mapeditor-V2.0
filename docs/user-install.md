# E2E Map Editor — Installation

## Which BepInEx?

Look in the game folder for the executable:

| File | Install |
|------|---------|
| `TheEscapists2.exe` | **Windows BepInEx x86** — even if you play on Linux via Steam Proton |
| `TheEscapists2.x86_64` | Linux / unix BepInEx |

Steam on Linux often ships the **Windows** `.exe`. In that case you **must**
use Windows BepInEx. Do not install the `*nix` package.
See: https://docs.bepinex.dev/articles/advanced/steam_interop.html#protonwine

Use **BepInEx 5.4.21** only. Newer 5.4.23.x builds crash on this game.

---

## Windows exe (Steam / Proton) — recommended path

1. Download  
   https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_x86_5.4.21.0.zip  
   (**x86**, not x64.)

2. Extract into the game folder next to `TheEscapists2.exe`.  
   You need `winhttp.dll`, `doorstop_config.ini`, and `BepInEx/`.

3. If an old Linux BepInEx install is present, remove it:
   - delete `run_bepinex.sh`
   - delete the `doorstop_libs/` folder

4. Install the mod DLLs:
   ```
   BepInEx/plugins/E2EMapEditor/E2EApi.dll
   BepInEx/plugins/E2EMapEditor/MapEditorMod.dll
   ```

5. **Steam → The Escapists 2 → Properties → Launch Options**
   (Linux / Proton only — tells Proton to load BepInEx’s `winhttp.dll`):
   ```
   WINEDLLOVERRIDES="winhttp=n,b" %command%
   ```
   On native Windows, leave Launch Options empty.

6. Start the game from Steam. No scripts.

7. Confirm in `BepInEx/LogOutput.log`:
   ```
   [Info   :E2E Map Editor] E2E Map Editor … loading
   ```

---

## Native Linux executable (`TheEscapists2.x86_64`)

Only if you actually have the Linux binary (not the `.exe`):

1. Download `BepInEx_unix_5.4.21.0.zip` from the same BepInEx 5.4.21 release.
2. Extract next to `TheEscapists2.x86_64`.
3. In `run_bepinex.sh` set `executable_name="TheEscapists2.x86_64"` and ensure
   `LD_LIBRARY_PATH` includes the game’s Mono folder:
   ```
   export LD_LIBRARY_PATH="${doorstop_libs}":"${BASEDIR}/TheEscapists2_Data/Mono/x86_64":${LD_LIBRARY_PATH}
   ```
4. Steam Launch Options: `./run_bepinex.sh %command%`
5. Install the mod DLLs under `BepInEx/plugins/E2EMapEditor/` as above.

---

## First run

- Web UI: http://127.0.0.1:8723
- In-game window: **F10**
- Config: `BepInEx/config/org.anonymusdennis.e2e.mapeditor.cfg`

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| No `BepInEx/LogOutput.log` / mod never loads | Wrong BepInEx flavour (nix next to `.exe`), or missing Proton launch option |
| Game is `.exe` but you installed unix BepInEx | Remove `run_bepinex.sh` + `doorstop_libs/`, install Windows x86 zip |
| Log stops in preloader / NativeDetour crash | Use 5.4.21, not 5.4.23+ |
| Plugins missing | DLLs must be in `BepInEx/plugins/E2EMapEditor/` |
