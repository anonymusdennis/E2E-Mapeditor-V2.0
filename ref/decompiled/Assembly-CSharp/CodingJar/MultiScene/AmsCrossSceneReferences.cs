using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodingJar.MultiScene;

[ExecuteInEditMode]
public sealed class AmsCrossSceneReferences : MonoBehaviour
{
	[SerializeField]
	private List<RuntimeCrossSceneReference> _crossSceneReferences = new List<RuntimeCrossSceneReference>();

	[SerializeField]
	[HideInInspector]
	private List<GameObject> _realSceneRootsForPostBuild = new List<GameObject>();

	private List<RuntimeCrossSceneReference> _referencesToResolve = new List<RuntimeCrossSceneReference>();

	private AmsMultiSceneSetup[] allMultiSceneSetups;

	public static event Action<RuntimeCrossSceneReference> CrossSceneReferenceRestored;

	public static AmsCrossSceneReferences GetSceneSingleton(Scene scene, bool bCreateIfNotFound)
	{
		AmsMultiSceneSetup sceneSingleton = scene.GetSceneSingleton<AmsMultiSceneSetup>(bCreateIfNotFound);
		if (!sceneSingleton)
		{
			return null;
		}
		AmsCrossSceneReferences component = sceneSingleton.gameObject.GetComponent<AmsCrossSceneReferences>();
		if ((bool)component)
		{
			return component;
		}
		if (bCreateIfNotFound)
		{
			return sceneSingleton.gameObject.AddComponent<AmsCrossSceneReferences>();
		}
		return null;
	}

	public void AddReference(RuntimeCrossSceneReference reference)
	{
		int num = _crossSceneReferences.FindIndex(((RuntimeCrossSceneReference)reference).IsSameSource);
		if (num >= 0)
		{
			_crossSceneReferences[num] = reference;
		}
		else
		{
			_crossSceneReferences.Add(reference);
		}
	}

	public void ResetCrossSceneReferences(Scene toScene)
	{
		_crossSceneReferences.RemoveAll((RuntimeCrossSceneReference x) => x.toScene.scene == toScene);
	}

	private void Awake()
	{
		_referencesToResolve.Clear();
		_referencesToResolve.AddRange(_crossSceneReferences);
	}

	private void Start()
	{
		PerformPostBuildCleanup();
		ResolvePendingCrossSceneReferences(bIsBaked: false);
		AmsMultiSceneSetup.OnStart = (Action<AmsMultiSceneSetup>)Delegate.Remove(AmsMultiSceneSetup.OnStart, new Action<AmsMultiSceneSetup>(HandleNewSceneLoaded));
		AmsMultiSceneSetup.OnStart = (Action<AmsMultiSceneSetup>)Delegate.Combine(AmsMultiSceneSetup.OnStart, new Action<AmsMultiSceneSetup>(HandleNewSceneLoaded));
		AmsMultiSceneSetup.OnDestroyed = (Action<AmsMultiSceneSetup>)Delegate.Remove(AmsMultiSceneSetup.OnDestroyed, new Action<AmsMultiSceneSetup>(HandleSceneDestroyed));
		AmsMultiSceneSetup.OnDestroyed = (Action<AmsMultiSceneSetup>)Delegate.Combine(AmsMultiSceneSetup.OnDestroyed, new Action<AmsMultiSceneSetup>(HandleSceneDestroyed));
	}

	private void OnDestroy()
	{
		AmsMultiSceneSetup.OnStart = (Action<AmsMultiSceneSetup>)Delegate.Remove(AmsMultiSceneSetup.OnStart, new Action<AmsMultiSceneSetup>(HandleNewSceneLoaded));
		AmsMultiSceneSetup.OnDestroyed = (Action<AmsMultiSceneSetup>)Delegate.Remove(AmsMultiSceneSetup.OnDestroyed, new Action<AmsMultiSceneSetup>(HandleSceneDestroyed));
	}

	private void HandleNewSceneLoaded(AmsMultiSceneSetup sceneSetup)
	{
		Scene scene = sceneSetup.gameObject.scene;
		if (!scene.isLoaded)
		{
			Debug.LogErrorFormat(this, "{0} Received HandleNewSceneLoaded from scene {1} which isn't considered loaded.  The scene MUST be considered loaded by this point", GetType().Name, scene.name);
		}
		foreach (RuntimeCrossSceneReference crossSceneReference in _crossSceneReferences)
		{
			if (!_referencesToResolve.Contains(crossSceneReference) && crossSceneReference.toScene.scene == scene)
			{
				_referencesToResolve.Add(crossSceneReference);
			}
		}
		if (_referencesToResolve.Count > 0)
		{
			allMultiSceneSetups = Resources.FindObjectsOfTypeAll<AmsMultiSceneSetup>();
			ConditionalResolveReferences(_referencesToResolve, bIsBaked: false);
			allMultiSceneSetups = null;
		}
	}

	private Scene QuickGetScene(ref AmsSceneReference sceneRef)
	{
		Scene sceneByPath = SceneManager.GetSceneByPath(sceneRef.editorPath);
		if (sceneByPath.IsValid())
		{
			return sceneByPath;
		}
		return SceneManager.GetSceneByPath(sceneRef.runtimePath);
	}

	private void HandleSceneDestroyed(AmsMultiSceneSetup sceneSetup)
	{
		Scene scene = sceneSetup.gameObject.scene;
		if (!scene.IsValid() || scene == base.gameObject.scene)
		{
			return;
		}
		List<RuntimeCrossSceneReference> list = new List<RuntimeCrossSceneReference>();
		allMultiSceneSetups = Resources.FindObjectsOfTypeAll<AmsMultiSceneSetup>();
		for (int num = _referencesToResolve.Count - 1; num >= 0; num--)
		{
			RuntimeCrossSceneReference item = _referencesToResolve[num];
			AmsSceneReference sceneRef = item.toScene;
			Scene scene2 = QuickGetScene(ref sceneRef);
			if (scene2 != scene)
			{
				list.Add(item);
			}
		}
		for (int num2 = _crossSceneReferences.Count - 1; num2 >= 0; num2--)
		{
			RuntimeCrossSceneReference item2 = _crossSceneReferences[num2];
			AmsSceneReference sceneRef2 = item2.toScene;
			Scene scene3 = QuickGetScene(ref sceneRef2);
			if (scene3 != scene)
			{
				list.Add(item2);
			}
		}
		_referencesToResolve = list;
		allMultiSceneSetups = null;
	}

	private IEnumerator CoWaitForSceneLoadThenResolveReferences(Scene scene)
	{
		if (!Application.isPlaying)
		{
			Debug.LogErrorFormat(this, "CoWaitForSceneLoadThenResolveReferences called, but we're not playing. Co-routines do not work reliably in the Editor!");
		}
		else if (scene.IsValid())
		{
			while (!scene.isLoaded)
			{
				yield return null;
			}
			ResolvePendingCrossSceneReferences(bIsBaked: false);
		}
	}

	[ContextMenu("Retry Pending Resolves")]
	public void ResolvePendingCrossSceneReferences(bool bIsBaked)
	{
		ConditionalResolveReferences(_referencesToResolve, bIsBaked);
	}

	[ContextMenu("Retry ALL Resolves")]
	private void RetryAllResolves()
	{
		_referencesToResolve.Clear();
		_referencesToResolve.AddRange(_crossSceneReferences);
		ResolvePendingCrossSceneReferences(bIsBaked: false);
	}

	private void ConditionalResolveReferences(List<RuntimeCrossSceneReference> references, bool bIsBaked)
	{
		for (int num = references.Count - 1; num >= 0; num--)
		{
			RuntimeCrossSceneReference obj = references[num];
			try
			{
				AmsSceneReference sceneRef = obj.fromScene;
				AmsSceneReference sceneRef2 = obj.toScene;
				if (QuickGetScene(ref sceneRef).isLoaded && QuickGetScene(ref sceneRef2).isLoaded)
				{
					references.RemoveAt(num);
					obj.Resolve(num, bIsBaked);
					if (AmsCrossSceneReferences.CrossSceneReferenceRestored != null)
					{
						AmsCrossSceneReferences.CrossSceneReferenceRestored(obj);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.Log("**PRISON_INFO** ConditionalResolveReferences ex " + ex.Message);
				Debug.LogException(ex, this);
			}
		}
	}

	private void PerformPostBuildCleanup()
	{
		if (!Application.isEditor || Application.isPlaying || _realSceneRootsForPostBuild.Count <= 0)
		{
			return;
		}
		GameObject[] rootGameObjects = base.gameObject.scene.GetRootGameObjects();
		GameObject[] array = rootGameObjects;
		foreach (GameObject gameObject in array)
		{
			if (!_realSceneRootsForPostBuild.Contains(gameObject))
			{
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
		}
		_realSceneRootsForPostBuild.Clear();
	}
}
