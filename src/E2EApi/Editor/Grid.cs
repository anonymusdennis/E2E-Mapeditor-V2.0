using UnityEngine;

namespace E2EApi.Editor
{
    /// <summary>Tile-grid coordinate helpers (120×120 grid, 6 layers).</summary>
    public static class Grid
    {
        public const int Width = 120;
        public const int Height = 120;
        public const int LayerCount = 6;

        /// <summary>
        /// Building layer currently selected in the level editor
        /// (0=Underground … 5=Roof), or -1 outside the editor.
        /// </summary>
        public static int CurrentEditorLayer
        {
            get
            {
                if (!Events.GameEvents.IsEditorActive)
                {
                    return -1; // play-testing or outside the editor
                }
                var editor = EditorLevelEditorManager.GetLevelEditorInstance();
                return editor != null ? (int)editor.m_CurrentLayer : -1;
            }
        }

        /// <summary>World-space centre of tile (x, y) on a layer, or null outside a level.</summary>
        public static Vector3? TileToWorld(int x, int y, int layer = 1)
        {
            // level editor: the play-mode tile systems don't exist; tiles live
            // under the highlight manager's level base at local (x-59.5, y-59.5)
            // (same formula the vanilla brush uses, local z -50 = above the map)
            var levelBase = EditorLevelBase();
            if (levelBase != null)
            {
                return levelBase.TransformPoint(new Vector3(x - 59.5f, y - 59.5f, -50f));
            }

            var manager = BaseLevelManager.GetInstance();
            if (manager == null || manager.m_BuildingLayers == null ||
                layer < 0 || layer >= manager.m_BuildingLayers.Length)
            {
                return null;
            }
            var tiles = manager.m_BuildingLayers[layer].m_Tiles_TileSystem;
            if (tiles == null)
            {
                return null;
            }
            return tiles.WorldPositionFromTileIndex(y, x);
        }

        private static Transform EditorLevelBase()
        {
            // during a play-test the play scene's tile systems are authoritative
            if (!Events.GameEvents.IsEditorActive)
            {
                return null;
            }
            var controller = LevelEditor_Controller.GetInstance();
            var highlight = controller != null ? controller.m_HighlightManager : null;
            return highlight != null && highlight.m_LevelBase != null
                ? highlight.m_LevelBase.transform : null;
        }

        /// <summary>Tile coordinates containing a world position, or null.</summary>
        public static bool WorldToTile(Vector3 world, out int x, out int y, int layer = 1)
        {
            x = y = -1;
            var levelBase = EditorLevelBase();
            if (levelBase != null)
            {
                Vector3 local = levelBase.InverseTransformPoint(world);
                x = Mathf.FloorToInt(local.x + 60f);
                y = Mathf.FloorToInt(local.y + 60f);
                return x >= 0 && x < Width && y >= 0 && y < Height;
            }
            var manager = BaseLevelManager.GetInstance();
            if (manager == null || manager.m_BuildingLayers == null ||
                layer < 0 || layer >= manager.m_BuildingLayers.Length)
            {
                return false;
            }
            var tiles = manager.m_BuildingLayers[layer].m_Tiles_TileSystem;
            if (tiles == null)
            {
                return false;
            }
            Vector3 origin = tiles.WorldPositionFromTileIndex(0, 0, center: false);
            Vector3 cell = tiles.CellSize;
            if (cell.x <= 0f || cell.y <= 0f)
            {
                return false;
            }
            x = Mathf.FloorToInt((world.x - origin.x) / cell.x);
            y = Mathf.FloorToInt((world.y - origin.y) / cell.y);
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }
    }
}
