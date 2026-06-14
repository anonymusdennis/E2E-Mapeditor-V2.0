using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AssetManager : MonoBehaviour
{
	public class AssetBundleData
	{
		public enum BundleType
		{
			Scene_prison,
			Scene_other,
			Persistent,
			Other
		}

		private string m_BundleName;

		private AssetBundle m_TheBundle;

		private BundleType m_BundleType = BundleType.Scene_other;

		public object[] m_LoadedAssets;

		public string bundleName => m_BundleName;

		public bool isLoaded => m_TheBundle != null;

		public bool isSceneBundle => m_BundleType == BundleType.Scene_prison || m_BundleType != BundleType.Scene_other;

		public BundleType bundleType => m_BundleType;

		public IEnumerator LoadBundleAsync(string bundleName, BundleType bundleType)
		{
			m_BundleName = bundleName.ToLower();
			m_BundleType = bundleType;
			return LoadBundleAsync();
		}

		public IEnumerator LoadBundleAsync()
		{
			Debug.Log("ASSET MANAGER: loading bundle " + m_BundleName);
			string path = $"{Platform.GetStreamingAssetsPath()}/AssetBundles/{m_BundleName}";
			m_TheBundle = AssetBundle.LoadFromFile(path);
			if (m_TheBundle == null)
			{
				Debug.LogError("ASSET MANAGER: failed to load bundle " + m_BundleName);
			}
			yield break;
		}

		public void LoadBundle(string bundleName, BundleType bundleType)
		{
			m_BundleName = bundleName.ToLower();
			m_BundleType = bundleType;
			Debug.Log("ASSET MANAGER: loading bundle " + m_BundleName);
			string path = $"{Platform.GetStreamingAssetsPath()}/AssetBundles/{m_BundleName}";
			m_TheBundle = AssetBundle.LoadFromFile(path);
			if (m_TheBundle == null)
			{
				Debug.LogError("ASSET MANAGER: failed to load bundle " + m_BundleName);
			}
		}

		public void UnloadBundle(bool unloadAllObjects = true)
		{
			if (m_TheBundle != null)
			{
				Debug.Log("ASSET MANAGER: unloading bundle " + m_BundleName + " all assets = " + unloadAllObjects);
				m_LoadedAssets = null;
				m_TheBundle.Unload(unloadAllObjects);
				m_TheBundle = null;
			}
		}

		public object[] LoadAllAssets()
		{
			if (m_TheBundle != null)
			{
				m_LoadedAssets = m_TheBundle.LoadAllAssets();
			}
			return m_LoadedAssets;
		}
	}

	public const string OBJECTIVE_FILES_BUNDLE = "objectivefiles";

	[Tooltip("List of asset bundles to load on startup and keep loaded for the lifetime of the game")]
	public string[] m_PersistentAssetBundles;

	private List<AssetBundleData> m_AssetBundles = new List<AssetBundleData>();

	private static AssetManager s_TheInstance;

	private const string c_tempBuildDir = "Assets/Temp/Build/";

	private List<AssetBundleData> assetBundles => m_AssetBundles;

	public static AssetManager instance => s_TheInstance;

	private void Awake()
	{
		if (s_TheInstance != null)
		{
			Debug.LogError("ASSET MANAGER: creating duplicate instance. Deleting myself");
			Object.Destroy(this);
		}
		else
		{
			Object.DontDestroyOnLoad(this);
			s_TheInstance = this;
		}
	}

	private void Start()
	{
		if (BuildVersion.m_UseAssetBundles)
		{
			StartCoroutine(_LoadPersistenAssetBundlesAsync());
		}
	}

	public int GetLoadedAssetBundleCount()
	{
		int num = 0;
		int i = 0;
		for (int count = assetBundles.Count; i < count; i++)
		{
			AssetBundleData assetBundleData = instance.assetBundles[i];
			if (assetBundleData != null && assetBundleData.isLoaded)
			{
				num++;
			}
		}
		return num;
	}

	public YieldInstruction LoadSceneAsync(string sceneName, LoadSceneMode mode, bool alsoLoadIngameBundles = false)
	{
		return StartCoroutine(_LoadSceneAsync(sceneName, mode, alsoLoadIngameBundles));
	}

	public IEnumerator _LoadSceneAsync(string sceneName, LoadSceneMode mode, bool alsoLoadBundleOverrides)
	{
		if (BuildVersion.m_UseAssetBundles)
		{
			if (alsoLoadBundleOverrides)
			{
				yield return _LoadBundleAsync("aibehaviours", AssetBundleData.BundleType.Other, autoLoadAssets: true);
				yield return _LoadBundleAsync("objectivefiles", AssetBundleData.BundleType.Other, autoLoadAssets: true);
			}
			AssetBundleData.BundleType bundleType = ((!alsoLoadBundleOverrides) ? AssetBundleData.BundleType.Scene_other : AssetBundleData.BundleType.Scene_prison);
			yield return _LoadBundleAsync(sceneName, bundleType);
		}
		yield return SceneManager.LoadSceneAsync(sceneName, mode);
	}

	private IEnumerator _LoadPersistenAssetBundlesAsync()
	{
		yield return StartCoroutine(_LoadBundleAsync("fonts.dynamic", AssetBundleData.BundleType.Persistent, autoLoadAssets: true));
	}

	private IEnumerator _LoadBundleAsync(string bundleName, AssetBundleData.BundleType bundleType, bool autoLoadAssets = false)
	{
		string lowercaseBundleName = bundleName.ToLower();
		AssetBundleData bundleData2 = m_AssetBundles.Find((AssetBundleData bundle) => bundle.bundleName == lowercaseBundleName);
		if (bundleData2 == null)
		{
			bundleData2 = new AssetBundleData();
			m_AssetBundles.Add(bundleData2);
			yield return bundleData2.LoadBundleAsync(bundleName, bundleType);
			if (autoLoadAssets)
			{
				bundleData2.LoadAllAssets();
			}
		}
		else if (!bundleData2.isLoaded)
		{
			yield return bundleData2.LoadBundleAsync();
			if (autoLoadAssets)
			{
				bundleData2.LoadAllAssets();
			}
		}
	}

	public void LoadScene(string sceneName, LoadSceneMode mode, bool alsoLoadBundleOverrides)
	{
		if (BuildVersion.m_UseAssetBundles && alsoLoadBundleOverrides)
		{
			LoadBundle("aibehaviours", AssetBundleData.BundleType.Other, autoLoadAssets: true);
			LoadBundle("objectivefiles", AssetBundleData.BundleType.Other, autoLoadAssets: true);
		}
		SceneManager.LoadScene(sceneName, mode);
	}

	private void LoadBundle(string bundleName, AssetBundleData.BundleType bundleType, bool autoLoadAssets = false)
	{
		string lowercaseBundleName = bundleName.ToLower();
		AssetBundleData assetBundleData = m_AssetBundles.Find((AssetBundleData bundle) => bundle.bundleName == lowercaseBundleName);
		if (assetBundleData == null)
		{
			assetBundleData = new AssetBundleData();
			m_AssetBundles.Add(assetBundleData);
			assetBundleData.LoadBundle(bundleName, bundleType);
			if (autoLoadAssets)
			{
				assetBundleData.LoadAllAssets();
			}
		}
		else if (!assetBundleData.isLoaded)
		{
			assetBundleData.LoadBundle(bundleName, bundleType);
			if (autoLoadAssets)
			{
				assetBundleData.LoadAllAssets();
			}
		}
	}

	public YieldInstruction UnloadSceneCoroutine(string sceneName)
	{
		return StartCoroutine(UnloadSceneAsync(sceneName));
	}

	public IEnumerator UnloadSceneAsync(string sceneName)
	{
		yield return SceneManager.UnloadSceneAsync(sceneName);
		if (!BuildVersion.m_UseAssetBundles)
		{
			yield break;
		}
		string text = sceneName.ToLower();
		bool flag = false;
		int num = 0;
		int i = 0;
		for (int count = m_AssetBundles.Count; i < count; i++)
		{
			if (!m_AssetBundles[i].isLoaded)
			{
				continue;
			}
			if (m_AssetBundles[i].bundleType == AssetBundleData.BundleType.Scene_prison)
			{
				num++;
			}
			if (m_AssetBundles[i].bundleName == text)
			{
				m_AssetBundles[i].UnloadBundle();
				if (m_AssetBundles[i].bundleType == AssetBundleData.BundleType.Scene_prison)
				{
					flag = true;
				}
			}
		}
		if (!flag || num != 1)
		{
			yield break;
		}
		int j = 0;
		for (int count2 = m_AssetBundles.Count; j < count2; j++)
		{
			if (m_AssetBundles[j].bundleName == "aibehaviours" || m_AssetBundles[j].bundleName == "objectivefiles")
			{
				m_AssetBundles[j].UnloadBundle();
				break;
			}
		}
	}

	public void UnloadScene(string sceneName)
	{
	}

	public object GetAssetFromBundle(string bundleName, string assetName)
	{
		int i = 0;
		for (int count = m_AssetBundles.Count; i < count; i++)
		{
			if (!(m_AssetBundles[i].bundleName == bundleName))
			{
				continue;
			}
			object[] loadedAssets = m_AssetBundles[i].m_LoadedAssets;
			if (loadedAssets != null)
			{
				int j = 0;
				for (int num = loadedAssets.Length; j < num; j++)
				{
					Object @object = loadedAssets[j] as Object;
					if (@object != null && @object.name == assetName)
					{
						return loadedAssets[j];
					}
				}
			}
			return null;
		}
		return null;
	}

	public T GetAssetFromBundle<T>(string bundleName) where T : class
	{
		int i = 0;
		for (int count = m_AssetBundles.Count; i < count; i++)
		{
			if (!(m_AssetBundles[i].bundleName == bundleName))
			{
				continue;
			}
			object[] loadedAssets = m_AssetBundles[i].m_LoadedAssets;
			if (loadedAssets != null)
			{
				int j = 0;
				for (int num = loadedAssets.Length; j < num; j++)
				{
					if (loadedAssets[j].GetType() == typeof(T))
					{
						return loadedAssets[j] as T;
					}
				}
			}
			return (T)null;
		}
		return (T)null;
	}

	public bool SwapAssetForOneInBundle<T>(string bundleName, ref T assetToSwap) where T : Object
	{
		if (assetToSwap != null)
		{
			string text = assetToSwap.name;
			text = text.Replace("(Clone)", string.Empty);
			object assetFromBundle = GetAssetFromBundle(bundleName, text);
			if (assetFromBundle != null && assetFromBundle is T)
			{
				assetToSwap = assetFromBundle as T;
				return true;
			}
		}
		return false;
	}
}
