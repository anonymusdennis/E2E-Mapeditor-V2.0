# E2EApi — Modder Documentation

`E2EApi.dll` is the modding API for The Escapists 2 (BepInEx 5, net35). The
map editor mod (`MapEditorMod.dll`) is its first consumer and contains no
direct game-type references — everything goes through this API. Other plugins
can reference `E2EApi.dll` the same way.

Sources: `src/E2EApi/`. Current version: `E2EApiInfo.Version = 0.1.0`
(semver; plugin GUID `org.anonymusdennis.e2e.api`).

## Threading rules

**Everything in this API must be called on Unity's main thread** unless noted
otherwise. The one exception is `MainThread` itself, which exists precisely to
get you there from a background thread (the web UI server uses it for every
request):

```csharp
// from any thread:
E2EApi.MainThread.Post(() => Blocks.SelectBrush(42));          // fire and forget
var json = E2EApi.MainThread.Run(() => SomeMainThreadCall());  // blocking, 5s default timeout
```

Harmony patches applied by the API are lazy: nothing is patched until the
first feature that needs a patch is used, so an idle API costs nothing.

---

## Core (`E2EApi`)

### `E2EApiInfo`

Identity constants: `Guid`, `Name`, `Version`.

### `MainThread`

Marshals work onto the main thread (the only thread-safe entry point).

- `Post(Action)` — queue for the next frame, fire and forget.
- `Run<T>(Func<T>, int timeoutMs = 5000)` / `Run(Action, int)` — run on the
  main thread and block until done; throws `TimeoutException` on timeout and
  rethrows exceptions from the action.

### `PatchRegistry`

Central Harmony registry — one Harmony instance for the whole API, one place
to see what is patched (conflict safety).

- `Harmony` — the shared instance.
- `EnsurePatched(Type patchClass)` — apply a `[HarmonyPatch]` class once;
  later calls are no-ops.
- `AppliedPatches` — snapshot of the patch classes applied so far.

### `Diagnostics`

Introspection for debugging (backs the web UI's debug endpoints). Main thread.

- `OverlayDebugJson()` — JSON snapshot of editor camera/brush/overlay state.
- `LoadStateJson()` — JSON snapshot of the `GlobalStart` loading state machine
  (useful when a load appears stuck).

---

## `E2EApi.Editor`

### `Blocks`

Spawnlist / building-block registry, wrapping `BuildingBlockManager`. The
unlock flags drive a Harmony reimplementation of
`BuildingBlockManager.GetBlocksOfType` (only active while an unlock is on).

- `ShowEditorOnly` (bool) — show dev-only (`m_EditorOnly`) blocks.
- `IgnoreLayerRestrictions` (bool) — offer every block on every layer.
- `IgnoreCompletionState` (bool) — also list incomplete/invalid blocks.
- `All` — raw `BaseBuildingBlock[]` (null outside a level/editor scene).
- `Get(int blockId)` — block by ID, or null.
- `GetSpawnList()` — `List<BlockInfo>` snapshot, game-type-free, for UIs.
- `Describe(BaseBuildingBlock)` — full researched `BlockInfo` for one block.
- `SelectBrush(int blockId)` — make a block the active editor brush (editor
  must be open).
- `CurrentBrush` — block id currently on the brush (-1 = none/no editor).
- `Register(BaseBuildingBlock)` — inject a custom block into the registry.

### `BlockInfo` / `BlockKind`

Game-type-free snapshot of a placeable: `Id`, `DisplayName`, `Kind`
(Tile/Wall/Decoration/Object/Complex/Room), `EditorOnly`, `Icon` (atlas
material), plus researched metadata (`InternalName`, `ClassName`,
`PrefabName`, `ValidLayers`, `Themes`, `Purpose`, `IsZone`, `LimitGroup`,
`Notes` — curated explanations of what the block actually does in-game).

### `BlockIcons`

Renders block icons to PNG bytes. Atlas icons are cropped via a
RenderTexture round-trip; blocks whose icon is the red "TEMP" placeholder get
a live render of their actual prefab instead. Cached. Main thread only.

- `GetPng(int blockId)` — PNG bytes or null.
- `ClearCache()`
- `DiagnoseJson()` — per-block icon/visual report (backs `/api/icon-report`).

### `Placement`

Programmatic map editing through `BuildingInstructionManager`, the game's
ordered edit log — every API edit is saved, validated and undoable exactly
like a hand edit. Only works while the level editor is active.

- `IsAvailable`
- `PlaceBlock(blockId, x, y, seed = 0, checkLimits = false)` — coordinates 0–119.
- `PlaceArea(blockId, x, y, width, height, seed = 0)`
- `Delete(x, y, DeleteType)`
- `AddToZone(zoneId, x, y, width, height)`
- `Command(CommandsEnum, value)` — layer change, level settings, …
- `GetCursorTile(out x, out y)` — editor cursor tile.
- `BeginUndoGroup()` / `EndUndoGroup()` — group edits into one undo step.

### `Grid`

Tile-grid coordinate helpers (120×120, 6 layers). Works in both the editor
scene and play mode (different coordinate spaces, handled internally).

- `TileToWorld(x, y, layer = 1)` — `Vector3?` centre of a tile.
- `WorldToTile(world, out x, out y, layer = 1)`
- `CurrentEditorLayer` — active build layer while the editor is the active
  surface, -1 otherwise (incl. during a play-test).
- Constants: `Width`, `Height`, `LayerCount`.

### `Limits`

Limitation-group overrides (placement caps, guard/inmate counts), re-applied
every frame while a manager exists.

- `OverrideTotal(group, total)` / `ClearOverride(group)`
- `SetGuardInmateAvailability(count)` — up to `count` guards and inmates
  regardless of placed cells (0 = vanilla). The V0 mod's signature feature.
- `GetNamedGroupIndex(name)`, `ClearAllOverrides()`

### `Restrictions`

Master "ignore all mapping restrictions" switch.

- `IgnoreAll` (bool) — when on: any block on any layer, level validation
  always passes (unfinished maps can be saved as Finished/uploaded), WIP maps
  become playable from the menu, and all `Blocks` unlocks are forced on.

### `EditorCamera`

- `LockPan` (bool) — disable mouse edge-panning (keyboard still works);
  `ApplyLock()` re-applies it on a new editor session.
- `ExtendZoom(extraSteps = 2)` — extra zoom-in steps below the vanilla
  minimum; once per editor session.
- `JumpTo(x, y)` — centre the camera on a tile.

### `EditorSession`

Programmatic entry into the editor flow. Main thread only.

- `Enter(levelFile = "")` — start an editor session from the main menu
  (empty = new map).
- `PlayTest()` — playtest the currently open map via the vanilla preview flow
  (handles the prison-checker dialog correctly).
- `SkipTitle()` — dismiss the "press spacebar" title screen without input
  hardware; returns a status string.

### `VanillaEditor`

- `MouseSuppressed` (bool) — vanilla editor ignores all mouse editing while a
  mod tool owns the mouse (keyboard camera still works).
- `SetBrushVisible(bool)` — hide/show the vanilla brush preview.

### `CustomAssets`

AssetBundle loading and placement. Bundles must be built with a **Unity 5.5-compatible**
pipeline or `LoadFromFile` returns null. See `docs/bundle-build-pipeline.md`.
Default location: `BepInEx/plugins/E2EMapEditor/bundles/`.

- `BundlesFolder` — absolute path to the bundles directory (created on first access).
- `ListBundles()` → `string[]` — filenames of all bundles in the bundles folder.
- `ListAssets(bundlePathOrName)` → `string[]` — asset names inside a bundle
  (pass a filename or full path; names are stripped of prefix/extension for display).
- `LoadBundle(path)` — load (or return the cached) bundle.
- `Load<T>(bundlePath, assetName)` — asset out of a bundle, or null.
- `Spawn(bundlePath, assetName, position)` — instantiate a prefab.
- `UnloadAll(destroyLoadedObjects = false)`

### `CustomAssetPlacements`

Map-persistent custom asset placements. Each placement ties a bundle+asset to a
tile coordinate. Instances are spawned when entering the editor and destroyed on
exit. Persisted in the sidecar `[custom_assets]` section.

- `Initialise()` — wire save/load hooks (called by the mod on startup).
- `Place(bundleName, assetName, x, y, layer, offX, offY, offZ, rotY)` — place a
  prefab at a tile; replaces any existing placement at the same tile.
- `EraseAt(x, y, layer)` → int — remove placement and destroy instance; returns count removed.
- `GetAt(x, y, layer)` → `Placement?` — query placement at a tile.
- `Clear()` — remove all placements and instances.
- `SpawnAll()` / `DestroyAll()` — manual lifecycle control.
- `All()` / `Count` / `Version`

`Placement` fields: `BundleName`, `AssetName`, `X`, `Y`, `Layer`, `OffX`, `OffY`, `OffZ`, `RotY`.

---

## `E2EApi.Players`

### `Player`

Wrapper around the game's `Gamer`/`Player` pair.

- Static: `GetLocal()`, `GetAllLocal()` (couch co-op), `GetAll()` (incl.
  remote), `InfiniteEnergy` (game dev flag, applies to all players).
- Escape hatches: `Gamer`, `Pawn` (the underlying game objects).
- `IsValid` — true while the player has a live pawn in a level; most members
  no-op/return 0 otherwise.
- Stats: `Health`, `Energy`, `Money`, `Heat` (get/set), `Heal()`.
- `Name`
- `GetTile(out x, out y, out floor)` — current tile position.
- `TeleportToTile(x, y, floor = -1)` — uses the game's own
  `Character.Teleport`, so networking/floor state stay consistent.
- `ClearSuspicion()` — zero heat, clear wanted flags, mark disguised.

---

## `E2EApi.Items`

### `Items`

Item registry and inventory operations, wrapping `ItemManager`.

- `GetData(itemDataId)` — `ItemData` definition or null.
- `Allowed` — items allowed in the current level.
- `KeyItems` — key items for the current level.
- `Give(player, itemDataId)` — give an item via the game's assignment RPC
  (network-aware).

---

## `E2EApi.Events`

### `GameEvents`

Lifecycle events; the Harmony patch is applied lazily on first subscription.
Handlers run on the main thread; exceptions are caught and logged.

- `LevelLoaded` / `LevelUnloaded` — any level (play or editor).
- `EditorEntered` / `EditorExited` — the level editor specifically.
- `IsInEditor` — true while an editor manager exists.

---

## `E2EApi.Features`

### `ElectricFences`

Any tile can be marked "electrified"; electrified tiles damage characters
standing on them in play mode (not in the editor). Persisted in the
`Level.e2e` sidecar (section `[fences]`).

- `Initialise()` — wires up sidecar save/load (called implicitly by setters).
- `SetElectrified(x, y, on)`, `IsElectrified(x, y)`, `Toggle(x, y)`
- `All()` — enumerate electrified tiles; `Count`; `Clear()`
- `DamagePerTick` (float, default 10) — damage per half-second of contact.
- `Version` — bumped on every change (overlays use it to rebuild).

### `Triggers`

Button/trigger links: a source tile linked to a target tile with an action
(currently `ToggleFence`). Persisted in the sidecar (section `[triggers]`).

- `AddLink(srcX, srcY, dstX, dstY, action = ToggleFence)`
- `RemoveLinksAt(x, y)`, `Clear()`, `All`, `Version`
- `ActivateAt(x, y)` — fire all links whose button is on that tile.
- `ActivateUnder(player)` — fire links under a player's feet (the mod binds
  this to the E key in play mode).

### Overlays and tools

- `FenceOverlay` — checkered highlight over every electrified tile (editor
  and play mode). `Enabled` (default true).
- `TriggerArrows` — arrows from each button to its target in the editor;
  clicking an arrow jumps the camera between its two ends. `Enabled`.
- `XRay` — outlines + name labels over invisible/dev-only placed blocks in
  the editor, making hidden content inspectable. `Enabled`,
  `GetInfoAt(x, y)` → `BlockInfo` (backs the hover tooltips).
- `TileMarker` — single highlight quad used as a mod-tool cursor:
  `Show(x, y, color)`, `Hide()`.

### `Cheats`

Play-mode helpers, not persisted: `KnockOutGuards()`, `KnockOutDogs()` —
return the number affected.

### `GameMap`

The game's per-floor map textures + floor metadata (play mode only; the
editor has no `FloorManager`).

- `GetFloors()` — `List<FloorInfo>` (`Index`, `Name`, `IsStartFloor`,
  `HasTexture`).
- `GetFloorPng(floorIndex)` — PNG bytes of the floor map, or null (handles
  non-readable textures via a RenderTexture round-trip).

---

## `E2EApi.Persistence`

### `ModExtras`

The mod-extras sidecar: a `Level.e2e` text file stored next to a custom map's
`Level.dat`. Vanilla never touches it, so maps stay 100% vanilla-compatible.
Saving/loading is hooked into `SaveManager.SaveUserLevel` /
`SaveManager.LoadTheLevel` via Harmony; an empty extras set deletes the file.

Format (UTF-8): magic line `E2EX1`, a `requiresMod=` line, then named
`[sections]` with one line per entry (section-defined syntax).

- `Current` — extras of the currently loaded/edited map.
- `Saving` / `Loaded` (events) — features write/read their sections here
  (this is how `ElectricFences` and `Triggers` persist; new features should
  do the same).
- `RequiresMod` (bool) — "this map needs the mod" flag.
- `Section(name)` — get-or-create a section's line list; `ClearSection(name)`.
- `Serialize()` / `Deserialize(text)`, `IsEmpty`.

### `MultiplayerGate`

Multiplayer compatibility handshake via Photon room custom properties.

When a map with `RequiresMod = true` is loaded in a Photon room (PUN 1.x),
the host broadcasts a room property `"e2e_mapeditor"` = current mod version.
Modded clients read this to confirm the requirement; unmodded clients see the
vanilla-fallback disclaimer map and have no gate code at all.

Photon types are resolved at runtime via reflection so `E2EApi.dll` compiles
without a hard dependency on the Photon assembly.

- `Initialise()` — wire up sidecar and level hooks (idempotent).
- `IsMultiplayer` — true when Photon reports being in a room.
- `IsHost` — true when the local player is the master client.
- `IsRequiredInCurrentRoom` — true when the active room carries the property.
- `RoomModVersion` — version string from the room property, or `null`.
- `AnnounceRequiresMod()` — host sets the room property; returns `false` if not
  the host or not in a room.
- `AnnounceNoLongerRequiresMod()` — host clears the property.
- `RefreshRoomState()` — re-reads room properties and fires events if state
  changed (call this from a Photon `OnRoomPropertiesUpdate` callback).
- `RoomRequiresMod` / `RoomNoLongerRequiresMod` (events) — fired on the main
  thread when the requirement transitions.

### `WorkshopInterop`

Steam Workshop interoperability for mod-required maps.

On upload (extends `VanillaFallback`'s existing hook): if the map has mod
content, writes `e2e_workshop_meta.txt` alongside the sidecar in the UGC
content folder. The file records `e2e_version` and `requires_mod`.

On download (via `GetCustomLevelFilePath`): if the map's folder contains
`e2e_workshop_meta.txt`, fires `WorkshopMapLoaded` with a `WorkshopMapMeta`
object so the UI can show a compatibility banner.

- `Initialise()` — apply patches (idempotent).
- `WorkshopMapLoaded` (event) — `Action<WorkshopMapMeta>`; fired on the main
  thread when a Workshop map with the meta file is resolved for play.
- `WorkshopMapMeta.ModVersion`, `.RequiresMod`, `.MapDirectory`,
  `.CompatibilityNote`.

---

## `E2EApi.UI`

Raw-uGUI toolkit (Unity 5.5-safe, no sprite assets needed). Main thread.

- `ModWindow` — movable in-game window (dark panel, draggable title bar,
  close button). `Create(title, w, h)`, `Content`, shared `OverlayCanvas`.
- `TabbedWindow` — `ModWindow` with a tab strip. `Create(...)`,
  `AddTab(label)` → content panel, `Select(index)`.
- `UiFactory` — `Label`, `Button`, `Toggle` factories matching the window
  style.
- `WindowMode` — display mode: `ForceWindowed(w = 0, h = 0)`,
  `SetFullscreen(...)`, `IsWindowed`.

---

# Web API (MapEditorMod)

`MapEditorMod` runs a minimal HTTP server (`src/MapEditorMod/WebUi/`) on
`http://127.0.0.1:8723/` (port configurable, localhost only). All game access
inside handlers is marshalled through `MainThread.Run`, so the endpoints are
safe to call from anywhere. Responses are JSON unless noted; action endpoints
generally return `{"ok":bool}` or `{"ok":bool,"msg":"…"}`.

| Method | Path | Query | Returns |
|--------|------|-------|---------|
| GET | `/` | — | the editor UI (HTML) |
| GET | `/api/state` | — | editor state: `inEditor`, cursor tile, fence/trigger counts, active tool + hint, pending link switch, `requiresMod`, all bool `settings`, all `numbers` |
| GET | `/api/blocks` | — | spawnlist array: `id`, `name`, `kind`, `editorOnly`, `hasIcon`, `internalName`, `className`, `prefab`, `layers`, `themes`, `purpose`, `zone`, `limitGroup`, `desc`, `notes` |
| GET | `/api/icon/{id}.png` | — | block icon PNG (404 if the block has none) |
| GET | `/api/icon-report` | — | per-block icon diagnostics (`BlockIcons.DiagnoseJson`) |
| POST | `/api/brush/{id}` | — | select block `{id}` as the editor brush |
| POST | `/api/tool` | `name=paint\|erase\|link\|none` | activate a mod editor tool (electricity paint/erase, trigger link) |
| POST | `/api/fence/cursor` | — | toggle an electric fence on the tile under the editor cursor |
| POST | `/api/fence/set` | `x`, `y`, `value=true\|false` | set a fence on an explicit tile |
| POST | `/api/trigger/cursor` | — | two-step trigger linking at the cursor: first call sets the button tile, second call links to the target |
| POST | `/api/requiresmod` | `value=true\|false` | set the map's "requires mod" sidecar flag |
| POST | `/api/clear-extras` | — | clear all fences and trigger links |
| POST | `/api/setting` | `name`, `value=true\|false` | flip a bool setting: `devBlocks`, `layers`, `completion`, `restrictions`, `xray`, `fenceOverlay`, `arrows`, `cameraLock`, `forceWindowed`, `autoOpen`, `infiniteEnergy` |
| POST | `/api/numsetting` | `name`, `value` (float) | set a numeric setting: `guardCap`, `zoomSteps`, `fenceDamage`, `windowWidth`, `windowHeight` |
| GET | `/api/floors` | — | play-mode floor list: `index`, `name`, `start`, `hasMap` |
| GET | `/api/map/{floor}.png` | — | floor map texture PNG (404 without one) |
| GET | `/api/map/v/{virtualIndex}.png` | — | map texture for a virtual layer (resolves to its backing physical floor; 404 if unavailable) |
| GET | `/api/player` | — | local player snapshot: `present`, `name`, `health`, `energy`, `money`, `heat`, `infiniteEnergy`, `tile {x,y,floor,virtualLayer}` (`virtualLayer` is -1 on vanilla maps) |
| POST | `/api/teleport` | `x`, `y`, `floor` (or `virtualLayer`) | teleport the local player to a tile; pass `virtualLayer` instead of `floor` on custom-geometry maps |
| GET | `/api/geometry` | — | virtual layer geometry: `width`, `height`, `originX`, `originY`, `layers[]`, `selected`, `hash`, `nativeCompatible`, `warning` |
| GET | `/api/layers` | — | alias for `/api/geometry` (virtual layer list + selected index) |
| POST | `/api/layers/select` | `index` | select a virtual layer in the editor (editor must be open) |
| POST | `/api/geometry/select` | `index` | select a virtual layer in the editor |
| POST | `/api/geometry/add` | `type=Underground\|Ground\|Vent\|Roof` | add a virtual layer |
| POST | `/api/geometry/remove` | `index` | move a virtual layer to the trash |
| POST | `/api/geometry/move` | `index`, `delta` | reorder a virtual layer |
| POST | `/api/geometry/duplicate` | `index` | duplicate a virtual layer |
| POST | `/api/geometry/type` | `index`, `type` | change the virtual layer type |
| POST | `/api/geometry/hide` | `index`, `hidden=true\|false` | hide/show a virtual layer in-game |
| POST | `/api/geometry/restore` | `trashIndex` | restore a trashed virtual layer |
| POST | `/api/geometry/bounds-delta` | `field`, `delta` | adjust map bounds (width/height/originX/originY) by a delta |
| POST | `/api/geometry/reset` | — | reset to vanilla 6-layer 120×120 layout |
| GET | `/api/debug/floor-registry` | — | dump `FloorTypeRegistry` state: physical→type map and virtual order |
| GET | `/api/debug/virtual-floors` | — | list all virtual layers with index, name, type, backingLayer, hidden |
| GET | `/api/custom-assets/bundles` | — | list bundle filenames in `BepInEx/plugins/E2EMapEditor/bundles/` |
| GET | `/api/custom-assets/list` | `bundle` | list asset names inside the named bundle |
| GET | `/api/custom-assets` | — | list all placements: `[{bundle, asset, x, y, layer}]` |
| POST | `/api/custom-assets/place` | `bundle`, `asset`, `x`, `y`, `layer`; optional `offX`, `offY`, `offZ`, `rotY` | place a custom asset at a tile |
| POST | `/api/custom-assets/place-cursor` | `bundle`, `asset`; optional `offX`, `offY`, `offZ`, `rotY` | place at the current editor cursor tile |
| POST | `/api/custom-assets/erase` | `x`, `y`, `layer` | erase custom asset at a tile |
| POST | `/api/custom-assets/erase-cursor` | — | erase at the current editor cursor tile |
| POST | `/api/custom-assets/clear` | — | clear all custom asset placements |
| GET | `/api/map-settings` | — | return all per-map settings as `{settings:{key:value,…}}` |
| POST | `/api/map-settings/set` | `key`, `value` | set a per-map setting (persisted to Level.e2e) |
| POST | `/api/map-settings/unset` | `key` | remove a per-map setting |
| POST | `/api/map-settings/clear` | — | remove all per-map settings |
| GET | `/api/multiplayer` | — | current Photon room state: `{inRoom, isHost, requiresMod, roomModVersion}` |
| POST | `/api/multiplayer/announce` | — | host sets room property `e2e_mapeditor` = current version; returns `{ok}` |
| POST | `/api/multiplayer/refresh` | — | re-read room properties and return updated state |
| POST | `/api/cheat` | `name=heal\|energy\|money\|stealth\|ko-guards\|ko-dogs` | run a play-mode cheat |
| POST | `/api/dev/skip-title` | — | dismiss the "press spacebar" title screen |
| POST | `/api/dev/enter-editor` | `file` (optional) | enter the level editor (empty `file` = new map) |
| POST | `/api/dev/playtest` | — | playtest the map currently open in the editor |
| GET | `/api/debug` | — | overlay/camera debug snapshot (`Diagnostics.OverlayDebugJson`) |
| GET | `/api/debug/load` | — | loading state machine snapshot (`Diagnostics.LoadStateJson`) |

Example session from the shell:

```sh
curl http://127.0.0.1:8723/api/state
curl -X POST 'http://127.0.0.1:8723/api/tool?name=paint'
curl -X POST 'http://127.0.0.1:8723/api/fence/set?x=60&y=60&value=true'
curl -X POST 'http://127.0.0.1:8723/api/cheat?name=heal'
```
