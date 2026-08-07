using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using E2EApi.Persistence;

namespace MapEditorMod
{
    /// <summary>
    /// Persists painted connected-texture (CTM) cells and the stamp
    /// definitions they reference in the Level.e2e sidecar, so re-loading a
    /// map keeps CTM areas editable (connectivity, colors, locks).
    /// Sections: [ctmstamps] one stamp per line, [ctmcells] one cell per line.
    /// </summary>
    internal static class ConnectedPersistence
    {
        private const string StampSection = "ctmstamps";
        private const string CellSection = "ctmcells";

        private static bool _initialised;

        public static void Initialise()
        {
            if (_initialised)
            {
                return;
            }
            _initialised = true;
            ModExtras.EnsurePatched();
            ModExtras.Saving += OnSaving;
            ModExtras.Loaded += OnLoaded;
        }

        private static void OnSaving(ModExtras extras)
        {
            extras.ClearSection(StampSection);
            extras.ClearSection(CellSection);
            var cells = EditorTools.ConnectedCellMap;
            if (cells.Count == 0)
            {
                return;
            }

            // Assign each distinct stamp object a unique persisted id.
            var stampIds = new Dictionary<ConnectedTileStamp, string>();
            var usedIds = new Dictionary<string, bool>();
            int auto = 0;
            foreach (var pair in cells)
            {
                var stamp = pair.Value.Stamp;
                if (stamp == null || stampIds.ContainsKey(stamp))
                {
                    continue;
                }
                string id = string.IsNullOrEmpty(stamp.Id) ? null : stamp.Id;
                if (id == null || usedIds.ContainsKey(id))
                {
                    do
                    {
                        id = "s" + auto++;
                    }
                    while (usedIds.ContainsKey(id));
                }
                usedIds[id] = true;
                stampIds[stamp] = id;
            }

            var stampLines = extras.Section(StampSection);
            foreach (var pair in stampIds)
            {
                stampLines.Add(SerializeStamp(pair.Key, pair.Value));
            }

            var cellLines = extras.Section(CellSection);
            foreach (var pair in cells)
            {
                int x, y, layer;
                if (!TryParseKey(pair.Key, out x, out y, out layer))
                {
                    continue;
                }
                var cell = pair.Value;
                string stampId;
                if (cell.Stamp == null || !stampIds.TryGetValue(cell.Stamp, out stampId))
                {
                    continue;
                }
                cellLines.Add(string.Format(CultureInfo.InvariantCulture,
                    "{0},{1},{2},{3},{4},{5},{6},{7}",
                    x, y, layer, Escape(stampId), cell.Color,
                    cell.Locked ? 1 : 0, cell.Manual ? 1 : 0, cell.ManualMask));
            }
        }

        private static void OnLoaded(ModExtras extras)
        {
            EditorTools.ClearConnectedCells();

            var stamps = new Dictionary<string, ConnectedTileStamp>();
            foreach (var line in extras.Section(StampSection))
            {
                string id;
                var stamp = DeserializeStamp(line, out id);
                if (stamp != null && id != null && !stamps.ContainsKey(id))
                {
                    stamps[id] = stamp;
                }
            }

            var cells = EditorTools.ConnectedCellMap;
            foreach (var line in extras.Section(CellSection))
            {
                var parts = line.Split(',');
                if (parts.Length < 8)
                {
                    continue;
                }
                int x, y, layer, color, locked, manual, manualMask;
                if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out x) ||
                    !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out y) ||
                    !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out layer) ||
                    !int.TryParse(parts[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out color) ||
                    !int.TryParse(parts[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out locked) ||
                    !int.TryParse(parts[6], NumberStyles.Integer, CultureInfo.InvariantCulture, out manual) ||
                    !int.TryParse(parts[7], NumberStyles.Integer, CultureInfo.InvariantCulture, out manualMask))
                {
                    continue;
                }
                ConnectedTileStamp stamp;
                if (!stamps.TryGetValue(Unescape(parts[3]), out stamp))
                {
                    continue;
                }
                cells[layer + ":" + x + ":" + y] = new ConnectedCell
                {
                    Stamp = stamp,
                    Color = color,
                    Locked = locked != 0,
                    Manual = manual != 0,
                    ManualMask = manualMask,
                };
            }
        }

        private static string SerializeStamp(ConnectedTileStamp stamp, string id)
        {
            var sb = new StringBuilder();
            sb.Append(Escape(id)).Append('|');
            sb.Append(Escape(stamp.Name)).Append('|');
            sb.Append(Escape(stamp.Mode)).Append('|');
            sb.Append(stamp.Decor ? 1 : 0).Append('|');
            sb.Append(stamp.Color).Append('|');
            sb.Append(stamp.Auto ? 1 : 0).Append('|');
            sb.Append(stamp.LockPlaced ? 1 : 0).Append('|');
            sb.Append(stamp.ManualSingle ? 1 : 0).Append('|');
            sb.Append(stamp.ManualMask).Append('|');
            sb.Append(stamp.CollideWithPlayers ? 1 : 0).Append('|');
            sb.Append(stamp.Destructible ? 1 : 0).Append('|');
            sb.Append(stamp.Damaging ? 1 : 0).Append('|');
            bool first = true;
            foreach (var pair in stamp.Variants)
            {
                foreach (var v in pair.Value)
                {
                    if (!first)
                    {
                        sb.Append(';');
                    }
                    first = false;
                    sb.Append(pair.Key).Append(':')
                      .Append(Escape(v.Atlas)).Append(':')
                      .Append(v.X).Append(':').Append(v.Y).Append(':')
                      .Append(v.W).Append(':').Append(v.H).Append(':')
                      .Append(v.Weight);
                }
            }
            return sb.ToString();
        }

        private static ConnectedTileStamp DeserializeStamp(string line, out string id)
        {
            id = null;
            var parts = line.Split('|');
            if (parts.Length < 13)
            {
                return null;
            }
            int decor, color, autoFlag, locked, manualSingle, manualMask,
                collide, destructible, damaging;
            if (!int.TryParse(parts[3], out decor) ||
                !int.TryParse(parts[4], out color) ||
                !int.TryParse(parts[5], out autoFlag) ||
                !int.TryParse(parts[6], out locked) ||
                !int.TryParse(parts[7], out manualSingle) ||
                !int.TryParse(parts[8], out manualMask) ||
                !int.TryParse(parts[9], out collide) ||
                !int.TryParse(parts[10], out destructible) ||
                !int.TryParse(parts[11], out damaging))
            {
                return null;
            }
            id = Unescape(parts[0]);
            var stamp = new ConnectedTileStamp
            {
                Id = id,
                Name = Unescape(parts[1]),
                Mode = Unescape(parts[2]),
                Decor = decor != 0,
                Color = color,
                Auto = autoFlag != 0,
                LockPlaced = locked != 0,
                ManualSingle = manualSingle != 0,
                ManualMask = manualMask,
                CollideWithPlayers = collide != 0,
                Destructible = destructible != 0,
                Damaging = damaging != 0,
            };
            if (parts[12].Length > 0)
            {
                foreach (var entry in parts[12].Split(';'))
                {
                    var f = entry.Split(':');
                    if (f.Length < 7)
                    {
                        continue;
                    }
                    int mask, vx, vy, vw, vh, weight;
                    if (!int.TryParse(f[0], out mask) ||
                        !int.TryParse(f[2], out vx) ||
                        !int.TryParse(f[3], out vy) ||
                        !int.TryParse(f[4], out vw) ||
                        !int.TryParse(f[5], out vh) ||
                        !int.TryParse(f[6], out weight))
                    {
                        continue;
                    }
                    List<ConnectedVariant> list;
                    if (!stamp.Variants.TryGetValue(mask, out list))
                    {
                        list = new List<ConnectedVariant>();
                        stamp.Variants[mask] = list;
                    }
                    list.Add(new ConnectedVariant
                    {
                        Atlas = Unescape(f[1]),
                        X = vx, Y = vy, W = vw, H = vh,
                        Weight = weight,
                    });
                }
            }
            return stamp;
        }

        private static bool TryParseKey(string key, out int x, out int y, out int layer)
        {
            x = y = layer = 0;
            var parts = key.Split(':');
            return parts.Length == 3 &&
                int.TryParse(parts[0], out layer) &&
                int.TryParse(parts[1], out x) &&
                int.TryParse(parts[2], out y);
        }

        /// <summary>Percent-encode delimiters used by the line formats.</summary>
        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }
            var sb = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                if (c == '%' || c == '|' || c == ';' || c == ':' || c == ',' ||
                    c == '\r' || c == '\n')
                {
                    sb.Append('%').Append(((int)c).ToString("X2", CultureInfo.InvariantCulture));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private static string Unescape(string value)
        {
            if (string.IsNullOrEmpty(value) || value.IndexOf('%') < 0)
            {
                return value ?? "";
            }
            var sb = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c == '%' && i + 2 < value.Length)
                {
                    int code;
                    if (int.TryParse(value.Substring(i + 1, 2), NumberStyles.HexNumber,
                        CultureInfo.InvariantCulture, out code))
                    {
                        sb.Append((char)code);
                        i += 2;
                        continue;
                    }
                }
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
