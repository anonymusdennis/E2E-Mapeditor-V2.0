#!/bin/bash
# Set Steam Launch Options for The Escapists 2 (641990) so Proton loads
# Doorstop's winhttp.dll (BepInEx). Quit Steam first so localconfig.vdf is not
# overwritten when Steam exits; then run this script and start Steam again.
set -euo pipefail

APP_ID=641990
LAUNCH='WINEDLLOVERRIDES="winhttp=n,b" %command%'
# Stored in VDF with escaped inner quotes:
LAUNCH_VDF='WINEDLLOVERRIDES=\"winhttp=n,b\" %command%'

STEAM_USERDATA="${STEAM_USERDATA:-$HOME/.local/share/Steam/userdata}"

if pgrep -x steam >/dev/null 2>&1 || pgrep -f '[Ss]team.*-srt-logger' >/dev/null 2>&1; then
  echo "WARNING: Steam appears to be running. Quit Steam completely, then re-run this script," >&2
  echo "or set Launch Options manually in the Steam client for app $APP_ID to:" >&2
  echo "  $LAUNCH" >&2
fi

found=""
for vdf in "$STEAM_USERDATA"/*/config/localconfig.vdf; do
  [ -f "$vdf" ] || continue
  if grep -q "\"$APP_ID\"" "$vdf" && grep -q '"Playtime"' "$vdf"; then
    # Confirm this file has the Apps Playtime block for APP_ID
    if python3 - "$vdf" "$APP_ID" "$LAUNCH_VDF" <<'PY'
import re, sys, shutil, time
from pathlib import Path
vdf = Path(sys.argv[1])
app_id = sys.argv[2]
launch_vdf = sys.argv[3]
text = vdf.read_text(encoding="utf-8")
# Prefer Apps block with Playtime + LaunchOptions
pat = re.compile(
    rf'("{app_id}"\s*\{{[\s\S]*?"Playtime"\s*"\d+"[\s\S]*?"LaunchOptions"\s*")([^"]*)(")',
    re.MULTILINE,
)
m = pat.search(text)
if not m:
    # Insert LaunchOptions after Playtime line if missing
    insert_pat = re.compile(
        rf'("{app_id}"\s*\{{[\s\S]*?"Playtime"\s*"\d+")(\s*)',
        re.MULTILINE,
    )
    m2 = insert_pat.search(text)
    if not m2 or "LaunchOptions" in text[m2.start():m2.start()+400]:
        sys.exit(2)
    new_text = text[:m2.end(1)] + f'\n\t\t\t\t\t\t"LaunchOptions"\t\t"{launch_vdf}"' + text[m2.end(1):]
else:
    new_text = text[:m.start()] + m.group(1) + launch_vdf + m.group(3) + text[m.end():]
bak = vdf.with_suffix(vdf.suffix + f".bak-e2emap-{time.strftime('%Y%m%d%H%M%S')}")
shutil.copy2(vdf, bak)
vdf.write_text(new_text, encoding="utf-8")
print(f"Updated: {vdf}")
print(f"Backup:  {bak}")
print(f'LaunchOptions => {launch_vdf.replace(chr(92)+chr(34), chr(34))}')
PY
    then
      found=1
      break
    fi
  fi
done

if [ -z "$found" ]; then
  echo "Could not find localconfig.vdf Apps/$APP_ID with Playtime." >&2
  echo "Set Steam Launch Options manually to:" >&2
  echo "  $LAUNCH" >&2
  exit 1
fi

echo "Done. If Steam was open, fully quit and re-run this script, then start Steam."
echo "Manual fallback (Steam → Properties → Launch Options):"
echo "  $LAUNCH"
