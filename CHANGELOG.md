# Changelog — E2E Map Editor

All notable changes are recorded here.
Format follows [Keep a Changelog](https://keepachangelog.com/).

---

## [Unreleased]

### Added
- **Multiplayer gate** (`MultiplayerGate`): when a custom map is flagged
  `requiresMod=true` and the host is in a Photon room, the host automatically
  sets a room custom property (`e2e_mapeditor`) so other modded clients can
  detect the requirement.  Public API: `MultiplayerGate.IsRequiredInCurrentRoom`,
  `AnnounceRequiresMod()`, `RefreshRoomState()`, events `RoomRequiresMod` and
  `RoomNoLongerRequiresMod`.
- **Web API endpoints**: `GET /api/multiplayer`, `POST /api/multiplayer/announce`,
  `POST /api/multiplayer/refresh`.
- **Web UI multiplayer panel** in the Settings tab: shows room host/client role,
  property state, and "Announce mod required" button.
- **Workshop interop** (`WorkshopInterop`): Workshop uploads now also stage an
  `e2e_workshop_meta.txt` companion file that records mod version and
  `requires_mod` flag.  Download side fires `WorkshopMapLoaded` event so UIs
  can show a compatibility note.
- **CI workflow** (`.github/workflows/ci.yml`): installer smoke-tests on Linux
  and Windows runners on every push/PR, plus a docs-presence check.

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
