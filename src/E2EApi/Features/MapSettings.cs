using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using E2EApi.Events;
using E2EApi.Persistence;
using HarmonyLib;
using UnityEngine;

namespace E2EApi.Features
{
    /// <summary>
    /// Per-map gameplay settings stored in the Level.e2e sidecar ([mapSettings] section).
    /// Settings are applied at play time via Harmony patches and scene sweeps.
    ///
    /// Supported keys (starter set — all optional):
    ///   timeScale=2.0          day-cycle speed multiplier (float &gt; 0)
    ///   startHour=22           in-game hour at map start (int 0–23)
    ///   startMinute=0          in-game minute at map start (int 0–59)
    ///   timedPrison=48h        escape deadline (format: Nh, NhMm, or Nm)
    ///   ambience=Play_Prison_05_Ambience_General   Wwise ambience event name
    ///   spotlightHours=18:30-06:30                 spotlight on/off window
    ///   playerMoney=500        starting money (int)
    ///   healthRegen=0          health restore rate per second (float)
    ///   energyRegen=0.1        energy restore rate (float)
    ///   heatDecay=0.05         heat decay rate (float)
    ///   generatorDowntime=120  generator inactive time in seconds (int)
    ///   cctvSpeed=2.0          CCTV camera sweep speed (float)
    ///   sniperDamage=100       guard-tower damage per shot (int)
    ///   sniperHeatThreshold=40 heat at which snipers open fire (int 0–100)
    ///   startingAlertness=3    initial alertness star rating (int 0–10)
    /// </summary>
    public static class MapSettings
    {
        private const string SectionName = "mapSettings";

        private static readonly Dictionary<string, string> _values =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Snapshot of original PrisonConfig field values so we can restore on level unload.
        private static readonly Dictionary<string, object> _configSnapshot =
            new Dictionary<string, object>();

        private static bool _initialised;

        /// <summary>Fired when any setting is added, changed, or removed.</summary>
        public static event Action Changed;

        // ── Initialisation ────────────────────────────────────────────────────

        public static void Initialise()
        {
            if (_initialised) return;
            _initialised = true;
            ModExtras.EnsurePatched();
            ModExtras.Saving += OnSaving;
            ModExtras.Loaded += OnLoaded;
            PatchRegistry.EnsurePatched(typeof(MapSettingsPatches));
            GameEvents.LevelLoaded += OnLevelLoaded;
            GameEvents.LevelUnloaded += OnLevelUnloaded;
        }

        // ── Public API ────────────────────────────────────────────────────────

        public static bool Has(string key) => _values.ContainsKey(key);

        public static string Get(string key, string @default = null)
        {
            string v;
            return _values.TryGetValue(key, out v) ? v : @default;
        }

        public static float GetFloat(string key, float @default)
        {
            string raw;
            if (!_values.TryGetValue(key, out raw)) return @default;
            float f;
            return float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out f) ? f : @default;
        }

        public static int GetInt(string key, int @default)
        {
            string raw;
            if (!_values.TryGetValue(key, out raw)) return @default;
            int i;
            return int.TryParse(raw, out i) ? i : @default;
        }

        public static void Set(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) return;
            _values[key] = value ?? "";
            try { Changed?.Invoke(); } catch { }
        }

        public static void Unset(string key)
        {
            if (_values.Remove(key))
                try { Changed?.Invoke(); } catch { }
        }

        public static void Clear()
        {
            if (_values.Count == 0) return;
            _values.Clear();
            try { Changed?.Invoke(); } catch { }
        }

        public static IEnumerable<KeyValuePair<string, string>> All() => _values;

        public static int Count => _values.Count;

        // ── Serialization ─────────────────────────────────────────────────────

        private static void OnSaving(ModExtras extras)
        {
            extras.ClearSection(SectionName);
            if (_values.Count == 0) return;
            var lines = extras.Section(SectionName);
            foreach (var pair in _values)
                lines.Add(pair.Key + "=" + pair.Value);
        }

        private static void OnLoaded(ModExtras extras)
        {
            _values.Clear();
            foreach (var line in extras.Section(SectionName))
            {
                int idx = line.IndexOf('=');
                if (idx <= 0) continue;
                _values[line.Substring(0, idx).Trim()] = line.Substring(idx + 1).Trim();
            }
        }

        // ── JSON for web API ──────────────────────────────────────────────────

        public static string ToJson()
        {
            var sb = new StringBuilder();
            sb.Append("{\"settings\":{");
            bool first = true;
            foreach (var pair in _values)
            {
                if (!first) sb.Append(',');
                first = false;
                sb.Append('"').Append(JsonEscape(pair.Key))
                  .Append("\":\"").Append(JsonEscape(pair.Value)).Append('"');
            }
            sb.Append("}}");
            return sb.ToString();
        }

        private static string JsonEscape(string s) =>
            s.Replace("\\", "\\\\").Replace("\"", "\\\"");

        // ── Level events ──────────────────────────────────────────────────────

        private static void OnLevelLoaded()
        {
            if (_values.Count == 0) return;
            try { ApplySceneSweep(); }
            catch (Exception e) { Log.Error($"MapSettings.ApplySceneSweep: {e}"); }
            try { ApplyPrisonConfig(); }
            catch (Exception e) { Log.Error($"MapSettings.ApplyPrisonConfig: {e}"); }
        }

        private static void OnLevelUnloaded()
        {
            try { RestorePrisonConfig(); }
            catch (Exception e) { Log.Error($"MapSettings.RestorePrisonConfig: {e}"); }
        }

        // ── Scene sweep (hardware / single-instance components) ───────────────

        private static void ApplySceneSweep()
        {
            // Generator downtime (seconds a generator stays off after being cut)
            if (Has("generatorDowntime"))
            {
                float v = GetFloat("generatorDowntime", 30f);
                foreach (var gen in Object.FindObjectsOfType<Generator>())
                    Traverse.Create(gen).Field("m_InactiveTime").SetValue(v);
            }

            // CCTV camera sweep speed
            if (Has("cctvSpeed"))
            {
                float v = GetFloat("cctvSpeed", 1f);
                foreach (var cam in Object.FindObjectsOfType<CCTVCamera>())
                    Traverse.Create(cam).Field("m_Speed").SetValue(v);
            }

            // Guard-tower sniper settings
            if (Has("sniperDamage") || Has("sniperHeatThreshold"))
            {
                foreach (var tower in Object.FindObjectsOfType<GuardTowerManager>())
                {
                    if (Has("sniperDamage"))
                        Traverse.Create(tower).Field("m_DamagePerShot").SetValue(GetInt("sniperDamage", 40));
                    if (Has("sniperHeatThreshold"))
                        Traverse.Create(tower).Field("m_ShootingHeatTolerance")
                                .SetValue(GetFloat("sniperHeatThreshold", 70f));
                }
            }

            // Starting alertness star rating
            if (Has("startingAlertness"))
            {
                var am = Object.FindObjectOfType<PrisonAlertnessManager>();
                if (am != null)
                    Traverse.Create(am).Field("m_StartingAlertness")
                            .SetValue(GetInt("startingAlertness", 0));
            }
        }

        // ── PrisonConfig mutation (snapshot + restore) ────────────────────────
        //
        // PrisonConfig is a ScriptableObject asset shared across scene loads;
        // we snapshot the originals and restore them on level unload so that
        // subsequent vanilla maps are not affected by our overrides.

        private static void ApplyPrisonConfig()
        {
            bool hasMoney   = Has("playerMoney");
            bool hasHpRegen = Has("healthRegen");
            bool hasEnRegen = Has("energyRegen");
            bool hasHeatDec = Has("heatDecay");
            if (!hasMoney && !hasHpRegen && !hasEnRegen && !hasHeatDec) return;

            // Access the active PrisonConfig via ConfigManager singleton.
            var cm = ConfigManager.GetInstance();
            if (cm == null) return;

            var config = Traverse.Create(cm).Field("m_ActiveConfig").GetValue();
            if (config == null) return;

            var playerCfg = Traverse.Create(config).Field("m_PlayerConfig").GetValue();
            if (playerCfg == null) return;

            var pcT = Traverse.Create(playerCfg);
            _configSnapshot.Clear();

            SnapshotAndSet(pcT, "m_MoneyBaseLine",     "playerMoney",  hasMoney,   GetFloat("playerMoney",  0f));
            SnapshotAndSet(pcT, "m_HealthRestoreRate", "healthRegen",  hasHpRegen, GetFloat("healthRegen",  0f));
            SnapshotAndSet(pcT, "m_EnergyRestoreRate", "energyRegen",  hasEnRegen, GetFloat("energyRegen",  0f));
            SnapshotAndSet(pcT, "m_HeatDecayRate",     "heatDecay",    hasHeatDec, GetFloat("heatDecay",    0f));
        }

        private static void SnapshotAndSet(Traverse t, string field, string key, bool apply, object newVal)
        {
            if (!apply) return;
            _configSnapshot[key] = t.Field(field).GetValue();
            t.Field(field).SetValue(newVal);
        }

        private static void RestorePrisonConfig()
        {
            if (_configSnapshot.Count == 0) return;

            var cm = ConfigManager.GetInstance();
            if (cm == null) { _configSnapshot.Clear(); return; }

            var config = Traverse.Create(cm).Field("m_ActiveConfig").GetValue();
            if (config == null) { _configSnapshot.Clear(); return; }

            var playerCfg = Traverse.Create(config).Field("m_PlayerConfig").GetValue();
            if (playerCfg != null)
            {
                var pcT = Traverse.Create(playerCfg);
                RestoreField(pcT, "m_MoneyBaseLine",     "playerMoney");
                RestoreField(pcT, "m_HealthRestoreRate", "healthRegen");
                RestoreField(pcT, "m_EnergyRestoreRate", "energyRegen");
                RestoreField(pcT, "m_HeatDecayRate",     "heatDecay");
            }
            _configSnapshot.Clear();
        }

        private static void RestoreField(Traverse t, string field, string key)
        {
            object orig;
            if (_configSnapshot.TryGetValue(key, out orig))
                t.Field(field).SetValue(orig);
        }

        // ── Harmony patches ───────────────────────────────────────────────────

        [HarmonyPatch]
        private static class MapSettingsPatches
        {
            /// <summary>
            /// Multiplies every tick's game-speed by the timeScale setting.
            /// Patching this method catches every consumer of game speed including
            /// fast-forward.
            /// </summary>
            [HarmonyPatch(typeof(RoutineManager), "GetCurrentGameSecondsPerRealSecond")]
            [HarmonyPostfix]
            static void TimeScale_Postfix(ref float __result)
            {
                if (!_values.ContainsKey("timeScale")) return;
                float scale;
                if (float.TryParse(_values["timeScale"], NumberStyles.Float,
                        CultureInfo.InvariantCulture, out scale) && scale > 0f)
                    __result *= scale;
            }

            /// <summary>
            /// Applied after LevelSetup_RoutineManager.Setup() so the values are
            /// in place before RoutineManager.StartInit() reads them.
            /// Applies: startHour/Minute, timedPrison, ambience, spotlightHours.
            /// </summary>
            [HarmonyPatch(typeof(LevelSetup_RoutineManager), "Setup")]
            [HarmonyPostfix]
            static void RoutineSetup_Postfix()
            {
                if (_values.Count == 0) return;

                var rm = Object.FindObjectOfType<RoutineManager>();
                if (rm == null) return;

                var rmT = Traverse.Create(rm);

                // startHour / startMinute
                if (_values.ContainsKey("startHour") || _values.ContainsKey("startMinute"))
                {
                    var rdT = rmT.Field("m_RoutinesData");
                    if (_values.ContainsKey("startHour"))
                        rdT.Field("m_StartOfTheDayHour")
                           .SetValue(GetInt("startHour", 8));
                    if (_values.ContainsKey("startMinute"))
                        rdT.Field("m_StartOfTheDayMinutes")
                           .SetValue(GetInt("startMinute", 0));
                }

                // timedPrison (e.g. "48h", "2h30m", "90m")
                if (_values.ContainsKey("timedPrison"))
                {
                    int hrs, mins;
                    ParseTimedPrison(Get("timedPrison"), out hrs, out mins);
                    if (hrs > 0 || mins > 0)
                    {
                        var rdT = rmT.Field("m_RoutinesData");
                        rdT.Field("m_bIsTimedPrison").SetValue(true);
                        rdT.Field("m_TimedHoursDuration").SetValue(hrs);
                        rdT.Field("m_TimedMinutesDuration").SetValue(mins);
                    }
                }

                // ambience Wwise event name (e.g. "Play_Prison_05_Ambience_General")
                if (_values.ContainsKey("ambience"))
                {
                    try
                    {
                        object parsed = Enum.Parse(
                            typeof(AUTOGEN_T17Wwise_Enums.Events),
                            Get("ambience"),
                            ignoreCase: true);
                        rmT.Field("m_RoutinesData")
                           .Field("m_Ambience")
                           .SetValue(parsed);
                    }
                    catch
                    {
                        Log.Warn($"MapSettings: unknown ambience event '{Get("ambience")}' — skipped");
                    }
                }

                // spotlightHours (e.g. "18:30-06:30")
                if (_values.ContainsKey("spotlightHours"))
                {
                    int sh, sm, eh, em;
                    if (ParseTimeRange(Get("spotlightHours"), out sh, out sm, out eh, out em))
                    {
                        var rdT = rmT.Field("m_RoutinesData");
                        rdT.Field("m_SpotlightsStartHour").SetValue(sh);
                        rdT.Field("m_SpotlightsStartMinutes").SetValue(sm);
                        rdT.Field("m_SpotlightsEndHour").SetValue(eh);
                        rdT.Field("m_SpotlightsEndMinutes").SetValue(em);
                    }
                }
            }
        }

        // ── Parse helpers ─────────────────────────────────────────────────────

        /// <summary>Parse "48h", "2h30m", "90m" → (hours, minutes).</summary>
        private static void ParseTimedPrison(string s, out int hours, out int minutes)
        {
            hours = 0;
            minutes = 0;
            if (string.IsNullOrEmpty(s)) return;
            s = s.Trim().ToLowerInvariant();
            int hi = s.IndexOf('h');
            int mi = s.IndexOf('m');
            if (hi >= 0)
            {
                int.TryParse(s.Substring(0, hi), out hours);
                if (mi > hi + 1)
                    int.TryParse(s.Substring(hi + 1, mi - hi - 1), out minutes);
            }
            else if (mi >= 0)
            {
                int.TryParse(s.Substring(0, mi), out minutes);
            }
        }

        /// <summary>Parse "HH:MM-HH:MM" time range.</summary>
        private static bool ParseTimeRange(string s, out int sh, out int sm, out int eh, out int em)
        {
            sh = sm = eh = em = 0;
            if (string.IsNullOrEmpty(s)) return false;
            string[] parts = s.Split('-');
            if (parts.Length != 2) return false;
            return ParseHHMM(parts[0].Trim(), out sh, out sm)
                && ParseHHMM(parts[1].Trim(), out eh, out em);
        }

        private static bool ParseHHMM(string s, out int h, out int m)
        {
            h = m = 0;
            string[] parts = s.Split(':');
            if (parts.Length != 2) return false;
            return int.TryParse(parts[0], out h) && int.TryParse(parts[1], out m);
        }
    }
}
