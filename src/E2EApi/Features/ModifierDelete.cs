using E2EApi.Editor;
using HarmonyLib;
using UnityEngine;

namespace E2EApi.Features
{
    /// <summary>
    /// Modifier keys for the editor's right-click delete:
    /// <list type="bullet">
    /// <item><b>Alt</b> — vanilla delete also wipes mod decorations / custom tiles
    ///   (and custom asset placements) in the deleted area.</item>
    /// <item><b>Shift</b> — delete only custom / mod tiles; skip the vanilla erase.</item>
    /// </list>
    /// When both are held, Shift wins (custom-tiles-only).
    /// </summary>
    public static class ModifierDelete
    {
        private static bool _initialised;

        public static bool ShiftHeld
        {
            get
            {
                return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            }
        }

        public static bool AltHeld
        {
            get
            {
                return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            }
        }

        public static void Initialise()
        {
            if (_initialised)
            {
                return;
            }
            _initialised = true;
            PatchRegistry.EnsurePatched(typeof(DeleteAreaPatch));
        }

        /// <summary>
        /// Erase every mod-placed custom tile / decor / animated stamp / custom
        /// asset at the tile. Returns how many placements were removed.
        /// </summary>
        public static int EraseCustomAt(int x, int y, int layer)
        {
            int removed = 0;
            removed += ModTiles.EraseAt(x, y, layer);
            removed += AnimatedModTiles.EraseAt(x, y, layer);
            removed += CustomAssetPlacements.EraseAt(x, y, layer);
            return removed;
        }

        /// <summary>Erase mod custom content over a rectangle (inclusive size).</summary>
        public static int EraseCustomInArea(int x, int y, int width, int height, int layer)
        {
            if (width <= 0 || height <= 0)
            {
                return 0;
            }
            int removed = 0;
            int x1 = x + width;
            int y1 = y + height;
            for (int py = y; py < y1; py++)
            {
                for (int px = x; px < x1; px++)
                {
                    removed += EraseCustomAt(px, py, layer);
                }
            }
            return removed;
        }

        /// <summary>Also drop vanilla decorations/objects at the tile.</summary>
        public static void EraseVanillaDecorationAt(int x, int y)
        {
            Placement.Delete(x, y,
                BuildingInstructionManager.InstructionDeleteElement.DeleteType.Object);
        }

        private static int CurrentLayer()
        {
            int layer = Grid.CurrentEditorLayer;
            return layer < 0 ? 1 : layer;
        }

        [HarmonyPatch(typeof(BuildingInstructionManager), nameof(BuildingInstructionManager.DeleteArea))]
        private static class DeleteAreaPatch
        {
            [HarmonyPrefix]
            private static bool Prefix(int X, int Y, int iWidth, int iHeight, ref bool __result)
            {
                if (!ShiftHeld)
                {
                    return true;
                }
                int removed = EraseCustomInArea(X, Y, iWidth, iHeight, CurrentLayer());
                __result = removed > 0;
                Log.Debug("Modifier-delete: Shift — custom-only erase " +
                    removed + " in " + iWidth + "x" + iHeight);
                return false;
            }

            [HarmonyPostfix]
            private static void Postfix(int X, int Y, int iWidth, int iHeight)
            {
                if (ShiftHeld || !AltHeld)
                {
                    return;
                }
                int layer = CurrentLayer();
                int removed = EraseCustomInArea(X, Y, iWidth, iHeight, layer);
                // Ensure decorations/objects in the drag are wiped even when
                // vanilla AllowedToDelete skipped them for this layer.
                int x1 = X + iWidth;
                int y1 = Y + iHeight;
                for (int py = Y; py < y1; py++)
                {
                    for (int px = X; px < x1; px++)
                    {
                        EraseVanillaDecorationAt(px, py);
                    }
                }
                Log.Debug("modifier-delete: Alt — also erased " + removed +
                    " custom + decorations in " + iWidth + "x" + iHeight);
            }
        }
    }
}
