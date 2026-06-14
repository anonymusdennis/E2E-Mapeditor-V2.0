using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CinematicEffects;

namespace Slate;

public class DirectorCamera : MonoBehaviour, IDirectableCamera
{
	public const string SET_MAIN_KEY = "Slate.SetMainWhenActive";

	public const string MATCH_MAIN_KEY = "Slate.MatchMainCamera";

	public const string AUTO_HANDLE_KEY = "Slate.AutoHandleRenderCamera";

	public const string DONT_DESTROY_KEY = "Slate.DirectorDontDestroyOnLoad";

	private static DirectorCamera _current;

	private static Camera _cam;

	private static DepthOfField dof;

	private static IDirectableCamera lastTargetShot;

	public static DirectorCamera current
	{
		get
		{
			if (_current == null)
			{
				_current = FindDirectorCamera();
				if (_current == null)
				{
					_current = new GameObject("★ Director Camera Root").AddComponent<DirectorCamera>();
					_current.cam.nearClipPlane = 0.01f;
					_current.cam.farClipPlane = 1000f;
				}
			}
			_current.gameObject.SetActive(value: false);
			return _current;
		}
	}

	public Camera cam
	{
		get
		{
			if (_cam == null)
			{
				_cam = GetComponentInChildren<Camera>(includeInactive: true);
				if (_cam == null)
				{
					_cam = CreateRenderCamera();
				}
			}
			return _cam;
		}
	}

	public Vector3 position
	{
		get
		{
			return current.transform.position;
		}
		set
		{
			current.transform.position = value;
		}
	}

	public Quaternion rotation
	{
		get
		{
			return current.transform.rotation;
		}
		set
		{
			current.transform.rotation = value;
		}
	}

	public float fieldOfView
	{
		get
		{
			return (!cam.orthographic) ? cam.fieldOfView : cam.orthographicSize;
		}
		set
		{
			cam.fieldOfView = value;
			cam.orthographicSize = value;
		}
	}

	public float focalPoint
	{
		get
		{
			return (!(dof != null)) ? 10f : dof.focus.focusPlane;
		}
		set
		{
			if (dof != null)
			{
				dof.focus.focusPlane = Mathf.Max(value, 0f);
			}
		}
	}

	public float focalRange
	{
		get
		{
			return (!(dof != null)) ? 15f : dof.focus.range;
		}
		set
		{
			if (dof != null)
			{
				dof.focus.range = Mathf.Max(value, 0f);
			}
		}
	}

	public static bool setMainWhenActive
	{
		get
		{
			return false;
		}
		set
		{
			PlayerPrefs.SetInt("Slate.SetMainWhenActive", value ? 1 : 0);
		}
	}

	public static bool matchMainCamera => false;

	public static bool autoHandleActiveState
	{
		get
		{
			return false;
		}
		set
		{
			PlayerPrefs.SetInt("Slate.AutoHandleRenderCamera", value ? 1 : 0);
		}
	}

	public static bool dontDestroyOnLoad
	{
		get
		{
			return false;
		}
		set
		{
			PlayerPrefs.SetInt("Slate.DirectorDontDestroyOnLoad", value ? 1 : 0);
		}
	}

	public static Camera renderCamera => current.cam;

	public static GameCamera gameCamera { get; set; }

	public static bool isEnabled { get; private set; }

	public static event Action<IDirectableCamera> OnCut;

	public static event Action OnActivate;

	public static event Action OnDeactivate;

	private void Awake()
	{
		if (_current != null && _current != this)
		{
			UnityEngine.Object.DestroyImmediate(base.gameObject);
			return;
		}
		_current = this;
		if (dontDestroyOnLoad)
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		Disable();
	}

	protected virtual void OnDestroy()
	{
		_current = null;
		_cam = null;
		dof = null;
		lastTargetShot = null;
	}

	public static void ForceCleanup()
	{
		if (_current != null)
		{
			UnityEngine.Object.Destroy(_current.gameObject);
		}
		_current = null;
		_cam = null;
		dof = null;
		lastTargetShot = null;
	}

	private Camera CreateRenderCamera()
	{
		_cam = new GameObject("Render Camera").AddComponent<Camera>();
		_cam.gameObject.AddComponent<AudioListener>();
		_cam.gameObject.AddComponent<GUILayer>();
		_cam.gameObject.AddComponent<FlareLayer>();
		_cam.transform.SetParent(base.transform);
		return _cam;
	}

	public static void Enable()
	{
		if (gameCamera == null)
		{
			Camera main = Camera.main;
			if (main != null && main != renderCamera)
			{
				gameCamera = main.GetComponent<GameCamera>();
				if (gameCamera == null)
				{
					gameCamera = main.gameObject.AddComponent<GameCamera>();
				}
			}
		}
		if (gameCamera != null)
		{
			gameCamera.gameObject.SetActive(value: false);
			if (matchMainCamera)
			{
				renderCamera.CopyFrom(gameCamera.cam);
			}
			current.transform.position = gameCamera.position;
			current.transform.rotation = gameCamera.rotation;
		}
		renderCamera.transform.localPosition = Vector3.zero;
		renderCamera.transform.localRotation = Quaternion.identity;
		if (setMainWhenActive)
		{
			renderCamera.gameObject.tag = "MainCamera";
		}
		DirectorGUI.current.enabled = true;
		if (autoHandleActiveState)
		{
			renderCamera.gameObject.SetActive(value: true);
		}
		dof = renderCamera.GetComponent<DepthOfField>();
		isEnabled = true;
		lastTargetShot = null;
		if (DirectorCamera.OnActivate != null)
		{
			DirectorCamera.OnActivate();
		}
	}

	public static void Disable()
	{
		if (DirectorCamera.OnDeactivate != null)
		{
			DirectorCamera.OnDeactivate();
		}
		DirectorGUI.current.enabled = false;
		if (autoHandleActiveState)
		{
			renderCamera.gameObject.SetActive(value: false);
		}
		if (setMainWhenActive)
		{
			renderCamera.gameObject.tag = "Untagged";
		}
		if (gameCamera != null)
		{
			gameCamera.gameObject.SetActive(value: true);
		}
		isEnabled = false;
	}

	public static void Update(IDirectableCamera source, IDirectableCamera target, EaseType interpolation, float weight, float damping = 0f)
	{
		if (source == null)
		{
			object obj;
			if (gameCamera != null)
			{
				IDirectableCamera directableCamera = gameCamera;
				obj = directableCamera;
			}
			else
			{
				obj = current;
			}
			source = (IDirectableCamera)obj;
		}
		if (target == null)
		{
			target = current;
		}
		Vector3 b = ((!(weight < 1f)) ? target.position : Easing.Ease(interpolation, source.position, target.position, weight));
		Quaternion b2 = ((!(weight < 1f)) ? target.rotation : Easing.Ease(interpolation, source.rotation, target.rotation, weight));
		float b3 = ((!(weight < 1f)) ? target.fieldOfView : Easing.Ease(interpolation, source.fieldOfView, target.fieldOfView, weight));
		float b4 = ((!(weight < 1f)) ? target.focalPoint : Easing.Ease(interpolation, source.focalPoint, target.focalPoint, weight));
		float b5 = ((!(weight < 1f)) ? target.focalRange : Easing.Ease(interpolation, source.focalRange, target.focalRange, weight));
		bool flag = target != lastTargetShot;
		if (!flag && Application.isPlaying && damping > 0f)
		{
			current.position = Vector3.Lerp(current.position, b, UpdateManager.deltaTime * damping);
			current.rotation = Quaternion.Lerp(current.rotation, b2, UpdateManager.deltaTime * damping);
			current.fieldOfView = Mathf.Lerp(current.fieldOfView, b3, UpdateManager.deltaTime * damping);
			current.focalPoint = Mathf.Lerp(current.focalPoint, b4, UpdateManager.deltaTime * damping);
			current.focalRange = Mathf.Lerp(current.focalRange, b5, UpdateManager.deltaTime * damping);
		}
		else
		{
			current.position = b;
			current.rotation = b2;
			current.fieldOfView = b3;
			current.focalPoint = b4;
			current.focalRange = b5;
		}
		if (flag && DirectorCamera.OnCut != null)
		{
			DirectorCamera.OnCut(target);
		}
		lastTargetShot = target;
	}

	private static DirectorCamera FindDirectorCamera()
	{
		int num = 0;
		num = SceneManager.sceneCount;
		for (int i = 0; i < num; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (!sceneAt.IsValid() || !sceneAt.isLoaded)
			{
				continue;
			}
			GameObject[] rootGameObjects = sceneAt.GetRootGameObjects();
			for (int j = 0; j < rootGameObjects.Length; j++)
			{
				DirectorCamera[] componentsInChildren = rootGameObjects[j].GetComponentsInChildren<DirectorCamera>(includeInactive: true);
				if (componentsInChildren != null && componentsInChildren.Length > 0)
				{
					return componentsInChildren[0];
				}
			}
		}
		return null;
	}

	GameObject IDirectableCamera.get_gameObject()
	{
		return base.gameObject;
	}
}
