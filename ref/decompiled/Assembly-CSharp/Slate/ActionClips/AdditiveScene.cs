using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Slate.ActionClips;

[Description("Additive load a Scene for a duration of time, or permanentely if length is zero.\n\nIf 'Update Root Cutscenes' is true, all root cutscenes objects of that scene will also be updated for the duration of the clip with an optional time offset provided.")]
[Category("Composition")]
public class AdditiveScene : DirectorActionClip, ISubClipContainable, IDirectable
{
	[HideInInspector]
	[SerializeField]
	private float _length = 5f;

	[HideInInspector]
	[SerializeField]
	private string _scenePath;

	public Vector3 scenePosition;

	public MiniTransformSpace space;

	public bool updateRootCutscenes = true;

	public float timeOffset;

	private Scene subScene;

	private List<Cutscene> rootCutscenes;

	private bool temporary;

	private bool waitLoad;

	float ISubClipContainable.subClipOffset
	{
		get
		{
			return timeOffset;
		}
		set
		{
			timeOffset = value;
		}
	}

	public override bool isValid => !string.IsNullOrEmpty(_scenePath);

	public override float length
	{
		get
		{
			return _length;
		}
		set
		{
			_length = value;
		}
	}

	protected override void OnEnter()
	{
		temporary = length > 0f;
		Activate();
	}

	protected override void OnReverseEnter()
	{
		if (temporary)
		{
			Activate();
		}
	}

	protected override void OnUpdate(float time)
	{
		if (Application.isPlaying && waitLoad && subScene.isLoaded)
		{
			waitLoad = false;
			InitializeSubSceneCutscenes();
		}
		if (temporary && updateRootCutscenes && rootCutscenes != null)
		{
			for (int i = 0; i < rootCutscenes.Count; i++)
			{
				rootCutscenes[i].Sample(time - timeOffset);
			}
		}
	}

	protected override void OnExit()
	{
		if (temporary)
		{
			DenitializeSubSceneCutscenes(forward: true);
			Deactivate();
		}
	}

	protected override void OnReverse()
	{
		DenitializeSubSceneCutscenes(forward: false);
		Deactivate();
	}

	private void Activate()
	{
		if (!string.IsNullOrEmpty(_scenePath) && RootTimeWithinRange())
		{
			waitLoad = true;
			SceneManager.LoadSceneAsync(CleanPath(_scenePath), LoadSceneMode.Additive);
			subScene = SceneManager.GetSceneByPath(_scenePath);
		}
	}

	private void Deactivate()
	{
		if (!string.IsNullOrEmpty(_scenePath))
		{
			SceneManager.UnloadSceneAsync(CleanPath(_scenePath));
			waitLoad = false;
		}
	}

	private string CleanPath(string path)
	{
		return path.Replace("Assets/", string.Empty).Replace(".unity", string.Empty);
	}

	private void InitializeSubSceneCutscenes()
	{
		rootCutscenes = new List<Cutscene>();
		if (!subScene.isLoaded || !subScene.IsValid())
		{
			return;
		}
		GameObject[] rootGameObjects = subScene.GetRootGameObjects();
		foreach (GameObject gameObject in rootGameObjects)
		{
			gameObject.transform.position += TransformPoint(scenePosition, (TransformSpace)space);
			if (gameObject.GetComponent(typeof(IDirectableCamera)) is IDirectableCamera directableCamera)
			{
				directableCamera.gameObject.SetActive(value: false);
				continue;
			}
			Cutscene component = gameObject.GetComponent<Cutscene>();
			if (component != null)
			{
				rootCutscenes.Add(component);
			}
		}
	}

	private void DenitializeSubSceneCutscenes(bool forward)
	{
		if (rootCutscenes != null)
		{
			foreach (Cutscene rootCutscene in rootCutscenes)
			{
				if (rootCutscene != null)
				{
					if (forward)
					{
						rootCutscene.SkipAll();
					}
					else
					{
						rootCutscene.Rewind();
					}
				}
			}
		}
		rootCutscenes = null;
	}
}
