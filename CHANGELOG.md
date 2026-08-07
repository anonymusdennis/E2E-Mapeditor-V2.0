# Changelog — E2E Map Editor

All notable changes are recorded here.
Format follows [Keep a Changelog](https://keepachangelog.com/).

---

## [Unreleased]

### Added
- **Full multi-floor support**: any combination and number of floors now works
  in both the editor and play mode. The selected floor is shown in isolation
  by default; new *Show floor below* / *Show floor above* checkboxes (quick
  panel + config) blend in the neighbouring floors, or disable *Only show
  selected floor* for the vanilla everything-below view.
- **Floor picker scrollbar**: the custom floor selection list gains a
  scrollbar and adapts its height when a map has many floors; the Configure
  Map Layers window's layer list scrolls as well.
- **Teleport triggers**: a new *Teleport link / stairs* editor tool links a
  source tile (e.g. a staircase) to a destination tile on any virtual floor.
  Players stepping on the source in play mode are teleported — including
  across extra map layers. Trigger data persists in `Level.e2e`.
- **Undo/redo everywhere**: Ctrl+Z undoes and Ctrl+Y (or Ctrl+Shift+Z) redoes
  both vanilla edits and every mod tool (custom tiles, animated tiles,
  connected textures, electric fences, trigger links). Drag gestures count as
  a single step. Quick panel gains Undo/Redo buttons.
- **Alt tool wheel**: hold Alt in the editor for a radial menu navigated with
  the mouse cursor or mouse wheel; release Alt to apply. Includes undo/redo,
  CTM brush collision/damage/auto-connect/color toggles, convert-to-normal
  brush, clear brush, floor isolation, X-ray and overlay toggles.
- **CTM brush collision**: connected texture brushes marked *Collision* now
  block players in play mode (positions are restored when standing on a
  blocked tile), persisted per map.
- **Connected texture persistence**: painted CTM cells and their stamp
  definitions are stored in `Level.e2e`, so connectivity, colors and locks
  survive save/reload.

### Fixed
- **More than six floors broke the editor and game**: floor visibility,
  layer syncing and teleport routing are now fully virtual-layer aware.
- **CTM variant weights**: weights can now be set to 0% (OFF) to disable a
  variant; weight rolls skip disabled variants instead of misbehaving. The
  brush window shows eight weight rows (up from four) plus an *off* button
  and an *Erase ALL painted tiles of this brush* action (single undo step).
- **CTM manual mode**: switching a brush from auto to manual now normalizes
  an unset subtile mask to "fully connected" instead of painting nothing.

---

## [2.5.0]

### Fixed
- **Custom-tile night lighting** (Issue #16, "modded blocks are 100× brighter
  at night"): custom tile and animated tile stamps now tint with the game's
  current ambient light in play mode — indoor/outdoor from the tile's
  environment, underground and vent layers use their own ambient — so they
  darken at night like the vanilla map. Editor rendering is unchanged.
- **Unchecked placements dropped at play/reload** (Issues #18 and #3, "stairs
  just broke" / tiles move or disappear): maps built with *Ignore all
  restrictions* now save an `uncheckedPlacements` flag in `Level.e2e`.
  Flagged maps replay their instruction log with the vanilla support checks
  bypassed in the editor, and play mode re-places tiles/walls the vanilla
  loader silently dropped (only where the editor itself would allow them:
  rooms, occupancy, caps and no-blocking rules still apply).
- **Vent walls undeletable outside vent floors** (Issue #21): the eraser now
  deletes automatic (vent) walls on non-vent layers instead of silently
  skipping them. Vanilla maps are unaffected (vanilla never places automatic
  walls there).
- **Ignore-all toggle leaked native check bypass**: turning the toggle off now
  also resets the game's internal `m_IgnoreChecks` flag (it previously stayed
  on until the game restarted).
- **Update prompt never appeared** (Discord reports): the update checker now
  uses Unity's native `WWW` transport. The game's .NET 3.5 Mono runtime has no
  TLS 1.2, so the old `HttpWebRequest` could never reach GitHub's API and the
  check always failed silently. The threaded request remains as a fallback.
- **`Placement.Delete` API did nothing**: web-UI/API deletes only recorded the
  instruction without running it; they now execute immediately and persist.

---

## [2.1.1] — 2026-06-14

### Added
- Map layers disclaimer in the in-game Configure Map Layers window and web UI
  layer stack overlay.
- **Multiplayer gate** (`MultiplayerGate`): Photon room property support when
  custom maps require the mod; web API and Settings tab panel.
- **Workshop interop** (`WorkshopInterop`): `e2e_workshop_meta.txt` companion
  on upload; `WorkshopMapLoaded` event on download.
- Phase 7.6: Virtual map layers (multi-floor geometry), web layer stack UI,
  expanded bounds, trash bin, drag-and-drop reorder.
- Phase 7.7: Per-map gameplay settings (`[mapSettings]`) with web UI panel.
- Phase 7.5+: Custom tilesets, custom asset placements, animated tiles.
- CI workflow for installer smoke-tests on Linux and Windows.

### Fixed
- **.NET 3.5 / BepInEx build compatibility**: net35-safe collections, string
  joins, `UnityEngine.Object` disambiguation, Harmony patch error handling,
  brush restriction patch target (`ValidateElement`), `EditorSession.Exit()`,
  web UI verbatim string quoting, launch script line endings.
- **CT / mod-tile playtest** (Issue #16): overlay rebuild after playtest,
  lit sprite material on level load, Y-axis tile coordinate inversion.
- **DuplicateLayer**: independent backing layers; `[!SHARED]` warning in layer list.

---

## [2.4.0]

### Added
- Phase 7.7: Per-map gameplay settings (`[mapSettings]` in Level.e2e) with
  full web-UI panel (Time, Audio, Player stats, Security hardware).
  Web API: `GET /api/map-settings`, `POST /api/map-settings/set/unset/clear`.

---

## [2.3.0]

### Added
- Phase 7.6: Virtual map layers (multi-floor geometry).  `MapGeometry` core,
  `FloorTypeRegistry`, `VirtualFloorState`, navigation patches, Z-lookup patches,
  character floor-tracking patches.
  Web API: `/api/layers`, `/api/layers/select`, `/api/map/v/{vi}.png`,
  `/api/debug/floor-registry`, `/api/debug/virtual-floors`.
  Web UI: virtual layer buttons in the Gameplay tab, `vLayer` in player stats.

---

## [2.2.0]

### Added
- Phase 7.5: Custom tilesets (`TileSets` harvester, `ModTiles`, `ModTileOverlay`,
  `AnimatedModTiles`), Workshop carry (`SteamPlatform.UploadUGCItem` prefix),
  vanilla-fallback disclaimer map, sidecar `[tiles]` section, loading overlay.
  Web UI Tilesets tab: harvest, atlas browser, paint/erase tools.
  Web UI Blocks tab: 14 filter chips, drag-and-drop, two-way highlight.

---

## [2.1.0]

### Added
- Phase 7: Dev-only block unlock, DLC/version-gated block unlock, electric
  fences (tile marking + runtime damage), trigger/button link system, custom
  asset placement via Unity 5.5 AssetBundles, Level.e2e sidecar persistence.

---

## [2.0.0]

### Added
- Phase 2–6: BepInEx 5 mod skeleton, V0 feature parity, E2EApi foundation
  (`E2EApi.Players`, `E2EApi.Items`, `E2EApi.Editor`, `E2EApi.Events`,
  `E2EApi.UI`), windowed mode, custom tabbed mapping UI, full placement parity,
  selection/inspection (hover tooltips, X-ray mode), web UI companion window.
