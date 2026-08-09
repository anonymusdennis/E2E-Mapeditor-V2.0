using E2EApi.Features;
using HarmonyLib;

namespace E2EApi.Editor.Patches
{
    [HarmonyPatch(typeof(LevelEditor_UIController), "UpdateLayerChanges")]
    internal static class VirtualLayerListPatchUpdate
    {
        private static void Postfix()
        {
            // Vanilla just re-enabled m_MasterLayers via cutaway — re-apply hide.
            MapGeometry.ApplyLayerVisibility();
            VirtualLayerListUi.Refresh();
        }
    }

    [HarmonyPatch(typeof(LevelEditor_UIController), "OnLayerButtonClicked")]
    internal static class VirtualLayerListPatchClick
    {
        private static bool Prefix(int buttonIndex)
        {
            if (!VirtualLayerListUi.IsActive)
            {
                return true;
            }
            VirtualLayerListUi.SelectLayer(buttonIndex);
            return false;
        }
    }

    [HarmonyPatch(typeof(LevelEditor_Controller), "IncLayer")]
    internal static class VirtualLayerListPatchInc
    {
        private static bool Prefix()
        {
            MapGeometry.MoveSelected(1);
            VirtualLayerListUi.Refresh();
            var ui = LevelEditor_UIController.GetInstance();
            if (ui != null)
            {
                ui.ExternalChangeLayer(
                    MapGeometry.ToNativeUiLayer(MapGeometry.GetBackingLayer(
                        MapGeometry.SelectedVirtualLayerIndex)));
                ui.UpdateLayerChanges();
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(LevelEditor_Controller), "DecLayer")]
    internal static class VirtualLayerListPatchDec
    {
        private static bool Prefix()
        {
            MapGeometry.MoveSelected(-1);
            VirtualLayerListUi.Refresh();
            var ui = LevelEditor_UIController.GetInstance();
            if (ui != null)
            {
                ui.ExternalChangeLayer(
                    MapGeometry.ToNativeUiLayer(MapGeometry.GetBackingLayer(
                        MapGeometry.SelectedVirtualLayerIndex)));
                ui.UpdateLayerChanges();
            }
            return false;
        }
    }

    internal static class VirtualLayerListPatch
    {
        internal static void EnsurePatched()
        {
            PatchRegistry.EnsurePatched(typeof(VirtualLayerListPatchUpdate));
            PatchRegistry.EnsurePatched(typeof(VirtualLayerListPatchClick));
            PatchRegistry.EnsurePatched(typeof(VirtualLayerListPatchInc));
            PatchRegistry.EnsurePatched(typeof(VirtualLayerListPatchDec));
        }
    }
}
