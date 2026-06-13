# V0 Mod Feature Inventory (the minimum feature bar)

Source: decompile of `old/00-legacy-bepinex-v0/MapEditor.dll`
(BepInEx plugin `org.anonymusdennis.net.MapEditor` v1.0.0.0, Feb 2022).
Decompiled output: `reference/v0-mod/`.

## What V0 actually did

### 1. Unlock editor-only (dev) blocks in the spawnlist

Harmony **prefix replace** on `BuildingBlockManager.GetBlocksOfType(...)`
(`reference/v0-mod/MyFirstPlugin.patches/BBManagerFix.cs`):

- Reimplements the block enumeration loop with `flag = true`, which:
  - includes blocks where `m_EditorOnly == true` (dev-only content)
  - relaxes the `m_ValidLayers` check (treats every call like the
    "all layers" case `layer == 6`)
- Still respects: `m_OurBlockSets & filterTheme`, `m_BlocksPurpose & filterPurpose`,
  `m_Family & iFamily`, `m_AutomaticBlock`, variation selectability
- Maintains its own `Plugin.blockList` copy and clears it when it grows past
  `m_BuildingBlocks.Length - 5` (crude cache reset)
- Calls `BuildingBlockManager.SortBlockList(ref blockList)` before returning

### 2. Raise limitation-group caps

In `Plugin.Update()` (every frame, try/catch swallowed):

```csharp
BuildingBlockManager.GetInstance().m_LimitationGroups[21].m_CurrentTotal = 24;
BuildingBlockManager.GetInstance().m_LimitationGroups[20].m_CurrentTotal = 24;
```

Forces two specific limitation groups (indices 20, 21) to a higher count so more
instances of those limited blocks can be placed. (Which groups these are needs
confirming against the fresh decompile — likely high-value placeables.)

### 3. Save interception (stub, never wired up)

`LevelDetailsManager_fix.StoreTheLevel` — a prefix that fakes a successful save
callback (`RequestResultEnum = 1`) and nulls the first argument. The patch for
`LimitationGroupChanged` was resolved in `Awake` but **never applied** (only the
`GetBlocksOfType` patch is registered). Dead code, but shows intent: bypass save
validation / limitation re-checks.

## Minimum feature bar for the new mod (Phase 3)

| # | Feature | V0 mechanism | New-mod home |
|---|---------|--------------|--------------|
| 1 | Dev/editor-only blocks visible in spawnlist | `GetBlocksOfType` prefix | `E2EApi.Editor.Blocks` filter options |
| 2 | Layer-restriction bypass | same patch (`layer == 6` path) | `E2EApi.Editor` placement options |
| 3 | Unlimited / raised limitation groups | `m_CurrentTotal` poke per frame | `E2EApi.Editor.Limits` (proper patch, not per-frame poke) |
| 4 | Save validation bypass | unused `StoreTheLevel` prefix | only if needed; do saves properly instead |

## Key game symbols V0 depended on

- `BuildingBlockManager`: `GetInstance()`, `m_BuildingBlocks`, `m_LimitationGroups`,
  `GetBlocksOfType(...)`, `SortBlockList(ref List<int>)`
- `BaseBuildingBlock`: `m_EditorOnly`, `m_ValidLayers`, `m_OurBlockSets`,
  `m_BlocksPurpose`, `m_Family`, `m_AutomaticBlock`, `m_Variation`,
  `m_VariationSelectable`
- `LimitationGroup`: `m_CurrentTotal`, `LimitationGroupChanged`
- `LevelDetailsManager`: `StoreTheLevel`, `RequestResult` / `RequestResultEnum`
- Enums: `BuildingBlockType`, `LevelLayers`, `LayersEnvironment`, `BlockSet`,
  `PurposeGroups`, `CompletionState`
