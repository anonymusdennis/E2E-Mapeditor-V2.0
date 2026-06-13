using UnityEngine;

namespace E2EApi.Editor
{
    /// <summary>Game-type-free block kind (mirrors BaseBuildingBlock.BuildingBlockType).</summary>
    public enum BlockKind
    {
        Unknown = 0,
        Tile = 1,
        Wall = 2,
        Decoration = 3,
        Object = 4,
        Complex = 5,
        Room = 6,
    }

    /// <summary>
    /// Snapshot of a placeable block for UI consumption. Carries no game types,
    /// so API consumers never need a game-DLL reference.
    /// </summary>
    public class BlockInfo
    {
        public int Id;
        public string NameId;
        public string DisplayName;
        public BlockKind Kind;
        public bool EditorOnly;
        public bool Automatic;
        public int Variation;
        public bool VariationSelectable;
        /// <summary>Icon material (atlas region via mainTextureOffset/Scale).</summary>
        public Material Icon;

        // ---- researched metadata (for tooltips / X-ray) ----

        /// <summary>The block definition's GameObject name (dev-facing name).</summary>
        public string InternalName;
        /// <summary>The implementing class (BuildingBlock_Object, _Tile, ...).</summary>
        public string ClassName;
        /// <summary>Localized description, when the game ships one.</summary>
        public string Description;
        /// <summary>Name of the prefab this block spawns (reveals its function).</summary>
        public string PrefabName;
        /// <summary>Human-readable list of layers the block may be placed on.</summary>
        public string ValidLayers;
        /// <summary>Prison themes this block belongs to.</summary>
        public string Themes;
        /// <summary>Room purposes (RollCall, Kitchen, ...) the block contributes to.</summary>
        public string Purpose;
        /// <summary>True for invisible zone-painting markers.</summary>
        public bool IsZone;
        /// <summary>Limitation/zone group this block counts towards (e.g. InmateCell).</summary>
        public string LimitGroup;
        /// <summary>Curated research notes explaining what the block is for.</summary>
        public string Notes;
    }
}
