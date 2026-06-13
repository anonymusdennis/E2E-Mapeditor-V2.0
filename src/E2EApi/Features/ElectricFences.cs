using System.Collections.Generic;
using E2EApi.Editor;
using E2EApi.Persistence;
using UnityEngine;

namespace E2EApi.Features
{
    /// <summary>
    /// Electric fences: any tile can be marked "electrified". Electrified tiles
    /// hurt characters standing on them while the map is played. Markings are
    /// persisted in the Level.e2e sidecar (section [fences], lines "x,y,damage").
    /// </summary>
    public static class ElectricFences
    {
        private const string SectionName = "fences";
        private const float TickSeconds = 0.5f;

        /// <summary>Damage applied per half-second of contact.</summary>
        public static float DamagePerTick = 10f;

        private static readonly Dictionary<long, bool> Electrified = new Dictionary<long, bool>();

        /// <summary>Bumped on every change; overlays use it to know when to rebuild.</summary>
        public static int Version { get; private set; }
        private static float _nextTick;
        private static Character[] _characterCache;
        private static float _nextCacheRefresh;
        private static bool _initialised;

        private static long Key(int x, int y) => ((long)x << 32) | (uint)y;

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

        public static void SetElectrified(int x, int y, bool on)
        {
            Initialise();
            if (on)
            {
                Electrified[Key(x, y)] = true;
            }
            else
            {
                Electrified.Remove(Key(x, y));
            }
            Version++;
        }

        public static bool IsElectrified(int x, int y) => Electrified.ContainsKey(Key(x, y));

        public static bool Toggle(int x, int y)
        {
            bool now = !IsElectrified(x, y);
            SetElectrified(x, y, now);
            return now;
        }

        public static int Count => Electrified.Count;

        public static IEnumerable<KeyValuePair<int, int>> All()
        {
            foreach (var key in Electrified.Keys)
            {
                yield return new KeyValuePair<int, int>((int)(key >> 32), (int)(uint)key);
            }
        }

        public static void Clear()
        {
            Electrified.Clear();
            Version++;
        }

        private static void OnSaving(ModExtras extras)
        {
            extras.ClearSection(SectionName);
            if (Electrified.Count == 0)
            {
                return;
            }
            var section = extras.Section(SectionName);
            foreach (var tile in All())
            {
                section.Add($"{tile.Key},{tile.Value},{DamagePerTick}");
            }
            extras.RequiresMod = true;
        }

        private static void OnLoaded(ModExtras extras)
        {
            Electrified.Clear();
            foreach (var line in extras.Section(SectionName))
            {
                string[] parts = line.Split(',');
                int x, y;
                if (parts.Length >= 2 && int.TryParse(parts[0], out x) && int.TryParse(parts[1], out y))
                {
                    Electrified[Key(x, y)] = true;
                }
            }
            Version++;
            if (Electrified.Count > 0)
            {
                Log.Info($"electric fences: {Electrified.Count} tile(s) active");
            }
        }

        /// <summary>Called every frame from the ApiRunner.</summary>
        internal static void Tick()
        {
            if (Electrified.Count == 0 || Time.time < _nextTick)
            {
                return;
            }
            _nextTick = Time.time + TickSeconds;

            // only hurt people while actually playing a level (incl. play-test)
            if (Events.GameEvents.IsEditorActive || BaseLevelManager.GetInstance() == null)
            {
                return;
            }

            if (_characterCache == null || Time.time >= _nextCacheRefresh)
            {
                _characterCache = Object.FindObjectsOfType<Character>();
                _nextCacheRefresh = Time.time + 3f;
            }

            foreach (var character in _characterCache)
            {
                if (character == null || character.m_CharacterStats == null)
                {
                    continue;
                }
                int x, y;
                if (!Grid.WorldToTile(character.transform.position, out x, out y))
                {
                    continue;
                }
                if (IsElectrified(x, y))
                {
                    var stats = character.m_CharacterStats;
                    stats.SetHealth(stats.Health - DamagePerTick);
                }
            }
        }
    }
}
