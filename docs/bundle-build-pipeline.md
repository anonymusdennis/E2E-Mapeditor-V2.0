# Unity 5.5 Asset Bundle Build Pipeline

Custom assets in E2E Map Editor are loaded from AssetBundles at runtime.
The game runs on **Unity 5.5.0p4 / Mono** (the native Linux build), so bundles
must be built with a **Unity 5.5.x editor** — bundles from later Unity versions
will silently fail to load.

## Prerequisites

* **Unity 5.5.x** (any patch) installed. The free/personal edition is fine.
  Download from [Unity Download Archive](https://unity.com/releases/editor/archive).
* Your assets (prefabs, meshes, textures, materials, …) already in a Unity 5.5
  project.

## Quick start

1. Open (or create) a Unity 5.5 project containing your assets.
2. Copy `tools/bundle/CreateBundles.cs` into the project's `Assets/Editor/`
   folder. Unity will compile it automatically.
3. Assign an **AssetBundle label** to each asset you want to export:
   - Select the asset in the Project window.
   - At the bottom of the Inspector, open the **AssetBundle** dropdown and type
     a bundle name (e.g. `myassets`).
4. In the Unity menu bar choose **Assets → Build AssetBundles**.
   The output appears in `Assets/AssetBundles/` (one file per bundle label).
5. Copy the resulting bundle file (no extension or `.bundle`) into:
   ```
   BepInEx/plugins/E2EMapEditor/bundles/
   ```
6. In the mod's web UI → **Tools** tab → **Custom assets** card, click **⟳**
   to refresh the bundle list, then pick your bundle and asset to place.

## Editor script

`tools/bundle/CreateBundles.cs` is a minimal build script:

```csharp
#if UNITY_EDITOR
using UnityEditor;
using System.IO;

public class CreateBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void Build()
    {
        string outDir = "Assets/AssetBundles";
        Directory.CreateDirectory(outDir);
        BuildPipeline.BuildAssetBundles(
            outDir,
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneLinux64);   // or StandaloneWindows64 for Windows
    }
}
#endif
```

> **Note:** The bundle target (`StandaloneLinux64` / `StandaloneWindows64`)
> must match the platform you are running the game on.  On Linux, use
> `StandaloneLinux64`; on Windows, use `StandaloneWindows64`.

## Compatibility notes

| Issue | Solution |
|---|---|
| Bundle loads but prefab is `null` | Prefab references a script that does not exist in the game — remove or stub the script. |
| Bundle fails to load entirely | Built with the wrong Unity version or wrong build target. |
| Textures appear pink/magenta | Shader not present in the game — use one of the built-in Unity shaders (Standard, Unlit, etc.). |

## Sidecar persistence

Placed custom assets are stored in the `Level.e2e` sidecar under
`[custom_assets]`. Each line has the format:

```
bundleName|assetName|x,y,layer[|off:dx,dy,dz][|rot:ry]
```

* `bundleName` — filename only (no path), e.g. `myassets`.
* `assetName` — prefab name as returned by `bundle.GetAllAssetNames()`, with
  directory prefix and extension stripped.
* `x,y,layer` — tile grid coordinates.
* `off:dx,dy,dz` — optional world-space offset from the tile centre (floats).
* `rot:ry` — optional Y-axis rotation in degrees.

Maps that contain custom assets automatically set `requiresMod=true` in the
sidecar so vanilla clients know they cannot fully load the map.
