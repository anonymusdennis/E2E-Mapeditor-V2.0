using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace CodingJar.MultiScene;

[ExecuteInEditMode]
public class AmsMultiSceneSetup : MonoBehaviour, ISerializationCallbackReceiver
{
	[Serializable]
	public enum LoadMethod
	{
		Baked,
		Additive,
		AdditiveAsync,
		DontLoad
	}

	[Serializable]
	public class SceneEntry
	{
		[BeginReadonly]
		public AmsSceneReference scene;

		[Tooltip("Should this be automatically loaded in the Editor?")]
		[FormerlySerializedAs("isLoaded")]
		public bool loadInEditor;

		[Tooltip("How should we load this scene at Runtime?")]
		[EndReadonly]
		public LoadMethod loadMethod;

		public AsyncOperation asyncOp { get; set; }

		public override string ToString()
		{
			return $"{scene.name} loadInEditor: {loadInEditor} loadMethod: {loadMethod}";
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (!(obj is SceneEntry sceneEntry))
			{
				return false;
			}
			return scene.Equals(sceneEntry.scene) && loadInEditor == sceneEntry.loadInEditor && loadMethod == sceneEntry.loadMethod && asyncOp == sceneEntry.asyncOp;
		}

		public override int GetHashCode()
		{
			return scene.GetHashCode() * 4 + loadInEditor.GetHashCode() * 2 + loadMethod.GetHashCode();
		}
	}

	[SerializeField]
	private bool _isMainScene;

	[SerializeField]
	private List<SceneEntry> _sceneSetup = new List<SceneEntry>();

	[SerializeField]
	[Readonly]
	private string _thisScenePath;

	private List<SceneEntry> _bakedScenesMerged = new List<SceneEntry>();

	public static Action<AmsMultiSceneSetup> OnAwake;

	public static Action<AmsMultiSceneSetup> OnStart;

	public static Action<AmsMultiSceneSetup> OnDestroyed;

	public static bool AMS_FORCE_BLOCK_SERIALIZE_CODE;

	public ReadOnlyCollection<SceneEntry> GetSceneSetup()
	{
		return _sceneSetup.AsReadOnly();
	}

	private void Awake()
	{
		AmsDebug.Log(this, "{0}.Awake() (Scene {1}). IsLoaded: {2}. Frame: {3}", GetType().Name, base.gameObject.scene.name, base.gameObject.scene.isLoaded, Time.frameCount);
		if (OnAwake != null)
		{
			OnAwake(this);
		}
		if (_isMainScene && (!Application.isEditor || base.gameObject.scene.isLoaded || Time.frameCount > 1))
		{
			LoadSceneSetup();
		}
	}

	private void OnDestroy()
	{
		if (OnDestroyed != null)
		{
			OnDestroyed(this);
		}
	}

	private void Start()
	{
		AmsDebug.Log(this, "{0}.Start() Scene: {1}. Frame: {2}", GetType().Name, base.gameObject.scene.name, Time.frameCount);
		if (OnStart != null)
		{
			OnStart(this);
		}
		if (_isMainScene)
		{
			LoadSceneSetup();
		}
	}

	private void LoadSceneSetup()
	{
		if (_isMainScene)
		{
			LoadSceneSetupAtRuntime();
		}
	}

	private void LoadSceneSetupAtRuntime()
	{
		List<SceneEntry> list = new List<SceneEntry>(_sceneSetup);
		foreach (SceneEntry item in list)
		{
			LoadEntryAtRuntime(item);
		}
	}

	private void LoadEntryAtRuntime(SceneEntry entry)
	{
		if (entry.loadMethod == LoadMethod.DontLoad)
		{
			return;
		}
		Scene sceneByPath = SceneManager.GetSceneByPath(entry.scene.editorPath);
		if (!sceneByPath.IsValid())
		{
			sceneByPath = SceneManager.GetSceneByPath(entry.scene.runtimePath);
		}
		if (!sceneByPath.IsValid())
		{
			if (entry.loadMethod == LoadMethod.AdditiveAsync)
			{
				AmsDebug.Log(this, "Loading {0} Asynchronously from {1}", entry.scene.name, base.gameObject.scene.name);
				entry.asyncOp = SceneManager.LoadSceneAsync(entry.scene.runtimePath, LoadSceneMode.Additive);
			}
			else if (entry.loadMethod == LoadMethod.Additive)
			{
				AmsDebug.Log(this, "Loading {0} from {1}", entry.scene.name, base.gameObject.scene.name);
				SceneManager.LoadScene(entry.scene.runtimePath, LoadSceneMode.Additive);
			}
		}
	}

	private void Reset()
	{
		_isMainScene = SceneManager.GetActiveScene() == base.gameObject.scene;
	}

	public void OnAfterDeserialize()
	{
	}

	public void OnBeforeSerialize()
	{
	}

	private IEnumerator CoWaitAndBake()
	{
		bool bAllLoaded = false;
		while (!bAllLoaded)
		{
			bAllLoaded = true;
			foreach (SceneEntry item in _sceneSetup)
			{
				bAllLoaded = bAllLoaded && (item.loadMethod == LoadMethod.DontLoad || item.scene.isLoaded || _bakedScenesMerged.Contains(item));
			}
			if (!bAllLoaded)
			{
				yield return null;
			}
		}
		foreach (SceneEntry item2 in _sceneSetup)
		{
			if (CanMerge(item2))
			{
				PreMerge(item2);
			}
		}
		foreach (SceneEntry item3 in _sceneSetup)
		{
			if (CanMerge(item3))
			{
				MergeScene(item3);
				_bakedScenesMerged.Add(item3);
			}
		}
	}

	private bool CanMerge(SceneEntry entry)
	{
		if (entry.loadMethod != 0)
		{
			return false;
		}
		Scene scene = entry.scene.scene;
		if (!scene.IsValid())
		{
			return false;
		}
		Scene activeScene = SceneManager.GetActiveScene();
		if (scene == activeScene || scene == base.gameObject.scene)
		{
			return false;
		}
		if (!base.gameObject.scene.isLoaded)
		{
			return false;
		}
		return true;
	}

	private void PreMerge(SceneEntry entry)
	{
		Scene scene = entry.scene.scene;
		AmsCrossSceneReferences sceneSingleton = AmsCrossSceneReferences.GetSceneSingleton(scene, bCreateIfNotFound: false);
		if ((bool)sceneSingleton)
		{
			sceneSingleton.ResolvePendingCrossSceneReferences(bIsBaked: false);
		}
	}

	private void MergeScene(SceneEntry entry)
	{
		Scene scene = entry.scene.scene;
		AmsMultiSceneSetup sceneSingleton = scene.GetSceneSingleton<AmsMultiSceneSetup>(bCreate: false);
		if ((bool)sceneSingleton)
		{
			UnityEngine.Object.Destroy(sceneSingleton.gameObject);
		}
		AmsDebug.Log(this, "Merging {0} into {1}", scene.path, base.gameObject.scene.path);
		SceneManager.MergeScenes(scene, base.gameObject.scene);
	}

	public static void ForceCleanup()
	{
		OnAwake = null;
		OnStart = null;
		OnDestroyed = null;
	}
}
