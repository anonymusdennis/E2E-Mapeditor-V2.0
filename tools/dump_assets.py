#!/usr/bin/env python3
"""Dump editor-relevant assets from The Escapists 2 with UnityPy.

Extracts Texture2D/Sprite/Material assets whose names or container paths look
level-editor related (block icons, leveleditor textures) into reference/assets/.

Run with: ~/tools/unitypy-venv/bin/python tools/dump_assets.py [--all-names]
"""
import os
import re
import sys

import UnityPy

GAME_DATA = os.path.expanduser(
    "~/.local/share/Steam/steamapps/common/The Escapists 2/TheEscapists2_Data"
)
OUT = os.path.join(os.path.dirname(__file__), "..", "reference", "assets")

EDITOR_RE = re.compile(
    r"leveleditor|foundation|buildingblock|level_editor|editoricon", re.I
)


def safe(name):
    return re.sub(r"[^\w.-]", "_", name)[:120] or "unnamed"


def main():
    os.makedirs(OUT, exist_ok=True)
    dump_all = "--all-names" in sys.argv
    names_log = open(os.path.join(OUT, "all-texture-names.txt"), "w")
    n_saved = 0
    files = []
    for root, _dirs, fnames in os.walk(GAME_DATA):
        for f in fnames:
            if f.endswith((".assets", ".resource", ".ress")) or re.fullmatch(
                r"level\d+|sharedassets\d+\.assets|globalgamemanagers", f
            ):
                files.append(os.path.join(root, f))
    files.sort()
    for path in files:
        rel = os.path.relpath(path, GAME_DATA)
        try:
            env = UnityPy.load(path)
        except Exception as e:
            print(f"skip {rel}: {e}")
            continue
        for obj in env.objects:
            if obj.type.name not in ("Texture2D", "Sprite"):
                continue
            try:
                data = obj.read()
            except Exception:
                continue
            name = getattr(data, "m_Name", "") or ""
            container = obj.container or ""
            names_log.write(f"{obj.type.name}\t{name}\t{container}\t{rel}\n")
            if not (EDITOR_RE.search(name) or EDITOR_RE.search(container)):
                continue
            try:
                img = data.image
                sub = os.path.join(OUT, "editor", obj.type.name)
                os.makedirs(sub, exist_ok=True)
                img.save(os.path.join(sub, f"{safe(name)}_{obj.path_id}.png"))
                n_saved += 1
            except Exception:
                pass
    names_log.close()
    print(f"saved {n_saved} editor-related images to {OUT}/editor/")
    print(f"full name index: {OUT}/all-texture-names.txt")


if __name__ == "__main__":
    main()
