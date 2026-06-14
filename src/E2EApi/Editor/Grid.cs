using UnityEngine;
using E2EApi.Features;
using Rotorz.Tile;

namespace E2EApi.Editor
{
    /// <summary>Tile-grid coordinate helpers plus virtual geometry access.</summary>
    public static class Grid
    {
        public const int NativeWidth = 120;
        public const int NativeHeight = 120;
        public const int NativeLayerCount = 6;
        public static int Width => MapGeometry.Width;
        public static int Height => MapGeometry.Height;
        public static int LayerCount => MapGeometry.LayerCount;
        public static int OriginX => MapGeometry.OriginX;
        public static int OriginY => MapGeometry.OriginY;

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
                return editor != null ? MapGeometry.SelectedVirtualLayerIndex : -1;
            }
        }

        public static int CurrentNativeEditorLayer
        {
            get
            {
                if (!Events.GameEvents.IsEditorActive)
                {
                    return -1;
                }
                var editor = EditorLevelEditorManager.GetLevelEditorInstance();
                return editor != null ? (int)editor.m_CurrentLayer : -1;
            }
        }

        /// <summary>World-space centre of tile (x, y) on a layer, or null outside a level.</summary>
        public static Vector3? TileToWorld(int x, int y, int layer = 1)
        {
            int nativeLayer = MapGeometry.GetBackingLayer(layer);
            // level editor: the play-mode tile systems don't exist; tiles live
            // under the highlight manager's level base at local (x-59.5, y-59.5)
            // (same formula the vanilla brush uses, local z -50 = above the map)
            var levelBase = EditorLevelBase();
            if (levelBase != null)
            {
                float z = -50f - (layer - nativeLayer) * 0.08f;
                return levelBase.TransformPoint(new Vector3(x - 59.5f, y - 59.5f, z));
            }

            var manager = BaseLevelManager.GetInstance();
            if (manager == null || manager.m_BuildingLayers == null ||
                nativeLayer < 0 || nativeLayer >= manager.m_BuildingLayers.Length)
            {
                return null;
            }
            var tiles = manager.m_BuildingLayers[nativeLayer].m_Tiles_TileSystem;
            if (tiles == null)
            {
                return null;
            }
            // In play mode the Rotorz tile system has row 0 at the TOP of the map
            // (highest world y) and row count-1 at the BOTTOM (lowest world y).
            // Our y convention is the opposite: y=0 is the southernmost tile.
            // Convert: row = (OriginY + Height - 1) - y
            int row = (OriginY + Height - 1) - y;
            var world = tiles.WorldPositionFromTileIndex(row, x);
            world.z -= (layer - nativeLayer) * 0.08f;
            return world;
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
                return x >= OriginX && x < OriginX + Width &&
                    y >= OriginY && y < OriginY + Height;
            }
            var manager = BaseLevelManager.GetInstance();
            int nativeLayer = MapGeometry.GetBackingLayer(layer);
            if (manager == null || manager.m_BuildingLayers == null ||
                nativeLayer < 0 || nativeLayer >= manager.m_BuildingLayers.Length)
            {
                return false;
            }
            var tiles = manager.m_BuildingLayers[nativeLayer].m_Tiles_TileSystem;
            if (tiles == null)
            {
                return false;
            }
            // In play mode the Rotorz tile system has row 0 at the TOP (highest
            // world y). WorldPositionFromTileIndex(0,0,center:false) gives the
            // corner of the top-left cell; subtracting that from world.y (which
            // is positive-upward) and dividing by the cell height gives the row
            // counted downward from the top.  We then convert that Rotorz row to
            // our y convention (0 = southernmost tile).
            Vector3 origin = tiles.WorldPositionFromTileIndex(0, 0, center: false);
            Vector3 cell = tiles.CellSize;
            if (cell.x <= 0f || cell.y <= 0f)
            {
                return false;
            }
            x = Mathf.FloorToInt((world.x - origin.x) / cell.x);
            int rotorzRow = Mathf.FloorToInt((origin.y - world.y) / cell.y);
            y = (OriginY + Height - 1) - rotorzRow;
            return x >= OriginX && x < OriginX + Width &&
                y >= OriginY && y < OriginY + Height;
        }

    }
}
