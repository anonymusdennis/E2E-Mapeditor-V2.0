# Changelog — E2E Map Editor

All notable changes are recorded here.
Format follows [Keep a Changelog](https://keepachangelog.com/).

---

## [Unreleased]

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
