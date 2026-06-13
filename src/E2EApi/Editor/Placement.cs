using E2EApi.Features;

namespace E2EApi.Editor
{
    /// <summary>
    /// Programmatic map editing. Routes through
    /// <c>BuildingInstructionManager</c>, the game's ordered edit log, so every
    /// API edit is saved, validated, and undoable exactly like a hand edit.
    /// Only works while the level editor is active.
    /// </summary>
    public static class Placement
    {
        private static BuildingInstructionManager Instructions => BuildingInstructionManager.GetInstance();

        /// <summary>True when the editor (and its instruction log) is available.</summary>
        public static bool IsAvailable => Instructions != null;

        /// <summary>
        /// Place a single block (tile, object, decoration, wall, room or complex)
        /// at grid position (x, y). Vanilla instructions only accept native
        /// 0-119 coordinates; expanded map bounds require sidecar-backed tools.
        /// </summary>
        public static bool PlaceBlock(int blockId, int x, int y, int seed = 0, bool checkLimits = false)
        {
            if (!MapGeometry.IsWithinNativeBounds(x, y))
            {
                return false;
            }
            var mgr = Instructions;
            return mgr != null && mgr.AddBlockOnce(blockId, (sbyte)x, (sbyte)y, seed, bDontRun: false, checkLimits);
        }

        /// <summary>Fill a rectangular area with a block.</summary>
        public static bool PlaceArea(int blockId, int x, int y, int width, int height, int seed = 0)
        {
            if (!MapGeometry.IsWithinNativeBounds(x, y))
            {
                return false;
            }
            var mgr = Instructions;
            return mgr != null && mgr.AddBlockArea(blockId, (sbyte)x, (sbyte)y, (sbyte)width, (sbyte)height, seed);
        }

        /// <summary>Delete whatever occupies (x, y) of the given delete type.</summary>
        public static bool Delete(int x, int y, BuildingInstructionManager.InstructionDeleteElement.DeleteType type)
        {
            if (!MapGeometry.IsWithinNativeBounds(x, y))
            {
                return false;
            }
            var mgr = Instructions;
            if (mgr == null)
            {
                return false;
            }
            mgr.AddDeleteToDeleteList(x, y, type);
            return true;
        }

        /// <summary>Add a rectangle to a zone (creates the zone if needed).</summary>
        public static bool AddToZone(int zoneId, int x, int y, int width, int height)
        {
            var mgr = Instructions;
            return mgr != null && mgr.AddToZone(zoneId, (sbyte)x, (sbyte)y, (sbyte)width, (sbyte)height);
        }

        /// <summary>Issue an editor command (layer change, level settings, ...).</summary>
        public static bool Command(BuildingInstructionManager.CommandsEnum command, int value)
        {
            var mgr = Instructions;
            return mgr != null && mgr.AddCommand(command, value);
        }

        /// <summary>The editor cursor's current tile, or false outside the editor.</summary>
        public static bool GetCursorTile(out int x, out int y)
        {
            x = y = -1;
            var controller = LevelEditor_Controller.GetInstance();
            if (controller == null)
            {
                return false;
            }
            x = controller.m_Block_X_Position;
            y = controller.m_Block_Y_Position;
            return x >= 0 && y >= 0;
        }

        /// <summary>Group subsequent edits into one undo step.</summary>
        public static void BeginUndoGroup()
        {
            var mgr = Instructions;
            if (mgr != null)
            {
                mgr.AddStartUndo();
            }
        }

        public static void EndUndoGroup()
        {
            var mgr = Instructions;
            if (mgr != null)
            {
                mgr.AddEndUndo();
            }
        }
    }
}
