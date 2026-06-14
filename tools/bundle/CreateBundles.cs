// Copy this file into Assets/Editor/ of a Unity 5.5.x project.
// Then use Assets → Build AssetBundles to produce bundles for the mod.
#if UNITY_EDITOR
using UnityEditor;
using System.IO;

public static class CreateBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void Build()
    {
        const string outDir = "Assets/AssetBundles";
        Directory.CreateDirectory(outDir);

        // Choose the build target that matches your game platform:
        //   StandaloneLinux64  — native Linux build (default Steam on Linux)
        //   StandaloneWindows64 — Windows build / Proton is transparent
        BuildPipeline.BuildAssetBundles(
            outDir,
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneLinux64);

        UnityEngine.Debug.Log("[E2E] Bundles written to " +
            Path.GetFullPath(outDir));
    }
}
#endif
