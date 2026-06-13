# Custom Tilesets — harvesting game art and painting it anywhere

This feature turns art the player already owns (base game + installed DLC
prison scenes) into freely paintable map content, carried in the `Level.e2e`
sidecar so maps stay vanilla-compatible.

## Architecture

```
TileSets (E2EApi.Editor)      harvest + disk cache + atlas/sprite access
ModTiles (E2EApi.Features)    placement model, [tiles] sidecar section
ModTileOverlay (Features)     world-space SpriteRenderers (editor + play)
EditorTools (MapEditorMod)    PaintTile / EraseTile mouse tools, TileStamp
WebUiServer/WebUiPage         Tilesets tab: harvest, browse, pick, paint
VanillaFallback (Persistence) disclaimer Level_Finished.dat + Workshop sidecar
LoadingOverlay (MapEditorMod) blocking progress bar during mod asset work
```

### Harvesting (`TileSets`)

The editor's placeable blocks live in `standardlevelblocks` (446 blocks); DLC
prisons are hand-built **scenes** in bundles with no editor blocks. So instead
of registering DLC blocks, the harvester loads each prison scene additively
from the main menu (`AssetManager` bundle + `SceneManager.LoadSceneAsync`),
collects every `Texture2D`/`Sprite` the scene brought in, blits them through a
`RenderTexture` (most are GPU-only) and writes PNG atlases to
`BepInEx/plugins/E2EMapEditor/tilecache/`:

- `atlases/<AtlasName>.png` + `<AtlasName>.txt` (size metadata)
- `sets/<set_id>.txt` (atlas list) + `sets/<set_id>.inventory.json`

Harvesting only works from the main menu (loading a prison scene mid-game
would corrupt the session) and shows the blocking loading overlay while it
runs. One set takes a few seconds; all 17 a couple of minutes. The cache is
reused forever after (`cacheVersion` invalidates UI caches when it grows).

### Content sets shipped (this machine, all 6 DLCs installed)

| set | atlases | | set | atlases |
|---|---|---|---|---|
| centre_perks | 257 | | dlc06_prison (Snow Way Out) | 242 |
| halloween_prison | 238 | | dlc04_prison (Wicked Ward) | 233 |
| oil_rig | 227 | | pow_camp | 226 |
| oldwestfort | 225 | | dlc05_prison (Dungeons & Duct Tape) | 224 |
| area_17 | 222 | | dictator_prison | 219 |
| gulag_prison | 215 | | dlc03_prison (Big Top Breakout) | 207 |
| space_prison | 195 | | transport_boat | 181 |
| transport_plane | 149 | | tutorial_prison | 151 |
| transport_train | 125 | | **total (unique PNGs)** | **1033** |

### Placement model (`ModTiles`)

A placement is `(x, y, layer, decor, atlasName, pixelRect)` — note **string
atlas name + pixel rect**, never a numeric block id, so placements are stable
across machines and can never collide with vanilla block ids. Multi-tile
stamps are one placement (rect spans several 32-px tiles; the game's tile size
is 32 px at 120×120 tiles per floor).

- floor stamps render under characters (sorting order 40+layer),
  decor stamps above them (22000+layer)
- in the editor only layers ≤ the current build layer show; lower layers dim
- placements whose atlas isn't cached locally render as a magenta checker
  placeholder and surface in the web UI as "missing atlases — harvest set X"
- atlases referenced by a freshly loaded map are pre-read one per frame with
  the loading overlay up (`ModTiles.Preloading`)

### Persistence

`[tiles]` section in `Level.e2e`, one line per placement:
`x,y,layer,decor,atlasName,rx,ry,rw,rh`. The sidecar loads:

- on editor entry (hook on `GameEvents.EditorEntered` reading
  `GlobalStart.m_strCustomLevelFile` — the editor never calls
  `SaveManager.LoadTheLevel`, which turned out to be dead code)
- on play-mode path resolution (`SaveManager.GetCustomLevelFilePath` postfix,
  which also covers Workshop download folders)

and saves in the `SaveManager.SaveUserLevel` postfix, next to `Level.dat`.

## Workshop uploads

`EditorPublishMenu` stages specific files into a temp folder and
`SteamPlatform.UploadUGCItem` uploads that folder. A prefix patch on
`UploadUGCItem` copies `Level.e2e` into the staging folder when the map's save
folder has one (UGC type `eCustomLevel` only). On the download side no patch
is needed: the `GetCustomLevelFilePath` postfix reads the sidecar from
whatever folder the game resolves, including Workshop item folders.

**Verified locally**: staging-folder patch logic, sidecar pickup from
arbitrary folders. **Not verified**: an actual Steam Workshop publish
round-trip (needs interactive Steam UI/another account to subscribe).

## Vanilla fallback ("NEEDS E2E MAPEDITOR MOD")

Config `Tilesets.VanillaFallbackMap` (default **on**), web UI toggle in
Tools → Map flags.

On every finished save (`SaveUserLevel(bIsFinishedVersion: true)`) of a map
that has mod content:

1. the real `Level_Finished.dat` is base64-stashed into the sidecar's
   `[realmap]` section,
2. the on-disk `Level_Finished.dat` is rewritten as the **disclaimer
   variant**: same gzip container/header, but the `LevelInstructions_V1` chunk
   is decrypted, `Draw_Once` floor-tile instructions spelling
   `NEEDS E2E MAPEDITOR MOD` (3×5 px font, plain ground tiles only — no
   water/lava, those spawn region renderers) are appended on GroundFloor,
   checksums recomputed, chunk re-encrypted.

Vanilla players therefore download a valid, playable map with the message
painted on the ground. Modded clients hit the `GetCustomLevelFilePath`
postfix: if the sidecar has a `[realmap]`, the original bytes are written to a
`Level_Real.e2etmp` scratch file and that path is returned to the loader — the
real map plays. With the toggle off, saves are untouched and vanilla players
just see the map without modded tiles.

## Editor/playtest lifecycle

`GameEvents` gained `PlaytestStarted`/`PlaytestEnded`/`IsPlaytesting`/
`IsEditorActive`. Important quirk: `EditorLevelEditorManager.OnDestroy`
**does not call** `base.OnDestroy()`, so the old patch on
`BaseLevelManager.OnDestroy` never fired for the editor — `EditorExited` was
dead. It now has a dedicated patch. On editor exit (incl. playtest start,
which tears the editor scene down) the mod hides the quick panel, mod window,
tool state, mouse suppression and brush; everything returns on
`EditorEntered`.

## Loading overlay

`LoadingOverlay` (MapEditorMod) is a full-screen dimmed uGUI canvas with a
progress bar + status text on the shared API overlay canvas. It appears while
`TileSets.Busy` (harvest, "harvesting dlc04_prison (1/17)") or
`ModTiles.Preloading` (atlas pre-read after map load) and eats all clicks via
its raycast-blocking backdrop; the mod also stops processing its own hotkeys
and tools while it is up. It never blocks the game's own loading flows — only
the mod's asset work keeps it visible, and it disappears the frame that work
finishes.

## Web UI

- **Tilesets tab**: harvest buttons per set + all, atlas browser with
  thumbnails, region picker with 32-px grid/zoom/drag-select, floor vs decor
  mode, paint/erase tools, "clear all modded tiles".
- **Blocks tab**: 14 metadata-derived filter chips (floors, walls, doors,
  furniture, security, lights, utilities, nature, vents, job objects, rooms,
  decoration, zones, dev-only) that OR-combine and AND with the search box;
  user-saved custom filter chips (★ Save filter, × to delete); drag-and-drop
  tile rearranging; selected block highlighted and synced both ways with the
  in-game brush.
- Prefs (custom filters + arrangement) persist server-side in
  `BepInEx/config/e2e_webui_prefs.json` via `GET/POST /api/prefs`.

## Known limitations / exclusions

- **Prefabs/interactive objects are not harvested** — only flat art
  (textures/sprites). Functional DLC objects (e.g. working Big Top doors)
  would need component graphs to survive scene unload; out of scope.
- Modded tiles are **visual only**: no collision, no zone/AI effects.
- Workshop publish round-trip untested end-to-end (interactive Steam UI).
- Harvest requires each DLC to be installed; un-owned DLC stays unharvestable
  (and maps using it show placeholders + a "harvest set X" warning).
