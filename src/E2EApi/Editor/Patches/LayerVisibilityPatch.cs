using E2EApi.Features;
using HarmonyLib;

namespace E2EApi.Editor.Patches
{
    /// <summary>
    /// Harmony Postfix on the private <c>LevelEditor_Controller.UpdateLayerChanges</c>.
    /// Vanilla shows every editor master layer at or below the current native layer
    /// (<c>m_MasterLayers[i].SetActive(i &lt;= currentLayer)</c>). With the mod's
    /// floor-isolation policy active we override that so only the selected virtual
    /// floor's backing layer — plus the optional "show floor below/above"
    /// neighbours — stays visible.
    /// </summary>
    [HarmonyPatch]
    internal static class LayerVisibilityPatch
    {
        [HarmonyPatch(typeof(LevelEditor_Controller), "UpdateLayerChanges")]
        [HarmonyPostfix]
        private static void UpdateLayerChanges_Postfix()
        {
            LayerVisibilityPatchGroup.ApplyPolicy();
        }
    }

    // ---- Registration -------------------------------------------------------

    internal static class LayerVisibilityPatchGroup
    {
        private static bool _patched;

        internal static void EnsurePatched()
        {
            if (_patched) return;
            _patched = true;
            PatchRegistry.EnsurePatched(typeof(LayerVisibilityPatch));
        }

        /// <summary>
        /// Applies the current <see cref="MapGeometry"/> visibility policy to the
        /// editor's highlight master layers. With isolation off, restores the
        /// vanilla "at or below current layer" rule so toggling round-trips
        /// cleanly.
        /// </summary>
        internal static void ApplyPolicy()
        {
            if (!E2EApi.Events.GameEvents.IsEditorActive)
            {
                return;
            }
            var highlight = LevelEditorHighLightManager.GetInstance();
            if (highlight == null || highlight.m_MasterLayers == null ||
                highlight.m_MasterLayers.Length < MapGeometry.NativeLayerCount)
            {
                return;
            }

            if (MapGeometry.IsolateSelectedFloor)
            {
                var visible = new bool[MapGeometry.NativeLayerCount];
                MapGeometry.ComputeNativeVisibility(visible);
                for (int i = 0; i < MapGeometry.NativeLayerCount; i++)
                {
                    var layer = highlight.m_MasterLayers[i];
                    if (layer != null && layer.activeSelf != visible[i])
                    {
                        layer.SetActive(visible[i]);
                    }
                }
                return;
            }

            // Isolation off: re-apply the vanilla rule (indices 1..5 at or below
            // the current native layer; underground master layer always active,
            // matching vanilla which never toggles slot 0 here).
            var manager = BaseLevelManager.GetInstance();
            if (manager == null)
            {
                return;
            }
            int current = (int)manager.m_CurrentLayer;
            for (int i = 0; i < MapGeometry.NativeLayerCount; i++)
            {
                var layer = highlight.m_MasterLayers[i];
                if (layer == null)
                {
                    continue;
                }
                bool show = i == 0 || i <= current;
                if (layer.activeSelf != show)
                {
                    layer.SetActive(show);
                }
            }
        }
    }
}
