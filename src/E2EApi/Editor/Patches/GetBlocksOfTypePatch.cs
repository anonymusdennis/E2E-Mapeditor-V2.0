using System.Collections.Generic;
using HarmonyLib;

namespace E2EApi.Editor.Patches
{
    /// <summary>
    /// Re-implementation of <c>BuildingBlockManager.GetBlocksOfType</c> with the
    /// dev-mode gates exposed (vanilla compiles them to a hardcoded
    /// <c>flag = false</c>). Only takes over when an unlock is active, otherwise
    /// the original runs untouched.
    /// </summary>
    [HarmonyPatch(typeof(BuildingBlockManager), nameof(BuildingBlockManager.GetBlocksOfType))]
    internal static class GetBlocksOfTypePatch
    {
        private static bool Prefix(
            BuildingBlockManager __instance,
            ref int __result,
            ref List<int> blockList,
            BaseBuildingBlock.BuildingBlockType blockType,
            BaseLevelManager.LevelLayers layer,
            BaseLevelManager.LayersEnvironment environment,
            BaseBuildingBlock.BlockSet filterTheme,
            BaseBuildingBlock.PurposeGroups filterPurpose,
            long iFamily,
            bool automatic,
            BaseBuildingBlock.CompletionState validity,
            bool bOnlySelectable)
        {
            if (!Blocks.AnyUnlockActive)
            {
                return true; // run the original
            }

            bool allLayers = layer == BaseLevelManager.LevelLayers.TOTAL || Blocks.IgnoreLayerRestrictions;
            int layerMask = allLayers
                ? ((environment != 0) ? 178956970 : 89478485)
                : ((environment != 0) ? (1 << ((int)layer * 2 + 1)) : (1 << ((int)layer * 2)));

            int count = 0;
            var all = __instance.m_BuildingBlocks;
            for (int i = all.Length - 1; i >= 0; i--)
            {
                BaseBuildingBlock block = all[i];
                if (block == null)
                {
                    continue;
                }
                if (block.m_EditorOnly && !Blocks.ShowEditorOnly)
                {
                    continue;
                }
                if (block.BlockType != blockType)
                {
                    continue;
                }
                if ((block.m_ValidLayers & layerMask) == 0 && layer != BaseLevelManager.LevelLayers.TOTAL)
                {
                    continue;
                }
                if (!automatic && block.m_AutomaticBlock)
                {
                    continue;
                }
                if (block.m_Variation != -1 && !block.m_VariationSelectable && bOnlySelectable)
                {
                    continue;
                }
                if ((block.m_OurBlockSets & filterTheme) == 0)
                {
                    continue;
                }
                if (filterPurpose != BaseBuildingBlock.PurposeGroups.ALL && (block.m_BlocksPurpose & filterPurpose) == 0)
                {
                    continue;
                }
                if (iFamily != -1 && (block.m_Family & iFamily) == 0)
                {
                    continue;
                }
                if ((int)block.GetBlockCompletionState() > (int)validity && !Blocks.IgnoreCompletionState)
                {
                    continue;
                }
                count++;
                blockList.Add(i);
            }

            BuildingBlockManager.SortBlockList(ref blockList);
            __result = count;
            return false; // skip the original
        }
    }
}
