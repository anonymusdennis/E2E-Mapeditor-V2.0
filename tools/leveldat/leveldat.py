#!/usr/bin/env python3
"""Parser/dumper for The Escapists 2 custom-map saves (Level.dat).

Format (reversed from Assembly-CSharp: LevelDetailsManager, ByteArrayConversion):

  stream  := chunk*
  chunk   := flag:u8 payload
  Most chunks: flag, len:i32le, <len bytes>, where the payload bytes end
  with a ChunkEnd byte (0x66) that is INCLUDED in len.

  File_Header_V1 (0x00): len:i32le, checksum:i32le, fields...
    checksum = sum(byte ^ 123) over the len bytes following the checksum int.
    fields are themselves chunks (flag, len, value..., 0x66) until ChunkEnd.

  Strings: i32le char count, then UTF-16LE code units; name/description are
  "encrypted" with a rolling add/xor (key 0x7B, seed 99).

Usage: leveldat.py <Level.dat> [--hex]
"""
import struct
import sys

FLAGS = {
    0: "File_Header_V1",
    1: "Type_Header_V1",
    2: "Version_Header_V1",
    3: "Difficulty_Level_V1",
    4: "Name_Header_V1",
    5: "Desc_Header_V1",
    6: "Author_Name_Header_V1",
    7: "Edited_Name_Header_V1",
    8: "Created_Date_Header_V1",
    9: "Edited_Date_Header_V1",
    10: "BuildingBlock_Type_V1",
    11: "Routines_Header_V1",
    12: "Inmates_Header_V1",
    13: "Guards_Header_V1",
    14: "Directory_Header_V1",
    15: "RandomGroups_Header_V1",
    16: "LastUploadedTo_Header_V1",
    17: "Filter_Settings_V1",
    18: "DataVersion_Header_V1",
    19: "PrisonOutfitType_V1",
    20: "PrisonMusicType_V1",
    100: "Instruction_FinishedTotal",
    101: "Instruction_DrawOnce",
    102: "Instruction_DrawArea",
    103: "Instruction_ChangeArea",
    104: "Instruction_DoNothing",
    105: "Instruction_IncrementLayer",
    106: "Instruction_DecrementLayer",
    107: "Instruction_Zone",
    201: "LevelInstructions_V1",
    202: "List_InstructionList_V1",
    203: "List_Complex_V1",
    204: "List_Single_V1",
    205: "List_Wall_V1",
    206: "List_Area_V1",
    207: "List_Area_Wall_V1",
    208: "List_Commands_V1",
    209: "List_Delete_V1",
    210: "List_Zone_V1",
}
CHUNK_END = 0x66

DIFFICULTY = {0: "Easy", 1: "Medium", 2: "Hard"}
LEVEL_TYPE = {0: "UNKNOWN", 1: "WorkInProgress", 2: "Finished"}
DATA_VERSION = {1: "V1_InitialRelease", 2: "V2_AddedZoneEditing"}


def get_int(data, pos):
    return struct.unpack_from("<i", data, pos)[0], pos + 4


def get_string(data, pos, encrypted=False):
    n, pos = get_int(data, pos)
    chars = []
    prev_key = 99
    for _ in range(n):
        b2, b3 = data[pos], data[pos + 1]
        pos += 2
        if encrypted:
            raw_b2 = b2
            b2 = ((b2 ^ 0x7B) - prev_key) & 0xFF
            prev_key = b3
            b3 = ((b3 ^ 0x7B) - raw_b2) & 0xFF
        chars.append(chr((b3 << 8) + b2))
    return "".join(chars), pos


def flag_name(f):
    return FLAGS.get(f, f"UNKNOWN_0x{f:02x}")


def parse_header_fields(data, pos, end, out):
    """Walk (flag, len, value..., 0x66) fields inside File_Header_V1."""
    while pos < end:
        flag = data[pos]
        pos += 1
        name = flag_name(flag)
        if flag == CHUNK_END:
            out.append(("ChunkEnd", ""))
            return pos
        length, pos = get_int(data, pos)
        field_end = pos + length  # includes trailing 0x66
        if flag in (4, 5, 6, 7, 14):  # encrypted strings
            val, _ = get_string(data, pos, encrypted=True)
            out.append((name, repr(val)))
        elif flag in (2, 18, 19, 20):  # int32 fields
            val, _ = get_int(data, pos)
            extra = ""
            if flag == 18:
                extra = f" ({DATA_VERSION.get(val, '?')})"
            out.append((name, f"{val}{extra}"))
        elif flag == 3:
            out.append((name, f"{data[pos]} ({DIFFICULTY.get(data[pos], '?')})"))
        elif flag == 1:
            out.append((name, f"{data[pos]} ({LEVEL_TYPE.get(data[pos], '?')})"))
        elif flag in (10, 12, 13):  # sbyte values
            val = struct.unpack_from("<b", data, pos)[0]
            out.append((name, str(val)))
        elif flag in (8, 9):  # int64 dates (DateTime ticks/binary)
            val = struct.unpack_from("<q", data, pos)[0]
            out.append((name, str(val)))
        elif flag == 16:  # ulong workshop id
            val = struct.unpack_from("<Q", data, pos)[0]
            out.append((name, str(val)))
        elif flag in (11, 15):  # byte-count-prefixed sbyte arrays
            cnt = data[pos]
            arr = list(struct.unpack_from(f"<{cnt}b", data, pos + 1))
            out.append((name, str(arr)))
        else:
            out.append((name, f"<{length} bytes>"))
        pos = field_end
    return pos


def decrypt_instructions(body, level_type):
    """Rolling decrypt of the LevelInstructions_V1 body (excludes final 0x66)."""
    key = 71 if level_type == 2 else 167  # Finished : WorkInProgress
    roll = 15 if level_type == 2 else 23
    out = bytearray(len(body))
    for i, enc in enumerate(body):
        out[i] = ((enc ^ key) - roll) & 0xFF
        roll = enc
    return bytes(out)


def parse_instruction_lists(body):
    """body = decrypted LevelInstructions payload: checksum:i32 then sublists."""
    checksum, pos = get_int(body, 0)
    calc = sum(b ^ 0x5F for b in body[4:])
    print(f"    body checksum={checksum} [{'OK' if calc == checksum else f'BAD (calc {calc})'}]")
    n = len(body)
    while pos < n:
        flag = body[pos]
        pos += 1
        name = flag_name(flag)
        if flag == CHUNK_END:
            continue
        if pos + 4 > n:
            print(f"    [{pos - 1:#06x}] {name} (truncated)")
            break
        length, pos = get_int(body, pos)
        sub_end = pos + length
        if flag in (202, 203, 204, 205, 206, 207, 208, 209, 210):
            count, _ = get_int(body, pos)
            print(f"    [{pos - 5:#06x}] {name} len={length} count={count}")
        else:
            print(f"    [{pos - 5:#06x}] {name} len={length}")
        pos = sub_end


def parse(data, hexdump=False):
    pos = 0
    n = len(data)
    level_type = 1  # WorkInProgress unless header says otherwise
    while pos < n:
        flag = data[pos]
        pos += 1
        name = flag_name(flag)
        if flag == CHUNK_END:
            print(f"[{pos - 1:#08x}] ChunkEnd")
            continue
        if pos + 4 > n:
            print(f"[{pos - 1:#08x}] {name} (truncated tail, {n - pos + 1} bytes)")
            break
        length, pos = get_int(data, pos)
        if flag == 0:
            checksum, cpos = get_int(data, pos)
            body_end = pos + length
            calc = sum(b ^ 123 for b in data[cpos:body_end])
            ok = "OK" if calc == checksum else f"BAD (calc {calc})"
            print(f"[{pos - 5:#08x}] {name} len={length} checksum={checksum} [{ok}]")
            fields = []
            parse_header_fields(data, cpos, body_end, fields)
            for fname, fval in fields:
                print(f"    {fname}: {fval}")
                if fname == "Type_Header_V1":
                    level_type = int(fval.split()[0])
            pos = body_end
        elif flag == 201:
            print(f"[{pos - 5:#08x}] {name} len={length} (encrypted body)")
            body = decrypt_instructions(data[pos:pos + length - 1], level_type)
            parse_instruction_lists(body)
            pos += length
        else:
            print(f"[{pos - 5:#08x}] {name} len={length}")
            if hexdump:
                chunk = data[pos:pos + min(length, 64)]
                print(f"    {chunk.hex(' ')}{' ...' if length > 64 else ''}")
            pos += length
    print(f"-- end at {pos:#x} / file size {n:#x}")


def main():
    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    if not args:
        print(__doc__)
        sys.exit(1)
    data = open(args[0], "rb").read()
    print(f"== {args[0]} ({len(data)} bytes)")
    parse(data, hexdump="--hex" in sys.argv)


if __name__ == "__main__":
    main()
