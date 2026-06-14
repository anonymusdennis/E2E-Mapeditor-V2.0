using E2EApi.Features;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace E2EApi.Editor.Patches
{
    /// <summary>
    /// Patches <c>LevelEditor_Controller.UpdateBlockPosition</c> so the cursor
    /// tile clamping respects the virtual map bounds stored in <see cref="MapGeometry"/>
    /// instead of the hardcoded 0–119 native grid.
    ///
    /// This makes the "increase buildable editor area" buttons actually effective:
    /// after changing Width/Height/OriginX/OriginY the cursor and brush can reach
    /// the new tiles. Tiles inside the native 120×120 grid are written to the game
    /// engine; tiles outside are saved as E2E sidecar tiles.
    ///
    /// The sbyte cast in AddBlockOnce limits coordinate values to -128..127, so
    /// the effective per-axis range is clamped to that range as well.
    /// </summary>
    [HarmonyPatch(typeof(LevelEditor_Controller), "UpdateBlockPosition")]
    internal static class MapExpansionPatch
    {
        private const int SbyteMin = -128;
        private const int SbyteMax = 127;
        private const int NativeXMax = 119;
        // Vanilla clamps Y to 117 to stay inside the tile grid (top 2 rows = border).
        private const int NativeYMax = 117;

        private static readonly FieldInfo _fRawMouse =
            typeof(LevelEditor_Controller).GetField("m_RawMouseToScreen",
                BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fBrushX =
            typeof(LevelEditor_Controller).GetField("m_Block_X_Position",
                BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fBrushY =
            typeof(LevelEditor_Controller).GetField("m_Block_Y_Position",
                BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fUpdateBrush =
            typeof(LevelEditor_Controller).GetField("m_bUpdateBrushPosition",
                BindingFlags.NonPublic | BindingFlags.Instance);

        static MapExpansionPatch()
        {
            if (_fRawMouse == null)
                Log.Warn("MapExpansionPatch: field m_RawMouseToScreen not found — map expansion cursor will not work");
            if (_fBrushX == null)
                Log.Warn("MapExpansionPatch: field m_Block_X_Position not found");
            if (_fBrushY == null)
                Log.Warn("MapExpansionPatch: field m_Block_Y_Position not found");
            if (_fUpdateBrush == null)
                Log.Warn("MapExpansionPatch: field m_bUpdateBrushPosition not found");
        }

        private static void Postfix(LevelEditor_Controller __instance)
        {
            if (_fRawMouse == null || _fBrushX == null || _fBrushY == null ||
                _fUpdateBrush == null)
            {
                return;
            }

            // Compute desired bounds from MapGeometry.
            int minX = Mathf.Clamp(MapGeometry.OriginX, SbyteMin, SbyteMax);
            int minY = Mathf.Clamp(MapGeometry.OriginY, SbyteMin, SbyteMax);
            int maxX = Mathf.Clamp(MapGeometry.OriginX + MapGeometry.Width - 1,
                SbyteMin, SbyteMax);
            int maxY = Mathf.Clamp(MapGeometry.OriginY + MapGeometry.Height - 1,
                SbyteMin, SbyteMax);

            // If bounds match the native grid, vanilla is already correct.
            if (minX == 0 && minY == 0 && maxX == NativeXMax && maxY <= NativeYMax)
            {
                return;
            }

            // m_RawMouseToScreen is set by GetMousePosition(), called at the start of
            // UpdateBlockPosition() through GetInputDevice() check — it stores the raw
            // world-space mouse position (before the +60 tile-space offset).
            var rawMouse = (Vector2)_fRawMouse.GetValue(__instance);

            // Replicate vanilla tile-space conversion: world.x += 60 gives tile index.
            float tx = rawMouse.x + 60f;
            float ty = rawMouse.y + 60f;

            int newX = Mathf.Clamp((int)tx, minX, maxX);
            int newY = Mathf.Clamp((int)ty, minY, maxY);

            int oldX = (int)_fBrushX.GetValue(__instance);
            int oldY = (int)_fBrushY.GetValue(__instance);

            if (oldX != newX || oldY != newY)
            {
                _fBrushX.SetValue(__instance, newX);
                _fBrushY.SetValue(__instance, newY);
                _fUpdateBrush.SetValue(__instance, true);
            }
        }
    }

    internal static class MapExpansionPatchRegistrar
    {
        internal static void EnsurePatched()
        {
            PatchRegistry.EnsurePatched(typeof(MapExpansionPatch));
        }
    }
}
