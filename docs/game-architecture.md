# The Escapists 2 — Game Architecture (modding view)

Engine: **Unity 5.5.0p4, Mono backend**, .NET 3.5-era profile.
Source: fresh ilspycmd decompile in `reference/decompiled/Assembly-CSharp/`
(3,700+ classes). Older 2024 dnSpy dump for diffing: `old/04-escapists-map-edit-mod/`.

## Level editor core

### Class hierarchy

```
BaseLevelManager (abstract, MonoBehaviour)
├── EditorLevelEditorManager     # the in-editor implementation (V1)
├── EditorLevelEditorManagerV2   # V2 (zone editing era)
├── GameLevelEditorManager       # replays instructions when PLAYING a custom map
└── GameLevelEditorManagerV2
```

- `BaseLevelManager`: owns the tile model. Grid is **120 × 120 tiles per layer**
  (`c_LayerTileCount = 14400`), 6 layers in `m_BuildingLayers[]`. Each
  `LayerDataCollection` holds parallel arrays: `m_TileProperties[]` (bitmask
  `TileProperty`), `m_TileTileIDs[]`, `m_WallTileIDs[]`, `m_ObjectTileIDs[]`
  plus the spawned `GameObject[]`s and `TileSystem` renderers.
- `EditorLevelEditorManager` (`GetLevelEditorInstance()`): the methods that
  actually mutate the map — all take `BuildingInstructionManager` elements:
  - `AddSingle` / `RemoveSingle` (objects), `AddSingleWall` / `RemoveSingleWall`
  - `AddArea` / `RemoveArea`, `AddAreaWall` / `RemoveAreaWall`
  - `CreateZone` / `DeleteZone` / `AddToZone` / `SubtractFromZone`
  - `AddCommand` / `RemoveCommand` (layer switches, settings)
  - `AddDelete` / `RestoreDelete`
  - `UpdateTiles()` refreshes visuals

### Instruction system (also the save format)

`BuildingInstructionManager.LevelInstructions` is an **ordered edit log**: every
editor action appends an element to a per-type list (Once/Wall/Area/AreaWall/
Complex/Command/Delete/Zone) plus a `(type, index)` entry in the master
instruction list. Loading a map **replays** the log. This is exactly what is
serialized into `Level.dat` (see `docs/leveldat-format.md`).

→ Mod implication: to place blocks programmatically, build an
`InstructionOnceElement` etc. and call the corresponding `Add*` on
`EditorLevelEditorManager`. Undo support comes free (`bStorePrevious`).

### Building blocks

- `BuildingBlockManager` (singleton, `GetInstance()`): registry of all placeable
  blocks in `m_BuildingBlocks[]` (indexed by block ID).
  - `GetBlocksOfType(ref List<int>, BuildingBlockType, LevelLayers, LayersEnvironment, BlockSet, PurposeGroups, family, automatic, validity, bOnlySelectable)`
    — the spawnlist query the vanilla UI uses. **V0 patched this** to expose
    dev-only blocks.
  - `AddNewBuildingBlock(BaseBuildingBlock)` — runtime block injection works
    (proven by the old MelonLoader experiments in `old/01-*/buttons.cs`).
  - `m_LimitationGroups[]` (`LimitationGroup`): per-group placement caps
    (e.g. "Escape" markers, "RollCall"). `GetNamedLimitationIndex(name)`.
  - `SortBlockList(ref List<int>)`.
- `BaseBuildingBlock` (abstract MonoBehaviour, ~115 public members). Key fields:
  - `m_ID`, `m_BlockNameID` (localization key), `m_UIImage` (icon material)
  - `m_EditorOnly` — dev-only gate (the Phase 7 unlock)
  - `m_ValidLayers` bitmask, `m_OurBlockSets` (theme), `m_BlocksPurpose`,
    `m_Family`, `m_Variation` / `m_VariationSelectable`, `m_AutomaticBlock`
  - `m_LimitationGroup` index
  - version gates: `m_FirstVersionAllowed` / `m_LastVersionAllowed`
  - subclasses: `BuildingBlock_Object`, tiles, walls, complex (multi-tile) etc.

### Editor UI

- `LevelEditor_Controller`: the editor screen controller — edit modes
  (`EditMode`), brush (`BlockChanged`), zoom, undo/redo buttons, snapshot
  preview rendering, marquee. Events: `EditModeChanged`, `BlockChanged`,
  `ZoomLevelChanged` — good API hook points.
- ~48 `LevelEditor_*` classes: tabs (`LevelEditor_BaseTab`,
  `LevelEditor_BlockSection`), tooltips, error list, cursor, brushes
  (`LevelEditorBrushController`), popouts (`BaseLevelEditor_BasePopout`).
- `LevelEditor_ZoneManager`: zone model (`Zone`, `ZoneTypes` in
  `ZoneDetailsManager`).
- `T17EventSystem : EventSystem` + Rewired input. The editor uses a
  "LevelEditor" input map.

### Save/load flow

```
LevelDetailsManager          # level metadata + (de)serialization + validation
  ├─ SerializeOurData / DeserializeOurData    (File_Header_V1)
  ├─ GetFrontendDataForLevel(ref List<byte>)  (menu list preview)
  ├─ StoreTheLevel / LoadTheLevel             (via SaveManager)
  ├─ ValidateWalkableAreas[V2]                (escape route validation)
  └─ c_CurrentLevelDataVersionNumber          (V1 / V2_AddedZoneEditing)
SaveManager                  # prison list, custom prison dirs (ESC2U<guid>)
  ├─ LoadTheLevel(dir, ref List<byte>)        reads <save>/Level.dat
  └─ EnumerateCustomPrisons / OnUserGeneratedContentUpdated (Workshop UGC)
PlatformIO / Platform        # path + Workshop abstraction
```

Saves live at `Saves/<steamid64>/ESC2U<guid>/Level.dat`
(finished uploads: `Level_Finished.dat`). Encodings + ciphers documented in
`docs/leveldat-format.md`; parser at `tools/leveldat/leveldat.py`.

## Player / gameplay layer

- `Gamer`: one per player (max 4); `m_PlayerObject` (the `Player` pawn),
  `m_RewiredPlayer` (input), `m_PhotonID` / `m_NetViewID` (network),
  `m_eLocation`, lifecycle events `OnDeleteRequested` / `OnDeleteImminent`.
  Static `m_GamerCount`.
- `Player` pawn + `CharacterStats` (health), heat system, `AICharacter_Guard`,
  `AICharacter_Dog` (detection — the old stealth experiments zeroed heat and
  knocked out guards/dogs).
- `CraftManager` — crafting recipes (old MelonLoader mod patched
  `CanCraftItem`-style checks).

## Multiplayer

Photon (PUN) based: `T17NetworkManager`, `CrossplayLobbyManager`, Photon IDs on
`Gamer`. Custom maps in MP: host sends the level; the Phase 9 "requires mod"
gate would ride on lobby custom properties.

## Modding approach (what this implies)

1. **BepInEx 5 (x64, Mono)** — confirmed workable (V0 was BepInEx).
2. Harmony patch points of interest:
   - `BuildingBlockManager.GetBlocksOfType` (spawnlist filter — V0 parity)
   - `LevelDetailsManager.StoreTheLevel` / `LoadTheLevel` (sidecar persistence)
   - `LevelEditor_Controller` events (UI lifecycle)
   - `LimitationGroup` caps (placement limits)
3. Programmatic editing goes through `EditorLevelEditorManager.Add*` with
   `BuildingInstructionManager` elements → saves and undo work for free.
4. Custom blocks: `AddNewBuildingBlock` + a `BaseBuildingBlock` subclass
   instance built at runtime (template: `old/01-melonloader-mod-vs-backups/`
   `buttons.cs`); icons via `m_UIImage` material.
5. Unity 5.5 = **uGUI** for our windows (no second OS window possible).
