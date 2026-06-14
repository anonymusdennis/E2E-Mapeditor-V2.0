using System;
using System.Collections.Generic;
using HarmonyLib;

namespace E2EApi
{
    /// <summary>
    /// Central Harmony patch registry for the API. All API-owned patches go
    /// through here so there is exactly one Harmony instance and one place to
    /// inspect what the API has touched (conflict safety).
    /// </summary>
    public static class PatchRegistry
    {
        private static Harmony _harmony;
        private static readonly List<Type> Applied = new List<Type>();

        public static Harmony Harmony
        {
            get
            {
                if (_harmony == null)
                {
                    _harmony = new Harmony(E2EApiInfo.Guid);
                }
                return _harmony;
            }
        }

        /// <summary>Apply a [HarmonyPatch] class once; subsequent calls are no-ops.</summary>
        public static void EnsurePatched(Type patchClass)
        {
            if (Applied.Contains(patchClass))
            {
                return;
            }
            try
            {
                Harmony.CreateClassProcessor(patchClass).Patch();
                Applied.Add(patchClass);
                Log.Debug($"patched {patchClass.Name}");
            }
            catch (Exception ex)
            {
                Log.Error($"failed to patch {patchClass.Name}: {ex.Message}");
            }
        }

        /// <summary>Patch classes the API has applied so far (read-only snapshot).</summary>
        public static Type[] AppliedPatches => Applied.ToArray();
    }
}
