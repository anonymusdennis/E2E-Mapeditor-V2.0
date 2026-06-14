using System.Collections.Generic;
using UnityEngine;

public class UIShimmerManager : T17MonoBehaviour
{
	private static UIShimmerManager m_Instance;

	private List<UIButtonShine> m_EnabledShines = new List<UIButtonShine>();

	public float m_TimeBetweenShines = 3f;

	private float m_ShineCountdown;

	protected override void Awake()
	{
		base.Awake();
		if (m_Instance != null)
		{
			Object.Destroy(this);
		}
		else
		{
			Object.DontDestroyOnLoad(base.gameObject);
			m_Instance = this;
		}
		m_ShineCountdown = m_TimeBetweenShines;
		base.enabled = false;
	}

	private void Update()
	{
		m_ShineCountdown -= Time.deltaTime;
		if (m_ShineCountdown <= 0f)
		{
			m_ShineCountdown = m_TimeBetweenShines;
			if (m_EnabledShines.Count != 0)
			{
				UIButtonShine uIButtonShine = m_EnabledShines[Random.Range(0, m_EnabledShines.Count)];
				uIButtonShine.Shine();
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	public void RegisterActiveShine(UIButtonShine newShine)
	{
		if (!m_EnabledShines.Contains(newShine))
		{
			m_EnabledShines.Add(newShine);
			newShine.Disable();
			if (!base.enabled)
			{
				base.enabled = true;
			}
		}
	}

	public static void Register(UIButtonShine newShine)
	{
		if (m_Instance == null)
		{
			CreateManager();
		}
		m_Instance.RegisterActiveShine(newShine);
	}

	public void UnRegisterInactiveShine(UIButtonShine oldShine)
	{
		m_EnabledShines.Remove(oldShine);
		if (base.enabled && m_EnabledShines.Count == 0)
		{
			base.enabled = false;
		}
	}

	public static void UnRegister(UIButtonShine oldShine)
	{
		if (m_Instance == null)
		{
			CreateManager();
		}
		m_Instance.UnRegisterInactiveShine(oldShine);
	}

	public UIShimmerManager GetInstance()
	{
		return m_Instance;
	}

	private static void CreateManager()
	{
		new GameObject("UIShimmerManager", typeof(UIShimmerManager));
	}
}
