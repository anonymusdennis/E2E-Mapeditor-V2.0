# Level.dat Format (custom map saves)

Reversed from `Assembly-CSharp.dll` (`LevelDetailsManager`, `BuildingInstructionManager`,
`DataHelpers.ByteArrayConversion`). Verified against the 5 real saves in
`old/06-game-saves/` by `tools/leveldat/leveldat.py` (all checksums OK).

## Primitives

- All integers little-endian. `i32` = 4 bytes, `i64` = 8 bytes.
- **String:** `charCount:i32`, then `charCount` UTF-16LE code units (2 bytes each).
- **Encrypted string** (name, description, author, edited-name, directory):
  rolling cipher per byte pair, key `0x7B`, seed `99`:

  ```
  encrypt: out = (b + prev) ^ 0x7B ; prev = out      (prev seeded 99, chained per byte)
  decrypt: b2 = ((c2 ^ 0x7B) - prev) & 0xFF ; prev = c3_cipher ; b3 = ((c3 ^ 0x7B) - c2_cipher) & 0xFF
  ```

## Chunk framing

Everything is `flag:u8, length:i32, payload[length]` where the payload's final
byte is `ChunkEnd = 0x66` (102), included in `length`.

## Top-level layout

```
File_Header_V1 (0x00)
  length:i32                  # bytes from after checksum to end of header
  checksum:i32                # sum(byte ^ 123) over those bytes
  fields...                   # sub-chunks, see below
  ChunkEnd (0x66)
LevelInstructions_V1 (0xC9 / 201)
  length:i32
  encryptedBody[length-1]     # see below
  ChunkEnd (0x66)
```

## Header fields (SerializationFlag enum)

| Flag | Name | Payload |
|------|------|---------|
| 1 | Type_Header_V1 | `u8` LevelType: 0 UNKNOWN, 1 WorkInProgress, 2 Finished |
| 2 | Version_Header_V1 | `i32` level version (increments per save) |
| 3 | Difficulty_Level_V1 | `u8`: 0 Easy, 1 Medium, 2 Hard |
| 4 | Name_Header_V1 | encrypted string |
| 5 | Desc_Header_V1 | encrypted string |
| 6 | Author_Name_Header_V1 | encrypted string |
| 7 | Edited_Name_Header_V1 | encrypted string |
| 8 | Created_Date_Header_V1 | `i64`, decimal `YYYYMMDDHHMM` |
| 9 | Edited_Date_Header_V1 | `i64`, decimal `YYYYMMDDHHMM` |
| 10 | BuildingBlock_Type_V1 | `s8`: 0 Standard |
| 11 | Routines_Header_V1 | `count:u8` + `count` sbytes (24 hourly routine slots) |
| 12 | Inmates_Header_V1 | `s8` inmate count |
| 13 | Guards_Header_V1 | `s8` guard count |
| 14 | Directory_Header_V1 | encrypted string (save dir GUID) |
| 15 | RandomGroups_Header_V1 | `count:u8` + sbytes |
| 16 | LastUploadedTo_Header_V1 | `u64` Steam Workshop ID |
| 17 | Filter_Settings_V1 | (BuildingBlock_FilterManager data) |
| 18 | DataVersion_Header_V1 | `i32`: 1 V1_InitialRelease, 2 V2_AddedZoneEditing |
| 19 | PrisonOutfitType_V1 | `i32` LevelScript.PRISON_ENUM |
| 20 | PrisonMusicType_V1 | `i32` LevelScript.PRISON_ENUM |

Each field is itself `flag, len:i32, value..., 0x66`.

## LevelInstructions_V1 body encryption

The body (everything between `length` and the final `0x66`) is encrypted with a
rolling byte cipher whose keys depend on level type:

| LevelType | xor key `b` | rolling seed `b2` |
|-----------|------------|-------------------|
| WorkInProgress | 167 | 23 |
| Finished | 71 | 15 |

```
encrypt: c[i] = (p[i] + b2) ^ b ; b2 = c[i]
decrypt: p[i] = (c[i] ^ b) - b2 ; b2 = c[i]
```

Decrypted body = `checksum:i32` (sum of `byte ^ 0x5F` over the rest) followed by
the instruction sublists (WorkInProgress) or a `FinishedLevelInstructions` blob
(Finished).

## Instruction sublists (WorkInProgress)

Each is `flag, len:i32, total:i32, elements..., 0x66`:

| Flag | Name | Element |
|------|------|---------|
| 202 | List_InstructionList_V1 | `type:u8` (Instruction_* 100-107), `index:i32` — the master ordered edit log; indexes into the per-type lists below |
| 203 | List_Complex_V1 | `blockID:i32` + nested complex instruction data |
| 204 | List_Single_V1 | `blockID:i32, x:u8, y:u8, seed:i32` |
| 205 | List_Wall_V1 | `blockID:i32, x:u8, y:u8, seed:i32` |
| 206 | List_Area_V1 | `blockID:i32, x:u8, y:u8, seed:i32, xCount:u8, yCount:u8` |
| 207 | List_Area_Wall_V1 | same as 206 |
| 208 | List_Commands_V1 | `command:u8 (CommandsEnum), value:i32` |
| 209 | List_Delete_V1 | `deleteType:u8, x:u8, y:u8` |
| 210 | List_Zone_V1 | `action:u8, zoneType:u8, left:u8, bottom:u8, w:u8, h:u8, id:i32, printLen:i32, zonePrint[printLen]` |

The map is **replayed**: loading walks `List_InstructionList_V1` in order and
re-executes each edit. Layer changes are commands (`Instruction_IncrementLayer`/
`DecrementLayer` in the master list).

## Map dimensions

Grid is 120 × 120 tiles (14400) across 6 building layers
(`BaseLevelManager.m_BuildingLayers[1..5]` checked in validation code).

## Implications for the mod (Phase 7)

- Sidecar approach is safest: vanilla files stay untouched; our extras
  (fences, triggers, custom assets) live next to `Level.dat`.
- Alternatively unknown flags are *skipped* by `FindHeader`/deserializers
  (they read len and jump), so a custom chunk with an unused flag id appended
  after `LevelInstructions_V1` may survive vanilla load — needs testing
  (the strict `flag = data[num++] == 102` checks may reject it; test in Phase 7).
- `Version_Header_V1`/`DataVersion_Header_V1` gates: vanilla refuses files with
  newer data versions — our extras must not bump these.

## Tool

`tools/leveldat/leveldat.py <Level.dat>` — dumps header fields, decrypts the
instruction body, verifies both checksums, lists all sublists with counts.
