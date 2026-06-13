using System.Text;
using HarmonyLib;

namespace E2EApi.Editor
{
    /// <summary>
    /// Makes every *installed* DLC's map content available in custom
    /// (editor-built) maps.
    ///
    /// How the game gates DLC map content: there is no entitlement check in
    /// the editor block pipeline at all (the spawnlist scene only ships the
    /// CentrePerks/RattleSnake editor block sets). What IS gated per prison
    /// are items and craft recipes: each <c>ItemData</c> / <c>Recipe</c>
    /// carries an "Allowed Prisons" mask (<c>m_PrisonMask</c>) that is matched
    /// against the running level's prison via
    /// <c>LevelScript.IsPrisonEnumInMask</c>. Custom maps only ever match the
    /// PlayerMade bit, so content tagged for DLC prisons (Wicked Ward,
    /// Glorious Regime, Snow Way Out, …) or for specific base-game prisons
    /// (e.g. the transport prisons) never shows up in editor-built maps.
    ///
    /// When <see cref="UnlockInstalled"/> is on, the mask check for custom
    /// maps additionally accepts content of every base-game prison and of
    /// every DLC prison whose DLC the player actually owns
    /// (<c>Platform.IsDLCAvailable</c>, i.e. the Steam entitlement check the
    /// game itself uses for the campaign list). Content of DLC that is NOT
    /// installed stays locked.
    /// </summary>
    public static class DlcContent
    {
        private static bool _unlockInstalled;
        private static uint _extraMask;
        private static float _maskComputedAt = -999f;

        public static bool UnlockInstalled
        {
            get => _unlockInstalled;
            set
            {
                bool wasOn = _unlockInstalled;
                _unlockInstalled = value;
                _maskComputedAt = -999f; // recompute on next check
                if (value)
                {
                    PatchRegistry.EnsurePatched(typeof(DlcMaskPatch));
                    if (!wasOn)
                    {
                        Log.Info("items/recipes of installed DLCs unlocked for custom maps");
                    }
                }
                if (value != wasOn)
                {
                    RefreshAllowedItems();
                }
            }
        }

        /// <summary>
        /// The game builds ItemManager's cached allowed-item pool once per
        /// session, so a mid-session toggle would only apply to recipes
        /// (whose list is rebuilt per level). This syncs the cached pool with
        /// the current mask rules immediately. No-op outside a level.
        /// </summary>
        public static void RefreshAllowedItems()
        {
            var manager = ItemManager.GetInstance();
            var level = LevelScript.GetInstance();
            if (manager == null || level == null || level.m_LevelSetup == null ||
                manager.m_ExistingItemData == null)
            {
                return;
            }
            var prisonEnum = level.m_LevelSetup.m_LevelInfo.m_PrisonEnum;
            var allowed = manager.GetAllowedList();
            int addedCount = 0;
            int removedCount = 0;
            foreach (var item in manager.m_ExistingItemData)
            {
                if (item == null ||
                    item.HasFunctionality(BaseItemFunctionality.Functionality.Key) != null)
                {
                    continue; // key items live in the separate key list
                }
                bool should = LevelScript.IsPrisonEnumInMask(item.m_PrisonMask, prisonEnum);
                if (should && !allowed.Contains(item))
                {
                    allowed.Add(item);
                    addedCount++;
                }
                else if (!should && allowed.Contains(item))
                {
                    allowed.Remove(item);
                    removedCount++;
                }
            }
            if (addedCount > 0 || removedCount > 0)
            {
                Log.Info($"allowed-item pool refreshed (+{addedCount}/-{removedCount})");
            }
        }

        /// <summary>
        /// Prison-mask bits that custom maps may additionally match: all
        /// base-game prisons plus DLC prisons whose DLC is installed.
        /// Cached briefly because the check runs in item/recipe init loops.
        /// </summary>
        internal static uint ExtraMaskForCustom()
        {
            float now = UnityEngine.Time.realtimeSinceStartup;
            if (now - _maskComputedAt > 5f)
            {
                _extraMask = ComputeExtraMask();
                _maskComputedAt = now;
            }
            return _extraMask;
        }

        private static uint ComputeExtraMask()
        {
            var levelData = LevelDataManager.GetInstance();
            var platform = Platform.GetInstance();
            if (levelData == null || levelData.m_T17Levels == null)
            {
                return 0;
            }
            uint mask = 0;
            foreach (var prison in levelData.m_T17Levels)
            {
                if (prison == null || prison.m_bIsDebug || prison.m_LevelInfo == null)
                {
                    continue;
                }
                int bit = LevelScript.GetPrisonBit(prison.m_LevelInfo.m_PrisonEnum);
                if (bit <= 0)
                {
                    continue;
                }
                bool available = !prison.m_bIsDLC ||
                    (prison.m_DLCData != null && platform != null &&
                     platform.IsDLCAvailable(prison.m_DLCData));
                if (available)
                {
                    mask |= (uint)bit;
                }
            }
            return mask;
        }

        /// <summary>
        /// JSON report of every prison the game knows: which are DLC, whether
        /// that DLC is installed (entitlement check), and which mask bits the
        /// unlock currently grants to custom maps.
        /// </summary>
        public static string StatusJson()
        {
            var sb = new StringBuilder();
            sb.Append("{\"unlockEnabled\":").Append(_unlockInstalled ? "true" : "false");
            sb.Append(",\"extraMask\":").Append(ComputeExtraMask());
            sb.Append(",\"prisons\":[");
            var levelData = LevelDataManager.GetInstance();
            var platform = Platform.GetInstance();
            bool first = true;
            if (levelData != null && levelData.m_T17Levels != null)
            {
                foreach (var prison in levelData.m_T17Levels)
                {
                    if (prison == null || prison.m_LevelInfo == null)
                    {
                        continue;
                    }
                    if (!first)
                    {
                        sb.Append(",");
                    }
                    first = false;
                    string name;
                    if (!Localization.Get(prison.m_NameLocalizationKey, out name) ||
                        string.IsNullOrEmpty(name))
                    {
                        name = prison.m_NameLocalizationKey;
                    }
                    bool installed = !prison.m_bIsDLC ||
                        (prison.m_DLCData != null && platform != null &&
                         platform.IsDLCAvailable(prison.m_DLCData));
                    sb.Append("{\"name\":").Append(Quote(name));
                    sb.Append(",\"prison\":").Append(Quote(prison.m_LevelInfo.m_PrisonEnum.ToString()));
                    sb.Append(",\"isDlc\":").Append(prison.m_bIsDLC ? "true" : "false");
                    sb.Append(",\"dlcId\":").Append(Quote(
                        prison.m_DLCData != null ? prison.m_DLCData.m_DLCID : ""));
                    sb.Append(",\"freeDlc\":").Append(
                        prison.m_DLCData != null && prison.m_DLCData.m_bFreeDLC ? "true" : "false");
                    sb.Append(",\"installed\":").Append(installed ? "true" : "false");
                    sb.Append(",\"debug\":").Append(prison.m_bIsDebug ? "true" : "false");
                    sb.Append("}");
                }
            }
            sb.Append("]}");
            return sb.ToString();
        }

        /// <summary>
        /// JSON report of item availability in the current level: how many
        /// item definitions exist, how many a vanilla custom map would allow
        /// (PlayerMade bit only), how many the unlock allows, and the names
        /// of the items the unlock adds. Empty outside a level.
        /// </summary>
        public static string ItemsJson()
        {
            var manager = ItemManager.GetInstance();
            var level = LevelScript.GetInstance();
            if (manager == null || level == null || level.m_LevelSetup == null)
            {
                return "{\"inLevel\":false}";
            }
            var prisonEnum = level.m_LevelSetup.m_LevelInfo.m_PrisonEnum;
            var all = manager.m_ExistingItemData;
            int total = 0;
            int vanillaAllowed = 0;
            int nowAllowed = 0;
            var added = new StringBuilder();
            bool firstAdded = true;
            foreach (var item in all)
            {
                if (item == null)
                {
                    continue;
                }
                total++;
                uint mask = (uint)item.m_PrisonMask;
                bool vanilla = mask == 0xFFFFFFFFu ||
                    (mask & (uint)LevelScript.GetPrisonBit(prisonEnum)) != 0;
                bool now = LevelScript.IsPrisonEnumInMask(item.m_PrisonMask, prisonEnum);
                if (vanilla)
                {
                    vanillaAllowed++;
                }
                if (now)
                {
                    nowAllowed++;
                }
                if (now && !vanilla)
                {
                    if (!firstAdded)
                    {
                        added.Append(",");
                    }
                    firstAdded = false;
                    string name;
                    if (!Localization.Get(item.m_ItemLocalizationTag, out name) ||
                        string.IsNullOrEmpty(name))
                    {
                        name = item.name;
                    }
                    added.Append("{\"id\":").Append(item.m_ItemDataID);
                    added.Append(",\"name\":").Append(Quote(name));
                    added.Append(",\"mask\":").Append(Quote(item.m_PrisonMask.ToString()));
                    added.Append("}");
                }
            }
            var sb = new StringBuilder();
            sb.Append("{\"inLevel\":true");
            sb.Append(",\"prison\":").Append(Quote(prisonEnum.ToString()));
            sb.Append(",\"totalItems\":").Append(total);
            sb.Append(",\"vanillaAllowed\":").Append(vanillaAllowed);
            sb.Append(",\"nowAllowed\":").Append(nowAllowed);
            sb.Append(",\"allowedListCount\":").Append(manager.GetAllowedList().Count);
            AppendRecipes(sb, prisonEnum);
            sb.Append(",\"addedByUnlock\":[").Append(added).Append("]}");
            return sb.ToString();
        }

        /// <summary>Craft-recipe availability (recipes are the main mask-gated
        /// content in play: the live list is rebuilt per level).</summary>
        private static void AppendRecipes(StringBuilder sb, LevelScript.PRISON_ENUM prisonEnum)
        {
            var crafts = CraftManager.GetInstance();
            if (crafts == null)
            {
                return;
            }
            var all = crafts.GetAllRecipes();
            int passing = 0;
            var unlocked = new StringBuilder();
            bool first = true;
            foreach (var recipe in all)
            {
                if (recipe == null)
                {
                    continue;
                }
                bool now = LevelScript.IsPrisonEnumInMask(recipe.m_PrisonMask, prisonEnum);
                uint mask = (uint)recipe.m_PrisonMask;
                bool vanilla = mask == 0xFFFFFFFFu ||
                    (mask & (uint)LevelScript.GetPrisonBit(prisonEnum)) != 0;
                if (now)
                {
                    passing++;
                }
                if (now && !vanilla && recipe.m_Product != null)
                {
                    if (!first)
                    {
                        unlocked.Append(",");
                    }
                    first = false;
                    string name;
                    if (!Localization.Get(recipe.m_Product.m_ItemLocalizationTag, out name) ||
                        string.IsNullOrEmpty(name))
                    {
                        name = recipe.m_Product.name;
                    }
                    unlocked.Append(Quote(name));
                }
            }
            sb.Append(",\"recipesTotal\":").Append(all.Count);
            sb.Append(",\"recipesPassingMask\":").Append(passing);
            sb.Append(",\"recipeProductsAddedByUnlock\":[").Append(unlocked).Append("]");
        }

        private static string Quote(string value)
        {
            if (value == null)
            {
                return "\"\"";
            }
            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        [HarmonyPatch(typeof(LevelScript), nameof(LevelScript.IsPrisonEnumInMask))]
        private static class DlcMaskPatch
        {
            private static void Postfix(ref bool __result,
                LevelScript.PRISON_ENUM_MASK mask, LevelScript.PRISON_ENUM toCheck)
            {
                if (__result || !_unlockInstalled ||
                    toCheck != LevelScript.PRISON_ENUM.CustomPrison)
                {
                    return;
                }
                if (((uint)mask & ExtraMaskForCustom()) != 0)
                {
                    __result = true;
                }
            }
        }
    }
}
