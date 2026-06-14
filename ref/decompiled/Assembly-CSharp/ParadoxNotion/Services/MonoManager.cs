using System;
using UnityEngine;

namespace ParadoxNotion.Services;

public class MonoManager : MonoBehaviour
{
	public enum UpdateMode
	{
		Auto,
		Manual
	}

	private static bool isQuiting;

	private static MonoManager _current;

	public static UpdateMode updateMode
	{
		get
		{
			return (!current.enabled) ? UpdateMode.Manual : UpdateMode.Auto;
		}
		set
		{
			current.enabled = value == UpdateMode.Auto;
		}
	}

	public static MonoManager current
	{
		get
		{
			if (_current == null && !isQuiting)
			{
				_current = UnityEngine.Object.FindObjectOfType<MonoManager>();
				if (_current == null)
				{
					_current = new GameObject("_MonoManager").AddComponent<MonoManager>();
				}
			}
			return _current;
		}
	}

	public event Action onUpdate;

	public event Action onLateUpdate;

	public event Action onFixedUpdate;

	public event Action onGUI;

	public event Action onApplicationQuit;

	public event Action<bool> onApplicationPause;

	public static void Create()
	{
		_current = current;
	}

	private void OnApplicationQuit()
	{
		isQuiting = true;
		if (this.onApplicationQuit != null)
		{
			this.onApplicationQuit();
		}
	}

	private void OnApplicationPause(bool isPause)
	{
		if (this.onApplicationPause != null)
		{
			this.onApplicationPause(isPause);
		}
	}

	private void Awake()
	{
		if (_current != null && _current != this)
		{
			UnityEngine.Object.DestroyImmediate(base.gameObject);
			return;
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		_current = this;
	}

	public void Update()
	{
		if (this.onUpdate != null)
		{
			this.onUpdate();
		}
	}

	public void LateUpdate()
	{
		if (this.onLateUpdate != null)
		{
			this.onLateUpdate();
		}
	}

	public void FixedUpdate()
	{
		if (this.onFixedUpdate != null)
		{
			this.onFixedUpdate();
		}
	}

	public void OnGUI()
	{
		if (this.onGUI != null)
		{
			this.onGUI();
		}
	}
}
