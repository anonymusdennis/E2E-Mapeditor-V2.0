using System;
using System.Collections.Generic;
using UnityEngine;

public class T17TooltipManager : T17MonoBehaviour
{
	[Serializable]
	public struct PlatformOverride
	{
		public Platform.PlatformOverride PlatformToOverride;

		public GameObject OverrideToolTipPrefab;
	}

	public static T17TooltipManager Instance;

	public GameObject m_ToolTipPrefab;

	private List<T17ItemTooltip> m_ItemTooltips = new List<T17ItemTooltip>();

	[Tooltip("Set to test an override for a different platform to what the editor is set to. If set to unknown then will use the platform the editor is set to. Only works when running in the editor")]
	[SerializeField]
	public List<PlatformOverride> m_PlatformOverrides;

	public Platform.PlatformOverride m_TestPlatformOverride;

	protected override void Awake()
	{
		base.Awake();
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	protected virtual void OnDestroy()
	{
		m_ToolTipPrefab = null;
		m_ItemTooltips.Clear();
		m_PlatformOverrides.Clear();
		if (Instance != null)
		{
			Instance = null;
		}
	}

	private void Start()
	{
		GameObject original = m_ToolTipPrefab;
		Platform.PlatformOverride platformOverride = Platform.PlatformOverride.None;
		if (m_PlatformOverrides.Count > 0)
		{
			for (int i = 0; i < m_PlatformOverrides.Count; i++)
			{
				if (m_PlatformOverrides[i].OverrideToolTipPrefab != null && m_PlatformOverrides[i].PlatformToOverride == Platform.PlatformOverride.Standalone)
				{
					original = m_PlatformOverrides[i].OverrideToolTipPrefab;
					platformOverride = m_PlatformOverrides[i].PlatformToOverride;
					break;
				}
			}
		}
		for (int j = 0; j < 10; j++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(original);
			gameObject.transform.SetParent(base.transform, worldPositionStays: true);
			gameObject.SetActive(value: false);
			gameObject.name = "Tooltip" + j;
			T17ItemTooltip component = gameObject.GetComponent<T17ItemTooltip>();
			if (component != null)
			{
				component.m_PlatformOverride = platformOverride;
				m_ItemTooltips.Add(component);
			}
		}
	}

	public T17ItemTooltip GetTooltip()
	{
		int num = -1;
		for (int i = 0; i < m_ItemTooltips.Count; i++)
		{
			T17ItemTooltip t17ItemTooltip = m_ItemTooltips[i];
			if (t17ItemTooltip == null || t17ItemTooltip.gameObject == null)
			{
				m_ItemTooltips.Remove(t17ItemTooltip);
				i--;
			}
			else if (!t17ItemTooltip.m_IsTaken)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			m_ItemTooltips[num].m_IsTaken = true;
			return m_ItemTooltips[num];
		}
		return null;
	}

	public void ReleaseTooltip(T17ItemTooltip tooltip)
	{
		if (tooltip != null)
		{
			tooltip.SetDelegateToHoldersRewiredId(null);
			tooltip.m_IsTaken = false;
			if (m_ItemTooltips.Remove(tooltip))
			{
				m_ItemTooltips.Add(tooltip);
			}
		}
	}

	private void LateUpdate()
	{
		for (int num = m_ItemTooltips.Count - 1; num >= 0; num--)
		{
			if (null != m_ItemTooltips[num] && null != m_ItemTooltips[num].gameObject && !m_ItemTooltips[num].m_IsTaken && m_ItemTooltips[num].gameObject.activeInHierarchy)
			{
				m_ItemTooltips[num].gameObject.SetActive(value: false);
				m_ItemTooltips[num].transform.SetParent(base.transform);
			}
		}
	}
}
