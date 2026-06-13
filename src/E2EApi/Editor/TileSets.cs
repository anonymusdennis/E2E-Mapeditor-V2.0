using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace E2EApi.Editor
{
    /// <summary>
    /// Custom tileset loader: turns installed game art (base + DLC prison
    /// scenes) into extra paintable editor content.
    ///
    /// The game's prison scenes ship as scene AssetBundles
    /// (StreamingAssets/&lt;platform&gt;/AssetBundles/&lt;scene&gt;), so their
    /// textures cannot be loaded by name. The harvester instead loads each
    /// scene additively while the frontend is up, immediately deactivates
    /// every root object (Awake/OnEnable have run, but no Start/Update),
    /// snapshots the newly loaded textures, copies the tile/decor atlases
    /// into a PNG cache on disk and unloads the scene again. Painting later
    /// only ever touches the cache, never the bundles. The cache is built
    /// from the player's own installed files, so no game art is ever
    /// redistributed with the mod.
    ///
    /// One map tile is 32×32 px in atlas art (the baked 120×120 floor
    /// textures are 3840×3840).
    ///
    /// Cache layout: BepInEx/plugins/E2EMapEditor/tilecache/
    ///   atlases/&lt;file&gt;.png   readable copies of harvested atlases
    ///   atlases/index.txt        atlasName|file|width|height
    ///   sets/&lt;setId&gt;.txt    atlas names belonging to the set (browse index)
    ///   sets/&lt;setId&gt;.inventory.json  raw harvest inventory (diagnostics)
    /// </summary>
    public static class TileSets
    {
        /// <summary>Pixels of one map tile in harvested atlas art.</summary>
        public const int TilePixels = 32;

        public static string Status { get; private set; } = "idle";
        public static bool Busy { get; private set; }

        /// <summary>Harvest progress for UI (sets done / sets queued).</summary>
        public static int ProgressCurrent { get; private set; }
        public static int ProgressTotal { get; private set; }

        /// <summary>Bumped whenever the cache changes (UI refresh hint).</summary>
        public static int CacheVersion { get; private set; }

        /// <summary>Dev aid: also dump every candidate texture during harvest.</summary>
        public static bool DumpRawTextures;

        private static string _cacheRoot;
        private static Dictionary<string, AtlasEntry> _atlasIndex; // name → entry
        private static readonly Dictionary<string, Texture2D> LoadedAtlases =
            new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Sprite> SpriteCache =
            new Dictionary<string, Sprite>();

        private class AtlasEntry
        {
            public string Name;
            public string File;
            public int Width;
            public int Height;
        }

        public static string CacheRoot
        {
            get
            {
                if (_cacheRoot == null)
                {
                    _cacheRoot = Path.Combine(BepInEx.Paths.PluginPath,
                        Path.Combine("E2EMapEditor", "tilecache"));
                }
                return _cacheRoot;
            }
        }

        private static string AtlasDir => Path.Combine(CacheRoot, "atlases");
        private static string SetsDir => Path.Combine(CacheRoot, "sets");
        private static string AtlasIndexPath => Path.Combine(AtlasDir, "index.txt");

        // ---- set enumeration ----

        public class SetInfo
        {
            public string Id;          // bundle/scene name, lowercase ("dlc04_prison")
            public string SceneName;   // exact scene name ("DLC04_Prison")
            public string DisplayName; // localized prison name
            public bool Installed;     // DLC owned / base game
            public bool Cached;        // harvest output exists
            public int AtlasCount;     // cached atlas count
        }

        /// <summary>
        /// Every prison scene that could be harvested on this machine:
        /// all non-debug prisons whose content is installed (base game or
        /// owned DLC). Returns an empty list before the frontend is up.
        /// </summary>
        public static List<SetInfo> ListSets()
        {
            var result = new List<SetInfo>();
            var levelData = LevelDataManager.GetInstance();
            var platform = Platform.GetInstance();
            if (levelData == null || levelData.m_T17Levels == null)
            {
                return result;
            }
            var seen = new HashSet<string>();
            foreach (var prison in levelData.m_T17Levels)
            {
                if (prison == null || prison.m_bIsDebug || prison.m_LevelInfo == null ||
                    string.IsNullOrEmpty(prison.m_LevelInfo.m_AssociatedFile))
                {
                    continue;
                }
                string scene = prison.m_LevelInfo.m_AssociatedFile;
                string id = scene.ToLowerInvariant();
                if (!seen.Add(id))
                {
                    continue;
                }
                bool installed = !prison.m_bIsDLC ||
                    (prison.m_DLCData != null && platform != null &&
                     platform.IsDLCAvailable(prison.m_DLCData));
                string name;
                if (!Localization.Get(prison.m_NameLocalizationKey, out name) ||
                    string.IsNullOrEmpty(name))
                {
                    name = prison.m_NameLocalizationKey;
                }
                var info = new SetInfo
                {
                    Id = id,
                    SceneName = scene,
                    DisplayName = name,
                    Installed = installed,
                };
                string indexPath = Path.Combine(SetsDir, SanitizeStrict(id) + ".txt");
                if (File.Exists(indexPath))
                {
                    info.Cached = true;
                    try
                    {
                        info.AtlasCount = File.ReadAllLines(indexPath).Length;
                    }
                    catch
                    {
                    }
                }
                result.Add(info);
            }
            return result;
        }

        /// <summary>Atlas names cached for one set (empty when not harvested).</summary>
        public static List<string> GetSetAtlases(string setId)
        {
            var result = new List<string>();
            string indexPath = Path.Combine(SetsDir, SanitizeStrict(setId) + ".txt");
            if (!File.Exists(indexPath))
            {
                return result;
            }
            foreach (var line in File.ReadAllLines(indexPath))
            {
                if (line.Length > 0)
                {
                    result.Add(line);
                }
            }
            return result;
        }

        // ---- harvesting ----

        /// <summary>
        /// Harvest one set (or all installed sets when <paramref name="setId"/>
        /// is "all"). Only allowed at the frontend — never during a level or
        /// editor session, because the prison scene is loaded additively.
        /// Returns a human-readable acceptance/rejection message.
        /// </summary>
        public static string StartHarvest(string setId)
        {
            if (Busy)
            {
                return "harvest already running: " + Status;
            }
            if (BaseLevelManager.GetInstance() != null || Events.GameEvents.IsInEditor)
            {
                return "harvesting only works from the main menu (leave the editor/level first)";
            }
            var sets = ListSets();
            if (sets.Count == 0)
            {
                return "no prison data yet (wait for the frontend to load)";
            }
            var queue = new List<SetInfo>();
            if (setId == "all")
            {
                foreach (var set in sets)
                {
                    if (set.Installed && !set.Cached)
                    {
                        queue.Add(set);
                    }
                }
                if (queue.Count == 0)
                {
                    return "all installed sets are already cached";
                }
            }
            else
            {
                var match = sets.Find(s => s.Id == setId);
                if (match == null)
                {
                    return "unknown set: " + setId;
                }
                if (!match.Installed)
                {
                    return "set not installed (DLC not owned): " + setId;
                }
                queue.Add(match);
            }
            Busy = true;
            Status = "starting";
            ApiRunner.Ensure();
            ApiRunner.StartRoutine(HarvestQueue(queue));
            return "harvest started (" + queue.Count + " set(s))";
        }

        private static IEnumerator HarvestQueue(List<SetInfo> queue)
        {
            ProgressTotal = queue.Count;
            for (int i = 0; i < queue.Count; i++)
            {
                var set = queue[i];
                ProgressCurrent = i;
                Status = "harvesting " + set.Id + " (" + (i + 1) + "/" + queue.Count + ")";
                Log.Info("TileSets: harvesting " + set.SceneName);
                var routine = HarvestOne(set);
                while (true)
                {
                    object current;
                    try
                    {
                        if (!routine.MoveNext())
                        {
                            break;
                        }
                        current = routine.Current;
                    }
                    catch (Exception e)
                    {
                        Log.Error("TileSets: harvest of " + set.Id + " failed: " + e);
                        Status = "error on " + set.Id + ": " + e.Message;
                        break;
                    }
                    yield return current;
                }
            }
            Busy = false;
            Status = "idle";
            ProgressCurrent = ProgressTotal;
            CacheVersion++;
            Log.Info("TileSets: harvest queue done");
        }

        private static IEnumerator HarvestOne(SetInfo set)
        {
            var texturesBefore = SnapshotIds<Texture2D>();

            string bundlePath = Platform.GetStreamingAssetsPath() + "/AssetBundles/" + set.Id;
            if (!File.Exists(bundlePath))
            {
                Log.Warn("TileSets: bundle missing: " + bundlePath);
                yield break;
            }
            var bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
            yield return bundleRequest;
            var bundle = bundleRequest.assetBundle;
            if (bundle == null)
            {
                Log.Warn("TileSets: could not load bundle " + set.Id);
                yield break;
            }
            AsyncOperation load = null;
            try
            {
                load = SceneManager.LoadSceneAsync(set.SceneName, LoadSceneMode.Additive);
            }
            catch (Exception e)
            {
                Log.Error("TileSets: scene load threw: " + e.Message);
            }
            if (load == null)
            {
                bundle.Unload(true);
                yield break;
            }
            yield return load;

            // freeze the scene before its Start/Update can run
            var scene = SceneManager.GetSceneByName(set.SceneName);
            if (scene.IsValid())
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root != null)
                    {
                        root.SetActive(false);
                    }
                }
            }
            yield return null;

            try
            {
                var newTextures = CollectNew<Texture2D>(texturesBefore);
                Log.Info("TileSets: " + set.Id + " → " + newTextures.Count + " new textures");
                CacheAtlases(set, newTextures);
            }
            finally
            {
                if (scene.IsValid())
                {
                    SceneManager.UnloadSceneAsync(set.SceneName);
                }
            }
            yield return null;
            bundle.Unload(true);
            yield return Resources.UnloadUnusedAssets();
        }

        private static HashSet<int> SnapshotIds<T>() where T : UnityEngine.Object
        {
            var ids = new HashSet<int>();
            foreach (var obj in Resources.FindObjectsOfTypeAll<T>())
            {
                if (obj != null)
                {
                    ids.Add(obj.GetInstanceID());
                }
            }
            return ids;
        }

        private static List<T> CollectNew<T>(HashSet<int> before) where T : UnityEngine.Object
        {
            var result = new List<T>();
            foreach (var obj in Resources.FindObjectsOfTypeAll<T>())
            {
                if (obj != null && !before.Contains(obj.GetInstanceID()))
                {
                    result.Add(obj);
                }
            }
            return result;
        }

        // ---- atlas selection + caching ----

        private static readonly string[] ExcludeContains =
        {
            "MapTexture", "SpriteAtlasTexture", "Generic_Escape", "_DMG",
            "LoadingScreen", "uvRef", "Lightmap", "ReflectionProbe",
        };

        /// <summary>
        /// Is this scene texture worth offering as paintable map art?
        /// Excludes the baked whole-floor composites, UI atlases, damage
        /// variants and tiny item icons.
        /// </summary>
        private static bool IsPaintableAtlas(Texture2D tex)
        {
            string name = tex.name;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            int min = Mathf.Min(tex.width, tex.height);
            int max = Mathf.Max(tex.width, tex.height);
            if (min < 32 || max < 64 || max > 2048)
            {
                return false;
            }
            foreach (var pattern in ExcludeContains)
            {
                if (name.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return false;
                }
            }
            if (name.StartsWith("FE_") || name.StartsWith("Hud") || name.StartsWith("UI"))
            {
                return false;
            }
            // baked floor composites: "Floor0", "Underground", "Vent3", …
            string trimmed = name.TrimEnd("0123456789".ToCharArray());
            if (trimmed == "Floor" || trimmed == "Underground" || trimmed == "Vent")
            {
                return false;
            }
            return true;
        }

        private static void CacheAtlases(SetInfo set, List<Texture2D> textures)
        {
            Directory.CreateDirectory(AtlasDir);
            Directory.CreateDirectory(SetsDir);
            var index = AtlasIndex();
            var setAtlases = new List<string>();
            var inventory = new StringBuilder();
            inventory.Append("{\"set\":\"").Append(set.Id).Append("\",\"textures\":[");
            bool first = true;
            string rawDir = null;
            if (DumpRawTextures)
            {
                rawDir = Path.Combine(Path.Combine(CacheRoot, "raw"), set.Id);
                Directory.CreateDirectory(rawDir);
            }
            foreach (var tex in textures)
            {
                if (!first)
                {
                    inventory.Append(",");
                }
                first = false;
                inventory.Append("{\"name\":\"").Append(Escape(tex.name))
                    .Append("\",\"w\":").Append(tex.width)
                    .Append(",\"h\":").Append(tex.height)
                    .Append(",\"fmt\":\"").Append(tex.format).Append("\"}");
                if (!IsPaintableAtlas(tex))
                {
                    continue;
                }
                if (setAtlases.Contains(tex.name))
                {
                    continue; // duplicate name within the scene
                }
                try
                {
                    if (!index.ContainsKey(tex.name))
                    {
                        string file = UniqueAtlasFile(index, tex.name);
                        var readable = MakeReadable(tex);
                        File.WriteAllBytes(Path.Combine(AtlasDir, file), readable.EncodeToPNG());
                        UnityEngine.Object.Destroy(readable);
                        var entry = new AtlasEntry
                        {
                            Name = tex.name,
                            File = file,
                            Width = tex.width,
                            Height = tex.height,
                        };
                        index[tex.name] = entry;
                        AppendAtlasIndexLine(entry);
                    }
                    setAtlases.Add(tex.name);
                    if (rawDir != null)
                    {
                        var readable = MakeReadable(tex);
                        File.WriteAllBytes(Path.Combine(rawDir, Sanitize(tex.name) + ".png"),
                            readable.EncodeToPNG());
                        UnityEngine.Object.Destroy(readable);
                    }
                }
                catch (Exception e)
                {
                    Log.Warn("TileSets: caching " + tex.name + " failed: " + e.Message);
                }
            }
            inventory.Append("]}");
            setAtlases.Sort(StringComparer.OrdinalIgnoreCase);
            string setFile = SanitizeStrict(set.Id);
            File.WriteAllLines(Path.Combine(SetsDir, setFile + ".txt"), setAtlases.ToArray());
            File.WriteAllText(Path.Combine(SetsDir, setFile + ".inventory.json"),
                inventory.ToString());
            CacheVersion++;
            Log.Info("TileSets: " + set.Id + " cached (" + setAtlases.Count + " atlases)");
        }

        private static string UniqueAtlasFile(Dictionary<string, AtlasEntry> index, string name)
        {
            string baseName = Sanitize(name);
            string candidate = baseName + ".png";
            int suffix = 2;
            while (FileInUse(index, candidate))
            {
                candidate = baseName + "_" + suffix + ".png";
                suffix++;
            }
            return candidate;
        }

        private static bool FileInUse(Dictionary<string, AtlasEntry> index, string file)
        {
            foreach (var entry in index.Values)
            {
                if (string.Equals(entry.File, file, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        // ---- atlas index + runtime access ----

        private static Dictionary<string, AtlasEntry> AtlasIndex()
        {
            if (_atlasIndex != null)
            {
                return _atlasIndex;
            }
            _atlasIndex = new Dictionary<string, AtlasEntry>();
            if (File.Exists(AtlasIndexPath))
            {
                foreach (var line in File.ReadAllLines(AtlasIndexPath))
                {
                    string[] parts = line.Split('|');
                    int w, h;
                    if (parts.Length == 4 && int.TryParse(parts[2], out w) &&
                        int.TryParse(parts[3], out h))
                    {
                        _atlasIndex[parts[0]] = new AtlasEntry
                        {
                            Name = parts[0],
                            File = parts[1],
                            Width = w,
                            Height = h,
                        };
                    }
                }
            }
            return _atlasIndex;
        }

        private static void AppendAtlasIndexLine(AtlasEntry entry)
        {
            Directory.CreateDirectory(AtlasDir);
            File.AppendAllText(AtlasIndexPath,
                entry.Name + "|" + entry.File + "|" + entry.Width + "|" + entry.Height + "\n");
        }

        /// <summary>True when the named atlas is in the local cache.</summary>
        public static bool HasAtlas(string atlasName) => AtlasIndex().ContainsKey(atlasName);

        /// <summary>Cache-relative PNG path for an atlas, or null.</summary>
        public static string GetAtlasPngPath(string atlasName)
        {
            AtlasEntry entry;
            if (!AtlasIndex().TryGetValue(atlasName, out entry))
            {
                return null;
            }
            string path = Path.Combine(AtlasDir, entry.File);
            return File.Exists(path) ? path : null;
        }

        public static bool GetAtlasSize(string atlasName, out int width, out int height)
        {
            AtlasEntry entry;
            if (AtlasIndex().TryGetValue(atlasName, out entry))
            {
                width = entry.Width;
                height = entry.Height;
                return true;
            }
            width = height = 0;
            return false;
        }

        /// <summary>The cached atlas as a live texture (lazy, kept loaded).</summary>
        public static Texture2D GetAtlasTexture(string atlasName)
        {
            Texture2D tex;
            if (LoadedAtlases.TryGetValue(atlasName, out tex) && tex != null)
            {
                return tex;
            }
            string path = GetAtlasPngPath(atlasName);
            if (path == null)
            {
                return null;
            }
            tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            if (!tex.LoadImage(File.ReadAllBytes(path)))
            {
                UnityEngine.Object.Destroy(tex);
                return null;
            }
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            LoadedAtlases[atlasName] = tex;
            return tex;
        }

        /// <summary>
        /// Sprite for an atlas region (pixel rect, bottom-left origin) at
        /// <see cref="TilePixels"/> per world tile. Null when the atlas is
        /// not cached on this machine.
        /// </summary>
        public static Sprite GetSprite(string atlasName, int x, int y, int w, int h)
        {
            string key = atlasName + ":" + x + "," + y + "," + w + "," + h;
            Sprite sprite;
            if (SpriteCache.TryGetValue(key, out sprite) && sprite != null &&
                sprite.texture != null)
            {
                return sprite;
            }
            var tex = GetAtlasTexture(atlasName);
            if (tex == null)
            {
                return null;
            }
            x = Mathf.Clamp(x, 0, tex.width - 1);
            y = Mathf.Clamp(y, 0, tex.height - 1);
            w = Mathf.Clamp(w, 1, tex.width - x);
            h = Mathf.Clamp(h, 1, tex.height - y);
            sprite = Sprite.Create(tex, new Rect(x, y, w, h),
                new Vector2(0f, 0f), TilePixels);
            SpriteCache[key] = sprite;
            return sprite;
        }

        /// <summary>PNG bytes of an atlas region (for UI thumbnails). Null when uncached.</summary>
        public static byte[] GetRegionPng(string atlasName, int x, int y, int w, int h)
        {
            var tex = GetAtlasTexture(atlasName);
            if (tex == null)
            {
                return null;
            }
            x = Mathf.Clamp(x, 0, tex.width - 1);
            y = Mathf.Clamp(y, 0, tex.height - 1);
            w = Mathf.Clamp(w, 1, tex.width - x);
            h = Mathf.Clamp(h, 1, tex.height - y);
            var crop = new Texture2D(w, h, TextureFormat.ARGB32, false);
            crop.SetPixels(tex.GetPixels(x, y, w, h));
            crop.Apply();
            byte[] png = crop.EncodeToPNG();
            UnityEngine.Object.Destroy(crop);
            return png;
        }

        /// <summary>
        /// Returns true if the atlas region (bottom-left origin, pixel coords) has
        /// at least one pixel with alpha > 0.04 (~10/255). Used by the auto-detect animation feature.
        /// </summary>
        public static bool RegionHasContent(string atlasName, int x, int y, int w, int h)
        {
            var tex = GetAtlasTexture(atlasName);
            if (tex == null) return false;
            x = Mathf.Clamp(x, 0, tex.width - 1);
            y = Mathf.Clamp(y, 0, tex.height - 1);
            w = Mathf.Clamp(w, 1, tex.width - x);
            h = Mathf.Clamp(h, 1, tex.height - y);
            var pixels = tex.GetPixels(x, y, w, h);
            foreach (var c in pixels)
            {
                if (c.a > 0.04f) return true;
            }
            return false;
        }

        // ---- helpers ----

        /// <summary>CPU-readable copy of any texture via a RenderTexture blit.</summary>
        internal static Texture2D MakeReadable(Texture2D source)
        {
            var rt = RenderTexture.GetTemporary(source.width, source.height, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            var previous = RenderTexture.active;
            try
            {
                Graphics.Blit(source, rt);
                RenderTexture.active = rt;
                var readable = new Texture2D(source.width, source.height, TextureFormat.ARGB32, false);
                readable.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
                readable.Apply();
                return readable;
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        private static string Sanitize(string name)
        {
            var sb = new StringBuilder(name.Length);
            foreach (char c in name)
            {
                sb.Append(char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.' ? c : '_');
            }
            return sb.ToString();
        }

        private static string SanitizeStrict(string name)
        {
            return Sanitize(name).Replace(".", "");
        }

        private static string Escape(string s)
        {
            return s == null ? "" : s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        public static string StatusJson()
        {
            var sb = new StringBuilder();
            sb.Append("{\"busy\":").Append(Busy ? "true" : "false");
            sb.Append(",\"status\":\"").Append(Escape(Status)).Append("\"");
            sb.Append(",\"cacheVersion\":").Append(CacheVersion);
            sb.Append(",\"atFrontend\":").Append(
                BaseLevelManager.GetInstance() == null && !Events.GameEvents.IsInEditor
                ? "true" : "false");
            sb.Append(",\"sets\":[");
            bool first = true;
            foreach (var set in ListSets())
            {
                if (!first)
                {
                    sb.Append(",");
                }
                first = false;
                sb.Append("{\"id\":\"").Append(Escape(set.Id)).Append("\"");
                sb.Append(",\"name\":\"").Append(Escape(set.DisplayName)).Append("\"");
                sb.Append(",\"installed\":").Append(set.Installed ? "true" : "false");
                sb.Append(",\"cached\":").Append(set.Cached ? "true" : "false");
                sb.Append(",\"atlases\":").Append(set.AtlasCount).Append("}");
            }
            sb.Append("]}");
            return sb.ToString();
        }

        /// <summary>JSON list of a set's cached atlases with sizes.</summary>
        public static string SetAtlasesJson(string setId)
        {
            var sb = new StringBuilder();
            sb.Append("{\"set\":\"").Append(Escape(setId)).Append("\",\"atlases\":[");
            bool first = true;
            foreach (var name in GetSetAtlases(setId))
            {
                int w, h;
                if (!GetAtlasSize(name, out w, out h))
                {
                    continue;
                }
                if (!first)
                {
                    sb.Append(",");
                }
                first = false;
                sb.Append("{\"name\":\"").Append(Escape(name))
                  .Append("\",\"w\":").Append(w).Append(",\"h\":").Append(h).Append("}");
            }
            sb.Append("]}");
            return sb.ToString();
        }
    }
}
