using System.Collections.Generic;
using UnityEngine;

namespace E2EApi.Editor
{
    /// <summary>
    /// Limitation-group overrides (placement caps, guard/inmate counts).
    /// V0 forced <c>m_LimitationGroups[20/21].m_CurrentTotal = 24</c> every frame
    /// to allow up to 24 inmates/guards; this is the same proven mechanism with
    /// a managed enforcement loop and an arbitrary group/value table.
    /// </summary>
    public static class Limits
    {
        private static readonly Dictionary<int, int> TotalOverrides = new Dictionary<int, int>();

        /// <summary>
        /// Force a limitation group's current total to a fixed value
        /// (re-applied every frame while the manager exists).
        /// </summary>
        public static void OverrideTotal(BuildingBlockManager.DefaultLimitationGroups group, int total)
            => OverrideTotal((int)group, total);

        public static void OverrideTotal(int groupIndex, int total)
        {
            TotalOverrides[groupIndex] = total;
            ApiRunner.Ensure();
        }

        public static void ClearOverride(BuildingBlockManager.DefaultLimitationGroups group)
            => TotalOverrides.Remove((int)group);

        /// <summary>
        /// Force the editor to offer up to <paramref name="count"/> guards and
        /// inmates regardless of placed cells/markers (0 = vanilla behaviour).
        /// The V0 mod's "24 guards/inmates" feature.
        /// </summary>
        public static void SetGuardInmateAvailability(int count)
        {
            if (count > 0)
            {
                OverrideTotal(BuildingBlockManager.DefaultLimitationGroups.Inmate, count);
                OverrideTotal(BuildingBlockManager.DefaultLimitationGroups.Guard, count);
            }
            else
            {
                ClearOverride(BuildingBlockManager.DefaultLimitationGroups.Inmate);
                ClearOverride(BuildingBlockManager.DefaultLimitationGroups.Guard);
            }
        }

        public static void ClearAllOverrides() => TotalOverrides.Clear();

        /// <summary>Look up a group index by its registered name (e.g. "Escape").</summary>
        public static int GetNamedGroupIndex(string name)
        {
            var mgr = BuildingBlockManager.GetInstance();
            return mgr != null ? mgr.GetNamedLimitationIndex(name) : -1;
        }

        internal static void Enforce()
        {
            if (TotalOverrides.Count == 0)
            {
                return;
            }
            var mgr = BuildingBlockManager.GetInstance();
            if (mgr == null)
            {
                return;
            }
            var groups = mgr.m_LimitationGroups;
            if (groups == null || groups.Length == 0)
            {
                return;
            }
            foreach (var pair in TotalOverrides)
            {
                if (pair.Key >= 0 && pair.Key < groups.Length && groups[pair.Key] != null)
                {
                    groups[pair.Key].m_CurrentTotal = pair.Value;
                }
            }
        }
    }
}
