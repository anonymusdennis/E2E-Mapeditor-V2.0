using System.Collections.Generic;
using E2EApi.Editor;
using E2EApi.Persistence;
using UnityEngine;

namespace E2EApi.Features
{
    /// <summary>
    /// Mod-tile collision: a registry of blocked tiles (per virtual layer) that
    /// local players cannot walk into during play mode. Painted by connected
    /// brushes with "collide with players" enabled. Persisted in the sidecar
    /// (section [collision], lines "x,y,layer").
    /// </summary>
    public static class ModCollision
    {
        private const string SectionName = "collision";

        private static readonly Dictionary<long, bool> Blocked = new Dictionary<long, bool>();
        private static readonly Dictionary<int, Vector3> _lastSafe = new Dictionary<int, Vector3>();
        private static bool _initialised;

        /// <summary>Bumped on every change (for overlays/UI).</summary>
        public static int Version { get; private set; }

        public static int Count => Blocked.Count;

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
            ApiRunner.Ensure();
        }

        public static bool IsBlocked(int x, int y, int layer)
        {
            return Blocked.ContainsKey(Key(x, y, layer));
        }

        public static void SetBlocked(int x, int y, int layer, bool blocked)
        {
            Initialise();
            long key = Key(x, y, layer);
            if (blocked)
            {
                if (!Blocked.ContainsKey(key))
                {
                    Blocked[key] = true;
                    Version++;
                }
            }
            else if (Blocked.Remove(key))
            {
                Version++;
            }
        }

        public static void Clear()
        {
            if (Blocked.Count > 0)
            {
                Blocked.Clear();
                Version++;
            }
            _lastSafe.Clear();
        }

        /// <summary>
        /// Per-frame play-mode tick: local players standing inside a blocked
        /// tile are pushed back to their last safe position, making blocked
        /// tiles behave like solid walls.
        /// </summary>
        internal static void Tick()
        {
            if (Blocked.Count == 0 ||
                Events.GameEvents.IsEditorActive ||
                BaseLevelManager.GetInstance() == null)
            {
                return;
            }

            foreach (var player in Players.Player.GetAllLocal())
            {
                if (player == null || !player.IsValid || player.Pawn == null)
                {
                    continue;
                }
                var pawn = player.Pawn;
                int x, y, floor, virtualLayer;
                if (!player.GetTile(out x, out y, out floor, out virtualLayer))
                {
                    continue;
                }
                int id = pawn.GetInstanceID();
                if (IsBlockedForPlayer(x, y, virtualLayer))
                {
                    Vector3 safe;
                    if (_lastSafe.TryGetValue(id, out safe))
                    {
                        pawn.transform.position = safe;
                    }
                }
                else
                {
                    _lastSafe[id] = pawn.transform.position;
                }
            }
        }

        private static bool IsBlockedForPlayer(int x, int y, int virtualLayer)
        {
            if (virtualLayer >= 0)
            {
                return IsBlocked(x, y, virtualLayer);
            }
            // No virtual layer info (vanilla map): match any layer at that tile.
            foreach (var key in Blocked.Keys)
            {
                if (UnpackX(key) == x && UnpackY(key) == y)
                {
                    return true;
                }
            }
            return false;
        }

        private static long Key(int x, int y, int layer)
        {
            // 21 bits per component, offset to keep negatives sortable.
            return ((long)(layer & 0x1FFFFF) << 42) |
                   ((long)(x & 0x1FFFFF) << 21) |
                   (uint)(y & 0x1FFFFF);
        }

        private static int UnpackX(long key)
        {
            int v = (int)((key >> 21) & 0x1FFFFF);
            return (v << 11) >> 11; // sign-extend 21-bit value
        }

        private static int UnpackY(long key)
        {
            int v = (int)(key & 0x1FFFFF);
            return (v << 11) >> 11;
        }

        private static int UnpackLayer(long key)
        {
            int v = (int)((key >> 42) & 0x1FFFFF);
            return (v << 11) >> 11;
        }

        private static void OnSaving(ModExtras extras)
        {
            extras.ClearSection(SectionName);
            if (Blocked.Count == 0)
            {
                return;
            }
            var section = extras.Section(SectionName);
            foreach (var key in Blocked.Keys)
            {
                section.Add(UnpackX(key) + "," + UnpackY(key) + "," + UnpackLayer(key));
            }
            extras.RequiresMod = true;
        }

        private static void OnLoaded(ModExtras extras)
        {
            Blocked.Clear();
            _lastSafe.Clear();
            foreach (var line in extras.Section(SectionName))
            {
                string[] parts = line.Split(',');
                int x, y, layer;
                if (parts.Length >= 3 &&
                    int.TryParse(parts[0], out x) &&
                    int.TryParse(parts[1], out y) &&
                    int.TryParse(parts[2], out layer))
                {
                    Blocked[Key(x, y, layer)] = true;
                }
            }
            Version++;
            if (Blocked.Count > 0)
            {
                Log.Info($"mod collision: {Blocked.Count} blocked tile(s)");
            }
        }
    }
}
