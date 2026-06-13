using System.Collections.Generic;
using E2EApi.Editor.Patches;

namespace E2EApi.Editor
{
    /// <summary>
    /// Spawnlist / building-block API. Wraps <c>BuildingBlockManager</c>.
    /// The unlock properties drive a Harmony patch on
    /// <c>BuildingBlockManager.GetBlocksOfType</c> (the query behind the
    /// vanilla editor spawnlist) — the V0 mod's core feature, now config-driven.
    /// </summary>
    public static class Blocks
    {
        private static bool _showEditorOnly;
        private static bool _ignoreLayerRestrictions;
        private static bool _ignoreCompletionState;

        /// <summary>Show dev-only blocks (<c>m_EditorOnly</c>) in the spawnlist.</summary>
        public static bool ShowEditorOnly
        {
            get => _showEditorOnly;
            set { _showEditorOnly = value; EnsurePatchIfNeeded(); }
        }

        /// <summary>Offer every block on every layer (V0 behaviour).</summary>
        public static bool IgnoreLayerRestrictions
        {
            get => _ignoreLayerRestrictions;
            set { _ignoreLayerRestrictions = value; EnsurePatchIfNeeded(); }
        }

        /// <summary>Also list blocks the game considers incomplete/invalid.</summary>
        public static bool IgnoreCompletionState
        {
            get => _ignoreCompletionState;
            set { _ignoreCompletionState = value; EnsurePatchIfNeeded(); }
        }

        internal static bool AnyUnlockActive =>
            _showEditorOnly || _ignoreLayerRestrictions || _ignoreCompletionState;

        private static void EnsurePatchIfNeeded()
        {
            if (AnyUnlockActive)
            {
                PatchRegistry.EnsurePatched(typeof(GetBlocksOfTypePatch));
            }
        }

        /// <summary>The live block registry (null until a level/editor scene exists).</summary>
        public static BaseBuildingBlock[] All
        {
            get
            {
                var mgr = BuildingBlockManager.GetInstance();
                return mgr != null ? mgr.m_BuildingBlocks : null;
            }
        }

        /// <summary>Look up a block by ID, or null.</summary>
        public static BaseBuildingBlock Get(int blockId) => BuildingBlockManager.GetBlock(blockId);

        /// <summary>True when a block id exists in the live registry.</summary>
        public static bool Exists(int blockId) => Get(blockId) != null;

        /// <summary>
        /// Game-type-free spawnlist snapshot for UI consumption.
        /// Includes editor-only blocks when <see cref="ShowEditorOnly"/> is set.
        /// Returns an empty list outside a level/editor scene.
        /// </summary>
        public static List<BlockInfo> GetSpawnList()
        {
            var result = new List<BlockInfo>();
            var all = All;
            if (all == null)
            {
                return result;
            }
            foreach (var block in all)
            {
                if (block == null)
                {
                    continue;
                }
                if (block.m_EditorOnly && !ShowEditorOnly)
                {
                    continue;
                }
                if (block.m_Variation != -1 && !block.m_VariationSelectable)
                {
                    continue;
                }
                result.Add(Describe(block));
            }
            return result;
        }

        /// <summary>Full researched metadata snapshot for one block.</summary>
        public static BlockInfo Describe(BaseBuildingBlock block)
        {
            string internalName = block.gameObject != null ? block.gameObject.name : null;
            string display = Localize(block.m_BlockNameID);
            if (string.IsNullOrEmpty(display) || display == block.m_BlockNameID ||
                block.m_BlockNameID.StartsWith("Text.") && display.StartsWith("Text."))
            {
                // no usable localization → fall back to the dev-facing object name
                display = CleanInternalName(internalName) ?? block.m_BlockNameID;
            }

            string prefabName = null;
            var single = block as BuildingBlock_Single;
            if (single != null)
            {
                if (single.m_Prefab != null)
                {
                    prefabName = single.m_Prefab.name;
                }
                else if (single.m_VisualPrefab != null)
                {
                    prefabName = single.m_VisualPrefab.name;
                }
            }

            var obj = block as BuildingBlock_Object;
            string limitGroup = null;
            if (block.m_LimitationGroup >= 0)
            {
                var mgr = BuildingBlockManager.GetInstance();
                var group = mgr != null ? mgr.GetTheLimitationGroup(block.m_LimitationGroup) : null;
                if (group != null)
                {
                    limitGroup = group.m_GroupName;
                }
            }
            string description = Localize(block.m_BlockDescriptionID);
            if (description != null && description.StartsWith("Text."))
            {
                description = null; // unlocalized tag, not a real description
            }
            return new BlockInfo
            {
                Id = block.m_ID,
                NameId = block.m_BlockNameID,
                DisplayName = display,
                Kind = (BlockKind)(int)block.BlockType,
                EditorOnly = block.m_EditorOnly,
                Automatic = block.m_AutomaticBlock,
                Variation = block.m_Variation,
                VariationSelectable = block.m_VariationSelectable,
                Icon = block.m_UIImage,
                InternalName = internalName,
                ClassName = block.GetType().Name,
                Description = description,
                PrefabName = prefabName,
                ValidLayers = DecodeLayers(block.m_ValidLayers),
                Themes = block.m_OurBlockSets.ToString(),
                Purpose = block.m_BlocksPurpose != 0 ? block.m_BlocksPurpose.ToString() : null,
                IsZone = obj != null && obj.m_ZoneObject,
                LimitGroup = limitGroup,
                Notes = BlockNotes.For(block, internalName, prefabName, limitGroup, description),
            };
        }

        private static string Localize(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return null;
            }
            string localized;
            if (Localization.Get(tag, out localized) && !string.IsNullOrEmpty(localized))
            {
                return localized;
            }
            return tag;
        }

        /// <summary>"Block_RollCallPos_x1" → "Roll Call Pos x1" etc.</summary>
        internal static string CleanInternalName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            string s = name;
            foreach (var prefix in new[] { "Block_", "BuildingBlock_", "BB_" })
            {
                if (s.StartsWith(prefix))
                {
                    s = s.Substring(prefix.Length);
                }
            }
            s = s.Replace('_', ' ');
            var sb = new System.Text.StringBuilder(s.Length + 8);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (i > 0 && char.IsUpper(c) && char.IsLower(s[i - 1]))
                {
                    sb.Append(' ');
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        private static string DecodeLayers(int mask)
        {
            string[] names = { "Underground", "Ground", "Ground Vent", "First Floor", "First Floor Vent", "Roof" };
            var sb = new System.Text.StringBuilder();
            for (int layer = 0; layer < names.Length; layer++)
            {
                if ((mask & (3 << (layer * 2))) != 0)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(names[layer]);
                }
            }
            return sb.Length > 0 ? sb.ToString() : "none";
        }

        /// <summary>
        /// Make a block the active editor brush, exactly as if clicked in the
        /// vanilla spawnlist. Requires the level editor to be open.
        /// </summary>
        public static bool SelectBrush(int blockId)
        {
            var controller = LevelEditor_Controller.GetInstance();
            if (controller == null)
            {
                return false;
            }
            controller.ExternalSelectBlock(blockId);
            return true;
        }

        /// <summary>Clear the vanilla editor block brush when the editor is open.</summary>
        public static void ClearBrush()
        {
            var controller = LevelEditor_Controller.GetInstance();
            if (controller != null)
            {
                controller.m_CurrentBlock = -1;
            }
        }

        /// <summary>
        /// Delete content at a tile using the delete type that matches the block id's kind.
        /// </summary>
        public static bool DeleteAt(int blockId, int x, int y)
        {
            var block = Get(blockId);
            if (block == null)
            {
                return false;
            }
            switch ((BlockKind)(int)block.BlockType)
            {
                case BlockKind.Wall:
                    return Placement.Delete(x, y,
                        BuildingInstructionManager.InstructionDeleteElement.DeleteType.Wall);
                case BlockKind.Decoration:
                    return Placement.Delete(x, y,
                        BuildingInstructionManager.InstructionDeleteElement.DeleteType.Object);
                case BlockKind.Object:
                case BlockKind.Complex:
                    return Placement.Delete(x, y,
                        BuildingInstructionManager.InstructionDeleteElement.DeleteType.Object);
                case BlockKind.Room:
                    return Placement.Delete(x, y,
                        BuildingInstructionManager.InstructionDeleteElement.DeleteType.Object);
                case BlockKind.Tile:
                    return Placement.Delete(x, y,
                        BuildingInstructionManager.InstructionDeleteElement.DeleteType.Tile);
                default:
                    return false;
            }
        }

        /// <summary>The block id currently on the editor brush (-1 = none/no editor).</summary>
        public static int CurrentBrush
        {
            get
            {
                var controller = LevelEditor_Controller.GetInstance();
                return controller != null ? controller.m_CurrentBlock : -1;
            }
        }

        /// <summary>
        /// Register a new building block at runtime (custom content).
        /// The block must be a fully initialised <c>BaseBuildingBlock</c> component.
        /// </summary>
        public static void Register(BaseBuildingBlock block)
        {
            var mgr = BuildingBlockManager.GetInstance();
            if (mgr == null)
            {
                Log.Warn("Blocks.Register: no BuildingBlockManager yet");
                return;
            }
            mgr.AddNewBuildingBlock(block);
            Log.Info($"registered custom block '{block.m_BlockNameID}' (id {block.m_ID})");
        }
    }
}
