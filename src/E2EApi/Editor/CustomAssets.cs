using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace E2EApi.Editor
{
    /// <summary>
    /// Custom asset support via AssetBundles. Bundles must be built with a
    /// Unity 5.5-compatible pipeline to load in this game.
    /// Default search path: BepInEx/plugins/E2EMapEditor/bundles/.
    /// </summary>
    public static class CustomAssets
    {
        private static readonly Dictionary<string, AssetBundle> Bundles =
            new Dictionary<string, AssetBundle>();

        /// <summary>Load (or fetch the already-loaded) bundle at the given path.</summary>
        public static AssetBundle LoadBundle(string path)
        {
            string key = Path.GetFullPath(path);
            AssetBundle bundle;
            if (Bundles.TryGetValue(key, out bundle) && bundle != null)
            {
                return bundle;
            }
            if (!File.Exists(key))
            {
                Log.Warn($"CustomAssets: bundle not found: {key}");
                return null;
            }
            bundle = AssetBundle.LoadFromFile(key);
            if (bundle == null)
            {
                Log.Error($"CustomAssets: failed to load bundle (wrong Unity version?): {key}");
                return null;
            }
            Bundles[key] = bundle;
            Log.Info($"CustomAssets: loaded bundle {Path.GetFileName(key)}");
            return bundle;
        }

        /// <summary>Load a prefab/asset out of a bundle, or null.</summary>
        public static T Load<T>(string bundlePath, string assetName) where T : Object
        {
            var bundle = LoadBundle(bundlePath);
            return bundle != null ? bundle.LoadAsset<T>(assetName) : null;
        }

        /// <summary>Instantiate a prefab from a bundle at a world position.</summary>
        public static GameObject Spawn(string bundlePath, string assetName, Vector3 position)
        {
            var prefab = Load<GameObject>(bundlePath, assetName);
            if (prefab == null)
            {
                return null;
            }
            var instance = Object.Instantiate(prefab, position, Quaternion.identity);
            return instance;
        }

        public static void UnloadAll(bool destroyLoadedObjects = false)
        {
            foreach (var pair in Bundles)
            {
                if (pair.Value != null)
                {
                    pair.Value.Unload(destroyLoadedObjects);
                }
            }
            Bundles.Clear();
        }
    }
}
