using System;
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
        private static readonly Dictionary<string, AssetBundle> _bundles =
            new Dictionary<string, AssetBundle>();

        // backward-compat alias used by old code
        private static Dictionary<string, AssetBundle> Bundles => _bundles;

        /// <summary>
        /// Absolute path to the directory where bundle files are stored.
        /// Created on first access if it does not exist.
        /// </summary>
        public static string BundlesFolder
        {
            get
            {
                string dir = Path.Combine(BepInEx.Paths.PluginPath,
                    Path.Combine("E2EMapEditor", "bundles"));
                try
                {
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }
                catch (Exception e)
                {
                    Log.Warn("CustomAssets: could not create bundles folder: " + e.Message);
                }
                return dir;
            }
        }

        /// <summary>
        /// List bundle filenames (name only, no path) found in
        /// <see cref="BundlesFolder"/>. Returns an empty array if the folder
        /// does not exist or is empty.
        /// </summary>
        public static string[] ListBundles()
        {
            string dir = BundlesFolder;
            if (!Directory.Exists(dir))
            {
                return new string[0];
            }
            var results = new List<string>();
            foreach (string path in Directory.GetFiles(dir))
            {
                // skip manifest files and any file starting with '.'
                string name = Path.GetFileName(path);
                if (name.StartsWith(".") || name.EndsWith(".manifest") ||
                    name.EndsWith(".meta"))
                {
                    continue;
                }
                results.Add(name);
            }
            results.Sort(StringComparer.OrdinalIgnoreCase);
            return results.ToArray();
        }

        /// <summary>
        /// List all asset names inside a bundle file (given as an absolute path
        /// or a filename relative to <see cref="BundlesFolder"/>).
        /// Returns an empty array if the bundle cannot be opened.
        /// </summary>
        public static string[] ListAssets(string bundlePathOrName)
        {
            string fullPath = Path.IsPathRooted(bundlePathOrName)
                ? bundlePathOrName
                : Path.Combine(BundlesFolder, bundlePathOrName);
            var bundle = LoadBundle(fullPath);
            if (bundle == null)
            {
                return new string[0];
            }
            var names = bundle.GetAllAssetNames();
            var results = new List<string>(names.Length);
            foreach (string n in names)
            {
                // Unity stores names with a "assets/..." prefix; strip it for display
                string display = n;
                int slash = display.LastIndexOf('/');
                if (slash >= 0)
                {
                    display = display.Substring(slash + 1);
                }
                // strip extension for cleaner display
                int dot = display.LastIndexOf('.');
                if (dot > 0)
                {
                    display = display.Substring(0, dot);
                }
                if (!results.Contains(display))
                {
                    results.Add(display);
                }
            }
            results.Sort(StringComparer.OrdinalIgnoreCase);
            return results.ToArray();
        }

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
        public static T Load<T>(string bundlePath, string assetName) where T : UnityEngine.Object
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
            var instance = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
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
